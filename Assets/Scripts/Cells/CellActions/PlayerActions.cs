using System;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Commands;
using UnityEngine;

namespace Aspekt.Hex
{
    public class PlayerActions : IInputObserver, IUnitActionObserver
    {
        private readonly CellIndicator indicator;
        private readonly GameManager game;
        private readonly NetworkGamePlayerHex player;
        private readonly PlayerInput input;

        private enum States
        {
            None,
            Building,
            Moving,
            Attacking,
            OwnUnitSelected,
        }

        private States state;

        private bool isPlayerTurn;
        private HexCell selectedCell;
        private HexCell builder;
        private UnitCell unit;
        private Cells.CellTypes cellBuildType;

        private HexCoordinates currentCoords;
        
        public PlayerActions(NetworkGamePlayerHex player, GameManager game)
        {
            this.player = player;
            this.game = game;
            
            indicator = new CellIndicator(game.Cells, game.UI);
            
            input = new PlayerInput();
            input.RegisterObserver(this);
            
            state = States.None;
        }
        
        public void Update()
        {
            input.HandleInput();
            
            var boardPosition = input.GetMousePositionOnBoard();
            var coords = HexCoordinates.FromPosition(boardPosition);
            
            if (coords.Equals(currentCoords)) return;
            currentCoords = coords;

            if (isPlayerTurn)
            {
                UpdateCursor(coords);
            }
        }
        
        public void OnBoardClickedPrimary(Vector3 position)
        {
            var coords = HexCoordinates.FromPosition(position);
            switch (state)
            {
                case States.Moving:
                    MoveIfValid(unit, coords);
                    break;
                case States.Attacking:
                    AttackIfValid(unit, coords);
                    break;
                case States.OwnUnitSelected:
                    HandleUnitSelectedAction(coords);
                    break;
                case States.Building:
                    HandleBuildAction(coords);
                    break;
                default:
                    HandleBoardClick(coords);
                    break;
            }
        }
        
        public void OnBoardClickedSecondary(Vector3 position)
        {
            
        }

        public void OnCancelPressed()
        {
            indicator.Clear();
            if (state == States.OwnUnitSelected || state == States.None)
            {
                unit = null;
                game.UI.HideCellInfo();
            }

            if (state == States.Building)
            {
                game.UI.ShowCellInfo(builder);
            }
            state = States.None;
        }

        public void UpdatePlayerTurn(bool isPlayerTurn)
        {
            this.isPlayerTurn = isPlayerTurn;
            if (!isPlayerTurn)
            {
                indicator.Clear();
            }
            else if (unit != null)
            {
                RefreshOwnUnitActions();
            }
        }
        
        public void SetBuild(HexCell originator, Cells.CellTypes type)
        {
            state = States.Building;
            cellBuildType = type;
            builder = originator;
            game.UI.HideCellInfo();
            indicator.ShowBuild(type, player.ID, originator);
        }

        public void SetUnitAction(UnitCell unitCell, UnitAction unitAction)
        {
            switch (unitAction.actionType)
            {
                case UnitActions.Move:
                    SetUnitMove(unitCell);
                    break;
                case UnitActions.Attack:
                    SetUnitAttack(unitCell);
                    break;
                default:
                    Debug.Log("invalid unit action: " + unitAction.actionType);
                    break;
            }
        }

        private void SetUnitMove(UnitCell movingUnit)
        {
            unit = movingUnit;
            state = States.Moving;
            indicator.ShowMovementGrid(unit.Coordinates, unit.GetStats().Speed);
        }

        private void SetUnitAttack(UnitCell attackingUnit)
        {
            unit = attackingUnit;
            state = States.Attacking;
            indicator.ShowAttackRange(unit.Coordinates, unit.GetStats().Range);
        }

        public void OnFinishedMove(UnitCell unit)
        {
            if (unit == this.unit)
            {
                RefreshOwnUnitActions();
            }
        }

        public void OnFinishedAttack(UnitCell unit)
        {
            if (unit == this.unit)
            {
                RefreshOwnUnitActions();
            }
        }

        public void OnUnitRemoved(UnitCell unit)
        {
            if (unit == this.unit)
            {
                this.unit = null;
                indicator.Clear();
                state = States.None;
            }
        }

