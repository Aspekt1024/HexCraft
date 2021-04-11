using System;
using System.Collections;
using System.Linq;
using Aspekt.Hex.Commands;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class GameManager : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private GameConfig config;
        [SerializeField] private GameUI ui;
#pragma warning restore 649

        public GameUI UI => ui;
        public GameData Data { get; private set; }
        public Cells Cells { get; private set; }
        public HexCamera Camera { get; private set; }
        public HexGrid Grid { get; private set; }

        public readonly CommandValidator CommandValidator = new CommandValidator();
        
        #region Networking

        public int PlayerID => gamePlayer.ID;

        private NetworkManagerHex room;
        private NetworkGamePlayerHex gamePlayer;

        public struct Dependencies
        {
            public NetworkManagerHex NetworkManager;
            public GameData Data;
            public HexGrid Grid;

            public bool IsValid()
            {
                return Data != null && Grid != null && NetworkManager != null;
            }
        }

        #endregion
        
        private void Awake()
        {
            Cells = FindObjectOfType<Cells>();
            Cells.RegisterCellLifecycleObserver(UI);

            Camera = FindObjectOfType<HexCamera>();
        }

        public void SetGamePlayer(NetworkGamePlayerHex player)
        {
            gamePlayer = player;
            UI.Init(player);
        }

        public NetworkGamePlayerHex GetPlayerFromID(int id)
        {
            return room.GamePlayers.FirstOrDefault(p => p.ID == id);
        }

        public void SetDependencies(Dependencies dependencies)
        {
            if (!dependencies.IsValid())
            {
                Debug.LogError("Tried to setup with invalid dependencies");
                // TODO exit to menu?
                return;
            }

            room = dependencies.NetworkManager;
            Data = dependencies.Data;
            Grid = dependencies.Grid;
            
            Data.Init(this, config);
            Grid.Init(this);
            Cells.Init(this, Grid);
            
            StartCoroutine(AwaitNetworkedPlayerRoutine());
        }

        /// <summary>
        /// Called on the server once all players are ready
        /// </summary>
        public void StartGameServer()
        {
            SetupPlayers();
            Data.NextTurn();
            Data.SetGameStarted();
            
            Camera.ScrollTo(Grid.GetStartLocation(gamePlayer.ID));
            
            Cells.RegisterCellLifecycleObserver(Data);
            
            Debug.Log("Game started for server");

            foreach (var player in room.GamePlayers)
            {
                player.RpcNotifyGameStart();
            }
        }

        public void StartGameClient()
        {
            Camera.ScrollTo(Grid.GetStartLocation(gamePlayer.ID));
            Debug.Log("Game started for client");
        }

        private void SetupPlayers()
        {
            foreach (var player in room.GamePlayers)
            {
                Grid.SetStartingLocation(player);
                Data.OnSuppliesChanged(player, config.startingSupply);
            }

            UI.UpdatePlayerInfo(room.GamePlayers);
        }

    #region Server Calls
        
        /// <summary>
        /// Called by the server to validate placement
        /// </summary>
        public void TryPlace(NetworkGamePlayerHex player, Int16 x, Int16 z, Cells.CellTypes type)
        {
            if (!IsCurrentPlayer(player)) return;
            
            var playerData = Data.GetPlayerData(player);
            var cost = Cells.GetCost(type);
            
            if (!playerData.CurrencyData.CanAfford(cost)) return;
            if (!playerData.TechnologyData.HasTechnologyForCell(type)) return;

            if (Grid.TryPlace(x, z, player.ID, type))
            {
                playerData.CurrencyData.Purchase(cost);
            }
        }

        public void MoveCell(HexCoordinates from, HexCoordinates to)
        {
            Grid.RpcMoveCell((Int16)from.X, (Int16)from.Z, (Int16)to.X, (Int16)to.Z);
        }

        public void RemoveCell(int removedByPlayerID, HexCell cell)
        {
            var gameWon = CheckGameWon(cell);
            
            Grid.RpcRemoveCell((Int16) cell.Coordinates.X, (Int16) cell.Coordinates.Z);
            
            if (gameWon)
            {
                Data.RpcGameWon((Int16)removedByPlayerID);
            }
        }
        
    #endregion Server Calls

        private bool CheckGameWon(HexCell lastDestroyedTarget)
        {
            var owner = lastDestroyedTarget.Owner;
            var homeCells = Cells.GetHomeCells(owner.ID);
            return homeCells.All(c => c == lastDestroyedTarget);
        }

        public bool IsCurrentPlayer(int playerID) => Data.IsCurrentPlayer(playerID);
        public bool IsCurrentPlayer(NetworkGamePlayerHex player) => Data.IsCurrentPlayer(player.ID);

        private IEnumerator AwaitNetworkedPlayerRoutine()
        {
            while (gamePlayer == null)
            {
                yield return null;
            }

            while (room.GamePlayers.Any(p => p.ID == 0))
            {
                yield return null;
            }
            
            UI.UpdatePlayerInfo(room.GamePlayers);
            gamePlayer.CmdSetReady();
            room.Game.Cells.RegisterCellEventObserver(gamePlayer);
        }
    }
}