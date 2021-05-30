using UnityEngine;

namespace Aspekt.Hex.Behaviours
{
    [CreateAssetMenu(menuName = "Hex/Cells/Default Building Behaviour", fileName = "BuildingBehaviour_Default")]
    public class DefaultBuildingBehaviour : BuildingBehaviour
    {
        public override int CalculateSupplies(BuildingCell cell, PlayerData data)
        {
            return cell.GetCurrencyBonus().supplies;
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