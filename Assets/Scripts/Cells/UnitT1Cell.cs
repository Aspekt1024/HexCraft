using System.Collections.Generic;
using Aspekt.Hex.UI;

namespace Aspekt.Hex
{
    public class UnitT1Cell : HexCell
    {
        public override string DisplayName { get; } = "Unit (T1)";

        public override List<CellUIItem.Details> ItemDetails { get; protected set; }

        private void Awake()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
            };
        }
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return false;
        }
    }
}