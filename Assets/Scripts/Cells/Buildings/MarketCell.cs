using UnityEngine;

namespace Aspekt.Hex
{
    public class MarketCell : BuildingCell, ISuppliesGenerator
    {
        public override Technology Technology { get; } = Technology.Market;
        
        public Transform GetTransform() => transform;
        
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

        public int GetSupplies()
        {
            var data = GameData.GetPlayerData(Owner);
            return data.CurrencyData.MaxProduction - data.CurrencyData.UtilisedProduction;
        }

    }
}