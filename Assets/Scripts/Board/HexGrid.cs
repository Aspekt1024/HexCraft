using System;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HexGrid : NetworkBehaviour
    {
        public HexCoordinates startLocation1;
        public HexCoordinates startLocation2;
        public Vector2Int boardHalfSize;

        private GameManager game;
        private Cells cells;

        private Vector3 boardHalfSizeWorldUnits;

        public Vector3 GetBoardLimitsInWorldUnits()
        {
            var lengthCoord = new HexCoordinates(boardHalfSize.x, 0);
            var heightCoord = new HexCoordinates(0, boardHalfSize.y);
            var lengthPos = HexCoordinates.ToPosition(lengthCoord);
            var heightPos = HexCoordinates.ToPosition(heightCoord);
            return new Vector3(lengthPos.x, 0f, heightPos.z);
        }

        public void Init(GameManager game)
        {
            this.game = game;
            cells = game.Cells;
        }

        public override void OnStartClient()
        {
            var room = FindObjectOfType<NetworkManagerHex>();
            room.RegisterBoard(this);
            boardHalfSizeWorldUnits = GetBoardLimitsInWorldUnits();
        }

        public void SetStartingLocation(NetworkGamePlayerHex player)
        {
            var startLocation = GetStartLocation(player.ID);
            TryPlace((Int16)startLocation.X, (Int16)startLocation.Z, (Int16)player.ID, Cells.CellTypes.Home);
        }

        public HexCoordinates GetStartLocation(int playerId)
        {
            return playerId == 1 ? startLocation1 : startLocation2;
        }

        public bool TryPlace(Int16 x, Int16 z, int playerID, Cells.CellTypes type)
        {
            var coords = new HexCoordinates(x, z);
            if (!cells.IsValidPlacement(type, coords, playerID)) return false;
            
            var cellID = cells.GetUniqueCellID();
            RpcCreateCell(x, z, (Int16)playerID, (Int16)type, (Int16)cellID);
            return true;
        }

        public bool IsWithinGridBoundary(int x, int z)
        {
            return IsWithinGridBoundary(new HexCoordinates(x, z));
        }
        
        public bool IsWithinGridBoundary(HexCoordinates coords)
        {
            var pos = HexCoordinates.ToPosition(coords);
            return Mathf.Abs(pos.x) < boardHalfSizeWorldUnits.x
                && Mathf.Abs(pos.z) < boardHalfSizeWorldUnits.z;
        }

        [ClientRpc]
        private void RpcCreateCell(Int16 x, Int16 z, Int16 playerID, Int16 cellTypeIndex, Int16 cellID)
        {
            if (!Enum.IsDefined(typeof(Cells.CellTypes), (Int32)cellTypeIndex))
            {
                Debug.LogError("invalid cell type");
                return;
            }
            
            var coords = new HexCoordinates(x, z);
            var player = game.GetPlayerFromID(playerID);
            var cell = cells.Create((Cells.CellTypes) cellTypeIndex, player, cellID);
            cell.Place(cellID, coords);
        }

        [ClientRpc]
        public void RpcMoveCell(Int16 cellID, Int16 toX, Int16 toZ)
        {
            var cell = cells.GetCell(cellID);
            if (cell == null) return;
            
            var newCoords = new HexCoordinates(toX, toZ);
            cell.MoveTo(newCoords);
        }
        
        [ClientRpc]
        public void RpcAttack(Int16 playerID, Int16 actionID, Int16 attackerID, Int16 targetID, Int16 damage, bool isDestroyed)
        {
            var attacker = (UnitCell)cells.GetCell(attackerID);
            var target = cells.GetCell(targetID);

            if (playerID == game.PlayerID)
            {
                game.CommandValidator.OnAttackReceived(actionID, attacker, target, damage, isDestroyed);
            }
            else if (attacker != null && attacker is UnitCell attackingUnit)
            {
                attackingUnit.ShowAttack(
                    target,
                    () =>
                    {
                        target.ShowDamage(attacker, damage);
                        target.RemoveHealth(damage);
                        if (isDestroyed)
                        {
                            cells.RemoveCell(target);
                        }
                        attacker.AttackComplete();
                    }
                );
            }
        }
    }
}