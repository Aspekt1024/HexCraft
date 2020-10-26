using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Mirror;

namespace Aspekt.Hex
{
    /// <summary>
    /// Game data is owned by the server and distributed to all clients
    /// </summary>
    public class GameData : NetworkBehaviour
    {
        private GameManager game;
        private NetworkManagerHex room;
        private GameConfig config;
        
        private readonly List<PlayerData> playerData = new List<PlayerData>();

        private PlayerData currentPlayer;

        private enum GameStates
        {
            Initializing,
            Started,
        }
        
        [SyncVar] private GameStates state = GameStates.Initializing;

        public bool IsRunning => state == GameStates.Started;
        
        public bool IsCurrentPlayer(NetworkGamePlayerHex player)
        {
            return currentPlayer != null && currentPlayer.Player.ID == player.ID;
        }
        
        public override void OnStartClient()
        {
            room = FindObjectOfType<NetworkManagerHex>();
            game = room.Game;
            room.RegisterGameData(this);
        }

        public void RegisterPlayers(List<NetworkGamePlayerHex> players)
        {
            foreach (var player in players)
            {
                var newPlayerData = new PlayerData(player);
                newPlayerData.Credits = config.StartingCredits;
                playerData.Add(newPlayerData);
            }
        }
        
        public void UnregisterPlayer(NetworkGamePlayerHex player)
        {
            playerData.RemoveAt(playerData.FindIndex(p => p.Player.ID == player.ID));
        }

        public void Init(GameConfig config)
        {
            this.config = config;
        }

        public void SetGameStarted()
        {
            state = GameStates.Started;
        }
        
        public void SetNextPlayer()
        {
            if (!isServer) return;
            
            if (currentPlayer == null)
            {
                room.GamePlayers[0].IsCurrentPlayer = true;
                RpcSetCurrentPlayer(room.GamePlayers[0].netId);
                return;
            }

            var currentPlayerIndex = playerData.FindIndex(p => p.Player.ID == currentPlayer.Player.ID);
            currentPlayerIndex = (currentPlayerIndex + 1) % room.GamePlayers.Count;
            
            currentPlayer.Player.IsCurrentPlayer = false;
            room.GamePlayers[currentPlayerIndex].IsCurrentPlayer = true;
            
            RpcSetCurrentPlayer(room.GamePlayers[currentPlayerIndex].netId);
        }
        
        [ClientRpc]
        private void RpcSetCurrentPlayer(uint playerNetID)
        {
            currentPlayer = playerData.First(p => p.Player.netId == playerNetID);
            game.UI.SetPlayerTurn(currentPlayer.Player);
        }
    }
}