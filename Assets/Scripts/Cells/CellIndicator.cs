using UnityEngine;

namespace Aspekt.Hex
{
    public class CellIndicator
    {
        public bool IsPlacingCell => cell != null;
        public Cells.CellTypes CellType { get; private set; }

        private HexCell cell;

        private readonly Cells cells;

        public CellIndicator(Cells cells)
        {
            this.cells = cells;
        }

        public void Show(Cells.CellTypes type, int playerID)
        {
            if (cell != null)
            {
                Object.Destroy(cell.gameObject);
            }

            CellType = type;
            cell = cells.Create(type);
            cell.DisplayAsIndicator(cells.HoloMaterial, cells.GetColour(playerID));
        }

        public void Hide()
        {
            if (cell == null) return;
            Object.Destroy(cell.gameObject);
            cell = null;
        }
        
        public void Update(Vector3 boardPosition)
        {
            if (cell == null) return;
            cell.SetCoordinates(HexCoordinates.FromPosition(boardPosition));
        }
    }
}