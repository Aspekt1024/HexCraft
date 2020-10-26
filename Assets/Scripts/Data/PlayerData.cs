using Aspekt.Hex;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerData
    {
        public readonly NetworkGamePlayerHex Player;
        
        public int Credits;
        public int ActionsRemaining;

        public PlayerData(NetworkGamePlayerHex player)
        {
            Player = player;
        }
    }
}