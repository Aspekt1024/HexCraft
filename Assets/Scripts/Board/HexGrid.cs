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

        public override void OnStartClient()
        {
            var room = FindObjectOfType<NetworkManagerHex>();
            game = FindObjectOfType<GameManager>();
            room.RegisterBoard(this);
            cells = FindObjectOfType<Cells>();
            cells.Init(this);
            boardHalfSizeWorldUnits = GetBoardLimitsInWorldUnits();
        }

        public void SetStartingLocation(NetworkGamePlayerHex player)
        {
            var startLocation = GetStartLocation(player.ID);
            TryPlace((Int16)startLocation.X, (Int16)startLocation.Z, (Int16)player.ID, Cells.CellTypes.Base);
        }

        public HexCoordinates GetStartLocation(int playerId)
        {
            return playerId == 1 ? startLocation1 : startLocation2;
        }

        public bool TryPlace(Int16 x, Int16 z, int playerID, Cells.CellTypes type)
        {
            var coords = new HexCoordinates(x, z);
            if (!cells.IsValidPlacement(type, coords, playerID)) return false;
            
            RpcCreateCell(x, z, (Int16)playerID, (Int16)type);
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
        private void RpcCreateCell(Int16 x, Int16 z, Int16 playerID, Int16 cellTypeIndex)
        {
            if (!Enum.IsDefined(typeof(Cells.CellTypes), (Int32)cellTypeIndex))
            {
                Debug.LogError("invalid cell type");
                return;
            }
            
            var coords = new HexCoordinates(x, z);
            var cell = cells.Create((Cells.CellTypes) cellTypeIndex);
            var player = game.GetPlayerFromID(playerID);
            cell.Place(coords, cells.GetColour(player.ID), player);
        }

        [ClientRpc]
        public void RpcRemoveCell(Int16 x, Int16 z, Int16 playerID)
        {
            var coords = new HexCoordinates(x, z);
            cells.RemoveCell(coords);
        }

        [ClientRpc]
        public void RpcMoveCell(Int16 fromX, Int16 fromZ, Int16 toX, Int16 toZ)
        {
            var cell = cells.GetCellAtPosition(new HexCoordinates(fromX, fromZ));
            if (cell == null) return;
            
            var newCoords = new HexCoordinates(toX, toZ);
            cell.MoveTo(newCoords);
        }
        
        [ClientRpc]
        public void RpcAttack(Int16 attackerX, Int16 attackerZ, Int16 targetX, Int16 targetZ, Int16 damage)
        {
            var attacker = cells.GetCellAtPosition(new HexCoordinates(attackerX, attackerZ));
            if (attacker == null || !(attacker is UnitCell attackingUnit)) return;

            var target = cells.GetCellAtPosition(new HexCoordinates(targetX, targetZ));
            if (target == null) return;

            attackingUnit.ShowAttack(target, damage);
        }
    }
}