using System.Collections.Generic;

namespace Aspekt.Hex
{
    public class PlayerData
    {
        public readonly NetworkGamePlayerHex Player;
        
        public TechnologyData TechnologyData;
        public CurrencyData CurrencyData;

        public int TurnNumber;
        
        public interface ITechObserver
        {
            void OnTechAdded(NetworkGamePlayerHex player, Technology tech);
        }
        
        private readonly List<ITechObserver> techObservers = new List<ITechObserver>();

        public PlayerData(NetworkGamePlayerHex player)
        {
            Player = player;
        }

        public void Init(GameConfig config)
        {
            TechnologyData = new TechnologyData(config.techConfig);
            CurrencyData = new CurrencyData(this);
        }

        public void RegisterTechObserver(ITechObserver observer) => techObservers.Add(observer);
        public void UnregisterTechObserver(ITechObserver observer) => techObservers.Remove(observer);

        public void TechnologyAchieved(Technology tech)
        {
            TechnologyData.AddTechnology(tech);
            techObservers.ForEach(o => o.OnTechAdded(Player, tech));
        }

        public PlayerData Clone()
        {
            var playerDataCopy = new PlayerData(Player)
            {
                TurnNumber = TurnNumber,
                TechnologyData = TechnologyData.Clone(), 
            };

            playerDataCopy.CurrencyData = new CurrencyData(playerDataCopy)
            {
                Supplies = CurrencyData.Supplies,
                MaxProduction = CurrencyData.MaxProduction,
                UtilisedProduction = CurrencyData.UtilisedProduction,
            };

            return playerDataCopy;
        }

        public bool HasSameData(PlayerData data)
        {
            return data.CurrencyData.Supplies == CurrencyData.Supplies
                   && data.CurrencyData.MaxProduction == CurrencyData.MaxProduction
                   && data.TechnologyData.IsEqual(TechnologyData);
        }
    }
}