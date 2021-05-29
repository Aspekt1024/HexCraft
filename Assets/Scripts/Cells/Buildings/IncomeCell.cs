using UnityEngine;

namespace Aspekt.Hex
{
    public class IncomeCell : BuildingCell, IProductionGenerator, ISuppliesGenerator
    {
        public Transform GetTransform() => transform;
        
        [Header("Income Settings")]
        public int production = 1;
        public int suppliesPerRound = 0;

        protected override void OnInit()
        {
            Stats.Production = production;
        }

        public int GetProduction()
        {
            return production;
        }
        
        public int GetSupplies(PlayerData data)
        {
            return suppliesPerRound;
        }

        public override void OnTechAdded(Technology tech)
        {
            
        }

        public override void OnTechRemoved(Technology tech)
        {
            
        }
    }
}