        private void SetOwnUnitSelected(UnitCell unit)
        {
            this.unit = unit;
            state = States.OwnUnitSelected;
            unit.RegisterActionObserver(this);
            RefreshOwnUnitActions();
        }

        private void RefreshOwnUnitActions()
        {
            if (!game.IsCurrentPlayer(player)) return;
            
            state = States.OwnUnitSelected;
            
            if (!unit.HasMoved)
            {
                indicator.ShowMovementGrid(unit.Coordinates, unit.GetStats().Speed);
            }

            if (!unit.HasAttacked)
            {
                indicator.ShowAttackableCells(unit, unit.GetStats().Range, false);
            }
            
            indicator.UpdateUnitCursor(unit, currentCoords);
        }

        private void UpdateCursor(HexCoordinates coords)
        {
            switch (state)
            {
                case States.Building:
                    indicator.UpdateBuildCursor(coords);
                    break;
                case States.OwnUnitSelected:
                    indicator.UpdateUnitCursor(unit, coords);
                    break;
                case States.Moving:
                    indicator.UpdateMoveCursor(unit, coords);
                    break;
                case States.Attacking:
                    indicator.UpdateAttackCursor(unit, coords);
                    break;
            }
        }

        private void HandleBuildAction(HexCoordinates coords)
        {
            if (indicator.IsProjectedCellInPlacementGrid())
            {
                indicator.Clear();
                player.CmdPlaceCell((Int16)coords.X, (Int16)coords.Z, (Int16)cellBuildType);
                state = States.None;
                game.UI.ShowCellInfo(builder);
            }
            else
            {
                // TODO display error message to player
            }
        }

        private void HandleUnitSelectedAction(HexCoordinates coords)
        {
            var isMoveValid = MoveIfValid(unit, coords);
            if (!isMoveValid)
            {
                var isAttackValid = AttackIfValid(unit, coords);
                if (!isAttackValid)
                {
                    HandleBoardClick(coords);
                }
            }
        }

        private void HandleBoardClick(HexCoordinates coords)
        {
            Clear();
            
            var cell = game.Cells.GetCellAtPosition(coords);
            if (cell != null)
            {
                if (cell is UnitCell u && u.PlayerId == player.ID)
                {
                    SetOwnUnitSelected(u);
                }

                selectedCell = cell;
                cell.SetSelected();
                game.UI.ShowCellInfo(cell);
            }
        }
        
        private bool MoveIfValid(UnitCell unit, HexCoordinates coords)
        {
            if (unit.HasMoved) return false;
            var path = game.Cells.GetPathWithValidityCheck(unit, coords, player.ID);
            var isValidPath = path != null;
            if (isValidPath)
            {
                state = States.None;
                indicator.Clear();
                player.CmdMoveCell(
                    (Int16) player.ID, (Int16) unit.ID, 
                    (Int16) coords.X, (Int16) coords.Z);
            }
            return isValidPath;
        }

        private bool AttackIfValid(UnitCell unit, HexCoordinates coords)
        {
            if (!game.IsCurrentPlayer(player) || unit.HasAttacked) return false;
            var target = game.Cells.GetCellAtPosition(coords);
            var isValid = game.Cells.IsValidAttackTarget(unit, target, player.ID);
            if (isValid)
            {
                state = States.None;
                indicator.Clear();

                var damage = ValidatedAttack.GetDamage(unit, target);
                var validatedAttack = new ValidatedAttack(player.GetNewActionID(), unit, target, damage);
                game.CommandValidator.RegisterAttack(validatedAttack);
                
                unit.ShowAttack(target, validatedAttack.OnAttackLanded);
                player.CmdAttackCell(
                    (Int16) player.ID, validatedAttack.ID,
                    (Int16) unit.ID, (Int16) target.ID
                );
            }
            return isValid;
        }

        private void Clear()
        {
            indicator.Clear();
            state = States.None;
            if (selectedCell != null)
            {
                selectedCell.SetUnselected();
                selectedCell = null;
            }

            ClearOwnUnit();
            builder = null;

            game.UI.HideCellInfo();
        }

        private void ClearOwnUnit()
        {
            if (unit == null) return;
            unit.UnregisterActionObserver(this);
            state = States.None;
            unit = null;
        }
    }
}