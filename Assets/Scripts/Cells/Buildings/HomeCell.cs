
using UnityEngine;

namespace Aspekt.Hex
{
    public class HomeCell : BuildingCell, ISuppliesGenerator, IProductionGenerator
    {
        public override Technology Technology { get; } = Technology.None;
        public int production = 1;
        public int suppliesPerRound = 2;

        public Transform GetTransform() => transform;
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.Training
                   || cellType == Cells.CellTypes.Income
                   || cellType == Cells.CellTypes.Blacksmith
                   || cellType == Cells.CellTypes.Market;
        }

        protected override void OnInit()
        {
            Stats.Production = production;
            Stats.Supplies = suppliesPerRound;
        }

        public int GetSupplies()
        {
            return suppliesPerRound;
        }

        public int GetProduction()
        {
            return production;
        }
    }
}