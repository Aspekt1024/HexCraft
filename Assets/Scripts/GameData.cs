using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Config;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    /// <summary>
    /// Game data is owned by the server and distributed to all clients
    /// </summary>
    public class GameData : NetworkBehaviour, ICellLifecycleObserver
    {
        private GameManager game;
        private NetworkManagerHex room;

        private readonly List<PlayerData> playerData = new List<PlayerData>();
        
        private PlayerData currentPlayer;
        private int currentPlayerActionPoints;
        private const int MaxActionPoints = 4;

        private enum GameStates
        {
            Initializing,
            Started,
        }
        
        [SyncVar] private GameStates state = GameStates.Initializing;

        public bool IsRunning => state == GameStates.Started;
        
        public GameConfig Config { get; private set; }

        public bool IsCurrentPlayer(NetworkGamePlayerHex player)
        {
            return currentPlayer != null && currentPlayer.Player.ID == player.ID;
        }
        
        public override void OnStartClient()
        {
            room = FindObjectOfType<NetworkManagerHex>();
            room.RegisterGameData(this);
        }

        public void RegisterPlayers(List<NetworkGamePlayerHex> players)
        {
            foreach (var player in players)
            {
                playerData.Add(new PlayerData(player));
            }
        }
        
        public void UnregisterPlayer(NetworkGamePlayerHex player)
        {
            playerData.RemoveAt(playerData.FindIndex(p => p.Player.ID == player.ID));
        }

        public void Init(GameManager game, GameConfig config)
        {
            this.game = game;
            Config = config;
            playerData.ForEach(d => d.Init(config));
        }

        public void SetGameStarted()
        {
            state = GameStates.Started;
        }

        public bool CanAddTech(Technology tech, int playerId)
        {
            var data = GetPlayerFromId(playerId);
            return Config.techConfig.CanAddTech(tech, data);
        }

        public bool CanRemoveTech(Technology tech, int playerId)
        {
            var cells = game.Cells.AllCells.Where(c => c.Owner.ID == playerId);
            return Config.techConfig.CanRemoveTech(tech, cells);
        }

        public void AddTech(Technology tech, int playerId)
        {
            var data = GetPlayerFromId(playerId);
            if (Config.techConfig.IsBuildingTech(tech))
            {
                RpcAddTech((Int16)tech, (Int16)playerId);
                return;
            }
            
            var techData = Config.GetTechDetails(tech);
            if (Config.techConfig.CanAddTech(techData, data))
            {
                SetCurrency(data.Player, data.Credits - techData.cost);
                RpcAddTech((Int16)tech, (Int16)playerId);
            }
        }

        public void RemoveTech(Technology tech, int playerId)
        {
            if (CanRemoveTech(tech, playerId))
            {
                RpcRemoveTech((Int16) tech, (Int16) playerId);
            }
        }

        public void SetCurrency(NetworkGamePlayerHex player, int credits)
        {
            RpcSetCurrency((Int16)player.ID, credits);
        }

        public void ModifyCurrency(PlayerData data, int change)
        {
            RpcSetCurrency((Int16)data.Player.ID, data.Credits + change);
        }

        public void AddActionPoints(int actionPoints)
        {
            currentPlayerActionPoints += actionPoints;
            if (currentPlayerActionPoints >= MaxActionPoints)
            {
                NextTurn();
            }
            RpcSetPlayerActions((Int16)currentPlayer.Player.ID, (Int16)(MaxActionPoints - currentPlayerActionPoints));
        }
        
        public void NextTurn()
        {
            if (!isServer) return;
            var player = SetNextPlayer();
            if (player.TurnNumber > 1)
            {
                GenerateIncome(player);
            }
            currentPlayerActionPoints = 0;
            RpcSetPlayerActions((Int16)player.Player.ID, (Int16)MaxActionPoints);
        }

        public bool IsTechAvailable(Technology tech, int playerId)
        {
            var player = GetPlayerFromId(playerId);
            return player.TechnologyData.HasTechnology(tech);
        }

        public bool IsTechAvailable(List<Technology> tech, int playerId)
        {
            var player = GetPlayerFromId(playerId);
            if (player == null) return false;

            return player.TechnologyData.HasTechnologies(tech);
        }

        public PlayerData GetPlayerData(NetworkGamePlayerHex player)
        {
            return playerData.First(p => p.Player.ID == player.ID);
        }

        public Technology GetCurrentLevelTech(TechGroups techGroup, int playerId)
        {
            var player = GetPlayerFromId(playerId);
            return player.TechnologyData.GetTechLevel(techGroup);
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
        public void RpcGameWon(Int16 winningPlayerId)
        {
            var winner = playerData.First(p => p.Player.ID == winningPlayerId);
            game.UI.ShowWinner(winner);
            currentPlayer.Player.IsCurrentPlayer = false;
            currentPlayer = null;
            game.UI.SetPlayerTurn(null);
        }
        
        [ClientRpc]
        private void RpcSetCurrentPlayer(Int16 playerId)
        {
            currentPlayer = playerData.First(p => p.Player.ID == playerId);
            foreach (var p in playerData)
            {
                var isCurrentPlayer = p.Player.ID == playerId;
                if (isCurrentPlayer)
                {
                    currentPlayer = p;
                    p.Player.IsCurrentPlayer = true;
                }
                p.Player.UpdateCurrentPlayerStatus(isCurrentPlayer);
            }
            game.UI.SetPlayerTurn(currentPlayer);
            game.Cells.OnNewTurn();
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

        [ClientRpc]
        private void RpcSetPlayerActions(Int16 playerId, Int16 actionCount)
        {
            var data = GetPlayerFromId(playerId);
            game.UI.SetPlayerActionCount(data, MaxActionPoints - currentPlayerActionPoints);
        }

        [ClientRpc]
        private void RpcAddTech(Int16 techId, Int16 playerId)
        {
            var data = GetPlayerFromId(playerId);
            var tech = (Technology) techId;
            data.TechnologyAchieved(tech);

            var playerCells = game.Cells.AllCells.Where(c => c.Owner.ID == playerId);
            foreach (var cell in playerCells)
            {
                cell.OnTechAdded(tech);
            }
            
            game.UI.Refresh();
        }

        [ClientRpc]
        private void RpcRemoveTech(Int16 techId, Int16 playerId)
        {
            var data = GetPlayerFromId(playerId);
            var tech = (Technology) techId;

            var playerCells = game.Cells.AllCells.Where(c => c.Owner.ID == playerId);
            foreach (var cell in playerCells)
            {
                cell.OnTechRemoved(tech);
            }
            
            game.UI.Refresh();
        }

        private PlayerData GetPlayerFromId(int id)
        {
            return playerData.FirstOrDefault(data => data.Player.ID == id);
        }

        public void OnCellCreated(HexCell cell)
        {
            if (IsCurrentPlayer(cell.Owner))
            {
                var data = GetPlayerFromId(cell.PlayerId);
                data.Player.AddTech(cell.Technology);
            }
        }

        public void OnCellRemoved(HexCell cell)
        {
            var data = GetPlayerFromId(cell.PlayerId);
            data.Player.RemoveTech(cell.Technology);
        }
    }
}