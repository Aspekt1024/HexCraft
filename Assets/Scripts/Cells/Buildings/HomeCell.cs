using UnityEngine;

namespace Aspekt.Hex
{
    public class HomeCell : BuildingCell, ISuppliesGenerator, IProductionGenerator
    {
#pragma warning disable 649
        [SerializeField] private GameObject townHall;
        [SerializeField] private GameObject keep;
        [SerializeField] private GameObject castle;
#pragma warning disable 649
        
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

        public override void OnTechAdded(Technology tech)
        {
            if (tech == Technology.UpgradeTownHall1)
            {
                townHall.SetActive(false);
                keep.SetActive(true);
                castle.SetActive(false);
            }
            else if (tech == Technology.UpgradeTownHall2)
            {
                townHall.SetActive(false);
                keep.SetActive(false);
                castle.SetActive(true);
            }
        }
    }
}