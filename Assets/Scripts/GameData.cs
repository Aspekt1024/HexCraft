using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Mirror;
using UnityEngine;

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
                var newPlayerData = new PlayerData(player)
                {
                    Credits = config.StartingCredits
                };
                playerData.Add(newPlayerData);
                
                if (isServer)
                {
                    RpcSetCurrency((Int16)player.ID, newPlayerData.Credits);
                }
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

        [ClientRpc]
        public void RpcGameWon(Int16 winningPlayerId)
        {
            var winner = playerData.First(p => p.Player.ID == winningPlayerId);
            game.UI.ShowWinner(winner);
            currentPlayer.Player.IsCurrentPlayer = false;
            currentPlayer = null;
            game.UI.SetPlayerTurn(null);
        }

        public void ModifyCurrency(PlayerData data, int change)
        {
            RpcSetCurrency((Int16)data.Player.ID, data.Credits + change);
        }
        
        public void NextTurn()
        {
            if (!isServer) return;
            var player = SetNextPlayer();
            if (player.TurnNumber > 1)
            {
                GenerateIncome(player);
            }
        }

        public PlayerData GetPlayerData(NetworkGamePlayerHex player)
        {
            return playerData.First(p => p.Player.ID == player.ID);
        }

        private PlayerData SetNextPlayer()
        {
            if (currentPlayer == null)
            {
                ServerSetCurrentPlayer(0);
                return playerData[0];
            }

            var currentPlayerIndex = playerData.FindIndex(p => p.Player.ID == currentPlayer.Player.ID);
            currentPlayerIndex = (currentPlayerIndex + 1) % room.GamePlayers.Count;

            ServerSetCurrentPlayer(currentPlayerIndex);
            
            return playerData[currentPlayerIndex];
        }

        private void ServerSetCurrentPlayer(int playerIndex)
        {
            if (currentPlayer != null)
            {
                currentPlayer.Player.IsCurrentPlayer = false;
            }
            
            playerData[playerIndex].Player.IsCurrentPlayer = true;
            playerData[playerIndex].TurnNumber++;
            RpcSetCurrentPlayer((Int16)playerData[playerIndex].Player.ID);
        }

        private void GenerateIncome(PlayerData data)
        {
            var incomeCells = game.Cells.GetIncomeCells(data.Player.ID);
            var homeCells = game.Cells.GetHomeCells(data.Player.ID);

            var credits = data.Credits;
            credits += incomeCells.Sum(c => c.CreditsPerRound);
            credits += homeCells.Sum(c => 2); // TODO set credits per round for home base cells
    
            RpcSetCurrency((Int16)data.Player.ID, credits);
        }
        
        [ClientRpc]
        private void RpcSetCurrentPlayer(Int16 playerId)
        {
            currentPlayer = playerData.First(p => p.Player.ID == playerId);
            game.UI.SetPlayerTurn(currentPlayer);
        }

        [ClientRpc]
        private void RpcSetCurrency(Int16 playerId, int credits)
        {
            foreach (var player in playerData)
            {
                if (player.Player.ID == playerId)
                {
                    player.Credits = credits;
                    if (player.Player.hasAuthority)
                    {
                        game.UI.SetCurrency(credits);
                    }
                }
            }
        }
    }
}