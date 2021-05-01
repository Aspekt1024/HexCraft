using UnityEngine;

namespace Aspekt.Hex
{
    public class HomeCell : BuildingCell, ISuppliesGenerator, IProductionGenerator
    {
        public override Technology Technology { get; } = Technology.TownHall;
        public int production = 1;
        public int suppliesPerRound = 2;

        public Transform GetTransform() => transform;

        protected override void OnInit()
        {
            Stats.Production = production;
            Stats.Supplies = suppliesPerRound;
        }

        public int GetSupplies(PlayerData data)
        {
            return suppliesPerRound;
        }

        public int GetProduction()
        {
            return production;
        }
    }
}