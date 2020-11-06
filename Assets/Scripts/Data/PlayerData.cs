using Aspekt.Hex;
using UnityEngine;

namespace Aspekt.Hex
{
    public class PlayerData
    {
        public readonly NetworkGamePlayerHex Player;
        public readonly UpgradeData UpgradeData;

        public int TurnNumber;
        public int Credits;

        public PlayerData(NetworkGamePlayerHex player)
        {
            Player = player;
            UpgradeData = new UpgradeData();
        }
    }
}