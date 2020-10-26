using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Menu;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aspekt.Hex
{
    public class NetworkManagerHex : NetworkManager
    {
#pragma warning disable 649
        [SerializeField] private int minPlayers = 2;
        [Scene] [SerializeField] private string menuScene;
        [Scene] [SerializeField] private string gameScene;
        [SerializeField] private NetworkRoomPlayerHex roomPlayerPrefab;
        [SerializeField] private NetworkGamePlayerHex gamePlayerPrefab;
        [SerializeField] private HexGrid boardPrefab;
        [SerializeField] private GameData gameDataPrefab;
#pragma warning restore 649

        public readonly List<NetworkRoomPlayerHex> RoomPlayers = new List<NetworkRoomPlayerHex>();
        public readonly List<NetworkGamePlayerHex> GamePlayers = new List<NetworkGamePlayerHex>();

        public GameManager Game { get; private set; }

        private MainMenu mainMenu;

        private GameManager.Dependencies gameManagerDependencies;
        private bool isStartGameActioned;
        private bool isAllPlayersLoadedInGame;

        public override void Awake()
        {
            base.Awake();
            Game = FindObjectOfType<GameManager>();
            gameManagerDependencies.NetworkManager = this;
        }

        public void RegisterMainMenu(MainMenu mainMenu)
        {
            this.mainMenu = mainMenu;
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            var player = SceneManager.GetActiveScene().path == menuScene
                ? Instantiate(roomPlayerPrefab).gameObject
                : Instantiate(gamePlayerPrefab).gameObject;

            NetworkServer.AddPlayerForConnection(conn, player);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            mainMenu.OnClientDisconnect();
        }

        public void AddRoomPlayer(NetworkRoomPlayerHex player) => RoomPlayers.Add(player);
        public void RemoveRoomPlayer(NetworkRoomPlayerHex player) => RoomPlayers.Remove(player);

        public void AddGamePlayer(NetworkGamePlayerHex player)
        {
            GamePlayers.Add(player);
        }

        public void RemoveGamePlayer(NetworkGamePlayerHex player)
        {
            GamePlayers.Remove(player);
            Game.Data.UnregisterPlayer(player);
        }

    public void ShowLobby() => mainMenu.ShowLobby();
        public bool IsLobbyReady() => RoomPlayers.Count == minPlayers && RoomPlayers.All(p => p.IsReady);
        
        public void StartGameFromLobby()
        {
            if (isStartGameActioned) return;
            
            if (SceneManager.GetActiveScene().path == menuScene)
            {
                if (!IsLobbyReady()) return;
                ServerChangeScene(gameScene);
            }
        }

        public void ConnectToTestServer(bool connectAsHost)
        {
            if (connectAsHost)
            {
                StartCoroutine(SetupTestServerRoutine());
            }
            else
            {
                StartClient();
            }
        }

        private IEnumerator SetupTestServerRoutine()
        {
            StartHost();
            while (GamePlayers.Count < minPlayers)
            {
                Debug.Log("[Test Server] waiting for second player");
                yield return new WaitForSeconds(1f);
            }
            SetupGame();
            GamePlayers[0].SetDisplayName("Host");
            GamePlayers[1].SetDisplayName("Client");
        }

        public override void ServerChangeScene(string newSceneName)
        {
            if (gameScene == newSceneName)
            {
                isStartGameActioned = true;
                for (int i = RoomPlayers.Count - 1; i >= 0; i--)
                {
                    var gamePlayer = Instantiate(gamePlayerPrefab);
                    gamePlayer.SetDisplayName(RoomPlayers[i].DisplayName);

                    var conn = RoomPlayers[i].connectionToClient;
                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayer.gameObject);
                }
            }

            base.ServerChangeScene(newSceneName);
        }

        #region Game Setup

        public override void OnServerSceneChanged(string sceneName)
        {
            if (sceneName == gameScene)
            {
                SetupGame();
            }
        }

        private void SetupGame()
        {
            var board = Instantiate(boardPrefab);
            NetworkServer.Spawn(board.gameObject);

            var gameData = Instantiate(gameDataPrefab);
            NetworkServer.Spawn(gameData.gameObject);
        }

        public void RegisterGameData(GameData data)
        {
            gameManagerDependencies.Data = data;
            CheckGameManagerDependencies();
            data.Init(GamePlayers);
        }

        public void RegisterBoard(HexGrid grid)
        {
            gameManagerDependencies.Grid = grid;
            CheckGameManagerDependencies();
        }

        private void CheckGameManagerDependencies()
        {
            if (gameManagerDependencies.IsValid())
            {
                Game.SetDependencies(gameManagerDependencies);
            }
        }

        [Server]
        public void UpdatePlayerReady()
        {
            if (GamePlayers.Count == minPlayers && GamePlayers.All(p => p.IsReady))
            {
                Game.StartGame();
            }
        }
        
        #endregion
        
    }
}