using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    /// <summary>
    /// Game data is owned by the server and distributed to all clients
    /// </summary>
    public class GameData : NetworkBehaviour, ICellLifecycleObserver, CurrencyData.IObserver
    {
        private GameManager game;
        private NetworkManagerHex room;

        private readonly List<PlayerData> playerData = new List<PlayerData>();
        
        private PlayerData currentPlayer;

        private enum GameStates
        {
            Initializing,
            Started,
        }
        
        [SyncVar] private GameStates state = GameStates.Initializing;

        public bool IsRunning => state == GameStates.Started;
        
        public GameConfig Config { get; private set; }

        public bool IsCurrentPlayer(int playerID) => currentPlayer != null && currentPlayer.Player.ID == playerID;
        
        public override void OnStartClient()
        {
            room = FindObjectOfType<NetworkManagerHex>();
            room.RegisterGameData(this);
        }

        public void RegisterPlayers(List<NetworkGamePlayerHex> players)
        {
            foreach (var player in players)
            {
                var newPlayerData = new PlayerData(player);
                playerData.Add(newPlayerData);
                player.SetPlayerData(newPlayerData);
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
            config.Init(game.Cells);
            playerData.ForEach(d =>
            {
                d.Init(config);
                d.CurrencyData.RegisterObserver(this);
            });
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
            
            var techData = Config.GetUpgradeDetails(tech);
            if (Config.techConfig.CanAddTech(techData, data))
            {
                data.CurrencyData.Purchase(techData.cost);
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
        
        public void OnMaxProductionChanged(NetworkGamePlayerHex player, int newProduction)
        {
            RpcSetMaxProduction((Int16)player.ID, (Int16)newProduction);
        }

        public void OnMaxPopulationChanged(NetworkGamePlayerHex player, int newPopulation)
        {
            RpcSetMaxPopulation((Int16)player.ID, (Int16)newPopulation);
        }

        public void OnProductionUtilisationChanged(NetworkGamePlayerHex player, int newUtilisation)
        {
            RpcSetProductionUtilisation((Int16)player.ID, (Int16)newUtilisation);
        }

        public void OnPopulationUtilisationChanged(NetworkGamePlayerHex player, int newUtilisation)
        {
            RpcSetPopulationUtilisation((Int16)player.ID, (Int16)newUtilisation);
        }

        public void OnSuppliesChanged(NetworkGamePlayerHex player, int newSupplies)
        {
            RpcSetSupplies((Int16)player.ID, (Int16)newSupplies);
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

        public bool CanAfford(int playerId, Currency cost)
        {
            var player = GetPlayerFromId(playerId);
            return player.CurrencyData.CanAfford(cost);
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
            RpcSetCurrentPlayer((Int16)playerData[playerIndex].Player.ID, (Int16)playerData[playerIndex].TurnNumber);
        }

        private void GenerateIncome(PlayerData data)
        {
            var suppliers = game.Cells.GetSuppliers(data.Player.ID);
            var supplies = data.CurrencyData.Supplies;
            var generatedSupplies = suppliers.Sum(c => c.CalculateSupplies(data));
            supplies += generatedSupplies;
    
            RpcSetSupplies((Int16)data.Player.ID, (Int16)supplies);
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
        private void RpcSetCurrentPlayer(Int16 playerId, Int16 turnNumber)
        {
            currentPlayer = playerData.First(p => p.Player.ID == playerId);
            currentPlayer.TurnNumber = turnNumber;
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
            game.Cells.OnNewTurn(currentPlayer);
        }

        [ClientRpc]
        private void RpcSetProductionUtilisation(Int16 playerId, int newUtilisation)
        {
            var player = GetPlayerFromId(playerId);
            player.CurrencyData.Production.utilised = newUtilisation;
            if (player.Player.hasAuthority)
            {
                game.UI.UpdateCurrency(player.CurrencyData);
            }
        }

        [ClientRpc]
        private void RpcSetPopulationUtilisation(Int16 playerId, int newUtilisation)
        {
            var player = GetPlayerFromId(playerId);
            player.CurrencyData.Population.utilised = newUtilisation;
            if (player.Player.hasAuthority)
            {
                game.UI.UpdateCurrency(player.CurrencyData);
            }
        }

        [ClientRpc]
        private void RpcSetMaxProduction(Int16 playerId, Int16 newProduction)
        {
            var player = GetPlayerFromId(playerId);
            player.CurrencyData.Production.maximum = newProduction;
            if (player.Player.hasAuthority)
            {
                game.UI.UpdateCurrency(player.CurrencyData);
            }
        }

        [ClientRpc]
        private void RpcSetMaxPopulation(Int16 playerId, Int16 newPopulation)
        {
            var player = GetPlayerFromId(playerId);
            player.CurrencyData.Population.maximum = newPopulation;
            if (player.Player.hasAuthority)
            {
                game.UI.UpdateCurrency(player.CurrencyData);
            }
        }

        [ClientRpc]
        private void RpcSetSupplies(Int16 playerId, Int16 newSupplies)
        {
            var player = GetPlayerFromId(playerId);
            player.CurrencyData.Supplies = newSupplies;
            if (player.Player.hasAuthority)
            {
                game.UI.UpdateCurrency(player.CurrencyData);
            }
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
            Debug.Log("cell created: " + cell.DisplayName + " : " + cell.Owner.ID);
            var data = GetPlayerFromId(cell.PlayerId);

            if (isServer)
            {
                RpcAddTech((Int16)cell.Technology, (Int16)cell.PlayerId);

                if (cell is BuildingCell buildingCell)
                {
                    var prod = buildingCell.CalculateProduction(data);
                    data.CurrencyData.ModifyMaxProduction(prod);

                    var pop = buildingCell.CalculatePopulation(data);
                    data.CurrencyData.ModifyMaxPopulation(pop);
                }
            }
        }

        public void OnCellRemoved(HexCell cell)
        {
            var data = GetPlayerFromId(cell.PlayerId);
            data.Player.OnCellRemoved(cell);

            if (isServer)
            {
                if (cell is BuildingCell buildingCell)
                {
                    var prod = buildingCell.CalculateProduction(data);
                    data.CurrencyData.ModifyMaxProduction(-prod);
                    
                    var pop = buildingCell.CalculatePopulation(data);
                    data.CurrencyData.ModifyMaxPopulation(-pop);
                }
            }
        }
    }
}