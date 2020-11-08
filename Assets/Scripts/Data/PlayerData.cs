using Aspekt.Hex;
using UnityEngine;

namespace Aspekt.Hex
{
    public class PlayerData
    {
        public readonly NetworkGamePlayerHex Player;
        public readonly TechnologyData TechnologyData;

        public int TurnNumber;
        public int Credits;

        public PlayerData(NetworkGamePlayerHex player)
        {
            Player = player;
            TechnologyData = new TechnologyData();
        }

        public void TechnologyAchieved(Technology tech)
        {
            TechnologyData.AddTechnology(tech);
        }
    }
}