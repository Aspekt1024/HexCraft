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

    [Serializable]
    public struct UtilisableCurrency
    {
        public int maximum;
        public int utilised;

        public int Available => maximum - utilised;
    }
    
    public class CurrencyData
    {
        public UtilisableCurrency Production;
        public UtilisableCurrency Population;
        public int Supplies;

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

        public void SetCurrency(Currency currency)
        {
            Supplies = currency.supplies;
            Production.maximum = currency.production;
            Population.maximum = currency.population;
        }
        
        public void RegisterObserver(IObserver observer) => observers.Add(observer);
        public void UnregisterObserver(IObserver observer) => observers.Remove(observer);
        
        public bool CanAfford(Currency cost)
        {
            return cost.production <= Production.Available
                   && cost.supplies <= Supplies
                   && cost.population <= Population.Available;
        }

        public bool HasUtilisableCurrencies(Currency cost)
        {
            return cost.population <= Population.Available
                   && cost.production <= Production.Available;
        }

        public void ModifyMaxProduction(int productionDelta)
        {
            observers.ForEach(o => o.OnMaxProductionChanged(playerData.Player, Production.maximum + productionDelta));
        }

        public void ModifyMaxPopulation(int populationDelta)
        {
            observers.ForEach(o => o.OnMaxPopulationChanged(playerData.Player, Population.maximum + populationDelta));
        }

        public void Purchase(Currency cost)
        {
            // This instructs the game data to update currency values based on changes in production,
            // currency values are not changed directly here.
            if (cost.production > 0)
            {
                var newProdUtilisation = Production.utilised + cost.production;
                observers.ForEach(o => o.OnProductionUtilisationChanged(playerData.Player, newProdUtilisation));
            }

            if (cost.supplies > 0)
            {
                var newSupplies = Supplies - cost.supplies;
                observers.ForEach(o => o.OnSuppliesChanged(playerData.Player, newSupplies));
            }

            if (cost.population > 0)
            {
                var newPopUtilisation = Population.utilised + cost.population;
                observers.ForEach(o => o.OnPopulationUtilisationChanged(playerData.Player, newPopUtilisation));
            }
        }

        public void RefundLostCell(Currency cost)
        {
            if (cost.production > 0)
            {
                var newProdUtilisation = Production.utilised - cost.production;
                observers.ForEach(o => o.OnProductionUtilisationChanged(playerData.Player, newProdUtilisation));
                var newPopUtilisation = Population.utilised - cost.population;
                observers.ForEach(o => o.OnPopulationUtilisationChanged(playerData.Player, newPopUtilisation));
            }
        }
    }
}