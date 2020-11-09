using System.Collections.Generic;

namespace Aspekt.Hex
{
    public class PlayerData
    {
        public readonly NetworkGamePlayerHex Player;
        public readonly TechnologyData TechnologyData;

        public int TurnNumber;
        public int Credits;

        public interface ITechObserver
        {
            void OnTechAdded(NetworkGamePlayerHex player, Technology tech);
        }
        
        private readonly List<ITechObserver> techObservers = new List<ITechObserver>();

        public PlayerData(NetworkGamePlayerHex player)
        {
            Player = player;
            TechnologyData = new TechnologyData();
        }

        public void RegisterTechObserver(ITechObserver observer) => techObservers.Add(observer);

        public void TechnologyAchieved(Technology tech)
        {
            TechnologyData.AddTechnology(tech);
            techObservers.ForEach(o => o.OnTechAdded(Player, tech));
        }
    }
}