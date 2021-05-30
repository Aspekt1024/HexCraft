using UnityEngine;

namespace Aspekt.Hex.Behaviours
{
    [CreateAssetMenu(menuName = "Hex/Cells/Market Building Behaviour", fileName = "BuildingBehaviour_Market")]
    public class MarketBehaviour : BuildingBehaviour
    {
        public override int CalculateSupplies(BuildingCell cell, PlayerData data)
        {
            return data.CurrencyData.MaxProduction - data.CurrencyData.UtilisedProduction;
        }

        public override int CalculateProduction(BuildingCell cell, PlayerData data)
        {
            return cell.GetCurrencyBonus().production;
        }

        public override int CalculatePopulation(BuildingCell cell, PlayerData data)
        {
            return cell.GetCurrencyBonus().production;
        }
    }
}