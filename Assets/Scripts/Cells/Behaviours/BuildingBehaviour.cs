using UnityEngine;

namespace Aspekt.Hex.Behaviours
{
    public abstract class BuildingBehaviour : ScriptableObject
    {
        public abstract int CalculateSupplies(BuildingCell cell, PlayerData data);

        public abstract int CalculateProduction(BuildingCell cell, PlayerData data);

        public abstract int CalculatePopulation(BuildingCell cell, PlayerData data);
    }
}