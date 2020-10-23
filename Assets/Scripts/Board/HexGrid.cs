using System;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HexGrid : NetworkBehaviour
    {
        [Serializable]
        public struct HexLocation
        {
            public int x;
            public int z;
        }
        
        public HexCoordinates startLocation1;
        public HexCoordinates startLocation2;

        private Cells cells;
        
        public override void OnStartClient()
        {
            var room = FindObjectOfType<NetworkManagerHex>();
            room.RegisterBoard(this);
            cells = FindObjectOfType<Cells>();
        }

        public void SetStartingLocation(NetworkGamePlayerHex player)
        {
            if (player.ID == 1)
            {
                TryPlace((Int16)startLocation1.X, (Int16)startLocation1.Z, (Int16)player.ID, Cells.CellTypes.Base);
            }
            else
            {
                TryPlace((Int16)startLocation2.X, (Int16)startLocation2.Z, (Int16)player.ID, Cells.CellTypes.Base);
            }
        }

        public bool TryPlace(Int16 x, Int16 z, int playerID, Cells.CellTypes type)
        {
            var coords = new HexCoordinates(x, z);
            if (!cells.IsValidPlacement(type, coords, playerID)) return false;
            
            RpcCreateCell(x, z, (Int16)playerID, (Int16)type);
            return true;
        }

        public bool TryRemove(Int16 x, Int16 z, Int16 playerID)
        {
            var coords = new HexCoordinates(x, z);
            if (!cells.IsPieceInCell(coords)) return false;
            RpcRemoveCell(x, z, playerID);
            return true;
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
            cell.Place(coords, cells.GetColour(playerID), playerID);
        }

        [ClientRpc]
        private void RpcRemoveCell(Int16 x, Int16 z, Int16 playerID)
        {
            var coords = new HexCoordinates(x, z);
            cells.RemoveCell(coords);
        }
    }
}