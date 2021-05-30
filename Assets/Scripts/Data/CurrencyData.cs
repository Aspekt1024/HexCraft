using System;
using System.Collections.Generic;

namespace Aspekt.Hex
{
    [Serializable]
    public struct Currency
    {
        public int production;
        public int supplies;
        public int population;
    }
    
    public class CurrencyData
    {
        public int MaxProduction;
        public int UtilisedProduction;
        public int Supplies;

        public int MaxPopulation;
        public int UtilisedPopulation;

        public int AvailableProduction => MaxProduction - UtilisedProduction;
        
        private readonly PlayerData playerData;
        
        public interface IObserver
        {
            void OnMaxProductionChanged(NetworkGamePlayerHex player, int newProduction);
            void OnMaxPopulationChanged(NetworkGamePlayerHex player, int newPopulation);
            void OnProductionUtilisationChanged(NetworkGamePlayerHex player, int newUtilisation);
            void OnPopulationUtilisationChanged(NetworkGamePlayerHex player, int newUtilisation);
            void OnSuppliesChanged(NetworkGamePlayerHex player, int newSupplies);
        }
        
        private readonly List<IObserver> observers = new List<IObserver>();

        public CurrencyData(PlayerData playerData)
        {
            this.playerData = playerData;
        }
        
        public void RegisterObserver(IObserver observer) => observers.Add(observer);
        public void UnregisterObserver(IObserver observer) => observers.Remove(observer);
        
        public bool CanAfford(Currency cost)
        {
            return cost.production <= MaxProduction - UtilisedProduction
                   && cost.supplies <= Supplies
                   && (cost.population == 0 || cost.population <= MaxPopulation - UtilisedPopulation);
        }

        public void ModifyMaxProduction(int productionDelta)
        {
            observers.ForEach(o => o.OnMaxProductionChanged(playerData.Player, MaxProduction + productionDelta));
        }

        public void ModifyMaxPopulation(int populationDelta)
        {
            observers.ForEach(o => o.OnMaxPopulationChanged(playerData.Player, MaxPopulation + populationDelta));
        }

        public void Purchase(Currency cost)
        {
            // This instructs the game data to update currency values based on changes in production,
            // currency values are not changed directly here.
            if (cost.production > 0)
            {
                var newProdUtilisation = UtilisedProduction + cost.production;
                observers.ForEach(o => o.OnProductionUtilisationChanged(playerData.Player, newProdUtilisation));
            }

            if (cost.supplies > 0)
            {
                var newSupplies = Supplies - cost.supplies;
                observers.ForEach(o => o.OnSuppliesChanged(playerData.Player, newSupplies));
            }

            if (cost.population > 0)
            {
                var newPopUtilisation = UtilisedPopulation + cost.population;
                observers.ForEach(o => o.OnPopulationUtilisationChanged(playerData.Player, newPopUtilisation));
            }
        }

        public void RefundLostCell(Currency cost)
        {
            if (cost.production > 0)
            {
                var newProdUtilisation = UtilisedProduction - cost.production;
                observers.ForEach(o => o.OnProductionUtilisationChanged(playerData.Player, newProdUtilisation));
                var newPopUtilisation = UtilisedPopulation - cost.population;
                observers.ForEach(o => o.OnPopulationUtilisationChanged(playerData.Player, newPopUtilisation));
            }
        }
    }
}