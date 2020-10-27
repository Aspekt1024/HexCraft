using System;
using Aspekt.Hex.UI;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    public class NetworkGamePlayerHex : NetworkBehaviour, IInputObserver, ICellEventObserver, ControlPanel.IEventReceiver
    {
        public bool IsReady { get; private set; }
        
        [SyncVar(hook = nameof(HandleDisplayNameChanged))]
        private string displayName;
        
        [SyncVar] public int ID;

        [SyncVar(hook = nameof(HandleCurrentPlayerChanged))]
        public bool IsCurrentPlayer = false;
        
        private NetworkManagerHex room;
        private GameManager game;
        private PlayerInput input;

        private CellIndicator indicator;

        public string DisplayName => displayName;
        
        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);

            room = FindObjectOfType<NetworkManagerHex>();
            game = room.Game;

            room.AddGamePlayer(this);
            
            if (hasAuthority)
            {
                game.SetGamePlayer(this);
            
                input = new PlayerInput();
                input.RegisterNotify(this);

                indicator = new CellIndicator(FindObjectOfType<Cells>(), game.UI);
            }
        }
        
        public override void OnStopClient()
        {
            room.RemoveGamePlayer(this);
        }

        [Server]
        public void SetPlayerID(int id)
        {
            ID = id;
        }
        
        [Server]
        public void SetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        private void Update()
        {
            if (!hasAuthority || !game.IsRunning()) return;
            
            input.HandleInput();
            indicator.Update(input.GetMousePositionOnBoard());
        }

        public void BoardClickedPrimary(Vector3 position)
        {
            var coords = HexCoordinates.FromPosition(position);
            if (indicator.IsPlacingCell)
            {
                if (indicator.IsProjectedCellInPlacementGrid())
                {
                    CmdPlaceCell((Int16)coords.X, (Int16)coords.Z, (Int16)indicator.CellType);
                }
                else
                {
                    // TODO display error message to player
                }
            }
            else if (indicator.IsMovingUnit)
            {
                var unit = indicator.GetMovingUnit();
                if (game.Cells.IsValidMove(unit, coords, ID))
                {
                    CmdMoveCell((Int16) ID,
                        (Int16) unit.Coordinates.X, (Int16) unit.Coordinates.Z, 
                        (Int16) coords.X, (Int16) coords.Z);
                }
            }
            else if (indicator.IsAttacking)
            {
                var unit = indicator.GetAttackingUnit();
                var target = game.Cells.GetCellAtPosition(coords);
                if (game.Cells.IsValidAttackTarget(unit, target, ID))
                {
                    CmdAttackCell((Int16) ID,
                        (Int16) unit.Coordinates.X, (Int16) unit.Coordinates.Z, 
                        (Int16) coords.X, (Int16) coords.Z);
                }
            }
            else
            {
                var cell = game.Cells.GetCellAtPosition(coords);
                if (cell != null)
                {
                    game.UI.ShowCellInfo(cell);
                }
                else
                {
                    game.UI.HideCellInfo();
                }
            }
        }

        public void BoardClickedSecondary(Vector3 position)
        {
            // TODO if unit selected, try move it, or attack target
            // TODO cancel actions
        }

        public void CancelPressed()
        {
            indicator.HideAll();
            game.UI.HideCellInfo();
        }

        public void OnEndTurnRequested()
        {
            CmdEndTurn();
        }

        private void HandleDisplayNameChanged(string oldName, string newName)
        {
            game.UI.UpdatePlayerInfo(room.GamePlayers);
        }

        private void HandleCurrentPlayerChanged(bool oldStatus, bool newStatus)
        {
            if (!hasAuthority) return;
            
            if (IsCurrentPlayer)
            {
                // TODO set UI to indicate to player
            }
            else
            {
                indicator.HideAll();
            }
        }

        public void IndicateBuildCell(Cells.CellTypes type, HexCell originator)
        {
            if (!IsCurrentPlayer) return;
            
            game.UI.HideCellInfo();
            indicator.ShowBuild(type, ID, originator);
        }

        public void UpgradeCell(HexCell originator)
        {
            // TODO upgrade
            Debug.Log("upgrade cell " + originator.DisplayName);
        }

        public void IndicateUnitAttack(UnitCell unit)
        {
            if (!IsCurrentPlayer) return;
            
            game.UI.HideCellInfo();
            indicator.IndicateAttack(unit);
        }

        public void IndicateUnitMove(UnitCell unit)
        {
            if (!IsCurrentPlayer) return;
            
            game.UI.HideCellInfo();
            indicator.ShowMoveRange(unit);
        }
        
        
        [Command]
        private void CmdPlaceCell(Int16 x, Int16 z, Int16 cellTypeIndex)
        {
            if (Enum.IsDefined(typeof(Cells.CellTypes), (Int32)cellTypeIndex))
            {
                var cellType = (Cells.CellTypes) cellTypeIndex;
                game.TryPlace(this, x, z, cellType);
            }
        }

        [Command]
        private void CmdAttackCell(Int16 playerId, Int16 originX, Int16 originZ, Int16 targetX, Int16 targetZ)
        {
            var attackingCell = game.Cells.GetCellAtPosition(new HexCoordinates(originX, originZ));
            if (attackingCell == null || !(attackingCell is UnitCell attackingUnit)) return;
            var target = game.Cells.GetCellAtPosition(new HexCoordinates(targetX, targetZ));
            if (game.Cells.IsValidAttackTarget(attackingUnit, target, playerId))
            {
                game.AttackCell(attackingUnit, target);
            }
        }
        
        [Command]
        private void CmdMoveCell(Int16 playerId, Int16 originX, Int16 originZ, Int16 targetX, Int16 targetZ)
        {
            var movingUnit = game.Cells.GetCellAtPosition(new HexCoordinates(originX, originZ));
            var target = new HexCoordinates(targetX, targetZ);
            if (game.Cells.IsValidMove(movingUnit, target, playerId))
            {
                game.MoveCell(movingUnit.Coordinates, target);
            }
        }
        
        [Command]
        private void CmdRemoveCell(Int16 x, Int16 z)
        {
            game.TryRemove(this, x, z);
        }

        [Command]
        public void CmdSetReady()
        {
            IsReady = true;
            room.UpdatePlayerReady();
        }

        [Command]
        private void CmdEndTurn()
        {
            game.Data.NextTurn();
        }
    }
}