using UnityEngine;

namespace Aspekt.Hex
{
    public interface IProductionGenerator
    {
        int GetProduction();
        Transform GetTransform();
    }

    public interface ISuppliesGenerator
    {
        int GetSupplies(PlayerData data);
        Transform GetTransform();
    }
}