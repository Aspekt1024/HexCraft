using Aspekt.Hex;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerData
    {
        public readonly NetworkGamePlayerHex Player;

        public int TurnNumber;
        public int Credits;

        public PlayerData(NetworkGamePlayerHex player)
        {
            Player = player;
        }
    }
}