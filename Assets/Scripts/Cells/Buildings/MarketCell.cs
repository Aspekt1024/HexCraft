using UnityEngine;

namespace Aspekt.Hex
{
    public class MarketCell : BuildingCell
    {
        public override Technology Technology { get; } = Technology.Market;
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return false;
        }

        public override void OnTechAdded(Technology tech)
        {
            
        }

        public override void OnTechRemoved(Technology tech)
        {
            
        }
    }
}