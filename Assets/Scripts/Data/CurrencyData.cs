using System;
using System.Collections.Generic;

namespace Aspekt.Hex
{
    [Serializable]
    public struct Cost
    {
        public int production;
        public int supplies;
    }
    
    public class CurrencyData
    {
        public int MaxProduction;
        public int UtilisedProduction;
        public int Supplies;

        public int AvailableProduction => MaxProduction - UtilisedProduction;
        
        private readonly PlayerData playerData;
        
        public interface IObserver
        {
            void OnMaxProductionChanged(NetworkGamePlayerHex player, int newProduction);
            void OnProductionUtilisationChanged(NetworkGamePlayerHex player, int newUtilisation);
            void OnSuppliesChanged(NetworkGamePlayerHex player, int newSupplies);
        }
        
        private readonly List<IObserver> observers = new List<IObserver>();

        public CurrencyData(PlayerData playerData)
        {
            this.playerData = playerData;
        }
        
        public void RegisterObserver(IObserver observer) => observers.Add(observer);
        public void UnregisterObserver(IObserver observer) => observers.Remove(observer);
        
        public bool CanAfford(Cost cost)
        {
            return cost.production <= MaxProduction - UtilisedProduction && cost.supplies <= Supplies;
        }

        public void ModifyMaxProduction(int productionDelta)
        {
            observers.ForEach(o => o.OnMaxProductionChanged(playerData.Player, MaxProduction + productionDelta));
        }

        public void Purchase(Cost cost)
        {
            // This instructs the game data to update currency values based on changes in production,
            // currency values are not changed directly here.
            if (cost.production > 0)
            {
                var newUtilisation = UtilisedProduction + cost.production;
                observers.ForEach(o => o.OnProductionUtilisationChanged(playerData.Player, newUtilisation));
            }

            if (cost.supplies > 0)
            {
                var newSupplies = Supplies - cost.supplies;
                observers.ForEach(o => o.OnSuppliesChanged(playerData.Player, newSupplies));
            }
        }
    }
}