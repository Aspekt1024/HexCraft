using UnityEngine;

namespace Aspekt.Hex
{
    public class IncomeCell : BuildingCell, IProductionGenerator
    {
        public override Technology Technology { get; } = Technology.Farm;
        
        [Header("Income Settings")]
        public int production = 1;

        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return false;
        }

        protected override void OnInit()
        {
            Stats.Production = production;
        }

        public int GetProduction()
        {
            return production;
        }

        public override void OnTechAdded(Technology tech)
        {
            
        }

        public override void OnTechRemoved(Technology tech)
        {
            
        }
    }
}