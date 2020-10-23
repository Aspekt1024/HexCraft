using System;
using System.Collections;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class GameManager : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private GameUI ui;
#pragma warning restore 649

        public GameUI UI => ui;
        public GameData Data { get; private set; }
        public Cells Cells { get; private set; }

        #region Networking

        private NetworkManagerHex room;
        private NetworkGamePlayerHex gamePlayer;
        private HexGrid grid;

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
        }

        public void SetGamePlayer(NetworkGamePlayerHex player)
        {
            gamePlayer = player;
            UI.Init(player);
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
            grid = dependencies.Grid;
            
            StartCoroutine(AwaitNetworkedPlayerRoutine());
        }

        public void StartGame()
        {
            SetupPlayers();
            Data.SetNextPlayer();
            Data.SetGameStarted();
            Debug.Log("Game started");
        }

        private void SetupPlayers()
        {
            for (int i = 0; i < room.GamePlayers.Count; i++)
            {
                room.GamePlayers[i].SetPlayerID(i + 1);
                grid.SetStartingLocation(room.GamePlayers[i]);
            }
        }
        
        /// <summary>
        /// Called by the server to validate placement
        /// </summary>
        public void TryPlace(NetworkGamePlayerHex player, Int16 x, Int16 z, Cells.CellTypes type)
        {
            if (!IsActionAllowed(player)) return;

            if (grid.TryPlace(x, z, player.ID, type))
            {
                Data.SetNextPlayer();
            }
        }

        public void TryRemove(NetworkGamePlayerHex player, Int16 x, Int16 z)
        {
            if (!IsActionAllowed(player)) return;

            if (grid.TryRemove(x, z, (Int16)player.ID))
            {
                Data.SetNextPlayer();
            }
        }

        public void MoveCell(HexCoordinates from, HexCoordinates to)
        {
            grid.RpcMoveCell((Int16)from.X, (Int16)from.Z, (Int16)to.X, (Int16)to.Z);
            
            Data.SetNextPlayer();
        }

        public void AttackCell(HexCoordinates attackerCoords, HexCoordinates targetCoords, int damage)
        {
            grid.RpcAttack((Int16)attackerCoords.X,
                (Int16)attackerCoords.Z,
                (Int16)targetCoords.X,
                (Int16)targetCoords.Z,
                (Int16)damage);
            
            Data.SetNextPlayer();
        }

        private bool IsActionAllowed(NetworkGamePlayerHex player)
        {
            return player == Data.CurrentPlayer;
        }
        
        private IEnumerator AwaitNetworkedPlayerRoutine()
        {
            while (gamePlayer == null)
            {
                yield return null;
            }
            UI.UpdatePlayerInfo(room.GamePlayers);
            gamePlayer.CmdSetReady();
            room.Game.Cells.RegisterCellEventObserver(gamePlayer);
        }
    }
}