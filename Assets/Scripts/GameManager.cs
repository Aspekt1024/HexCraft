using System;
using System.Collections;
using System.Linq;
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
            grid = dependencies.Grid;

            Data.Init(config);
            
            StartCoroutine(AwaitNetworkedPlayerRoutine());
        }

        public void StartGame()
        {
            SetupPlayers();
            Data.NextTurn();
            Data.SetGameStarted();
            Debug.Log("Game started");
        }

        private void SetupPlayers()
        {
            for (int i = 0; i < room.GamePlayers.Count; i++)
            {
                grid.SetStartingLocation(room.GamePlayers[i]);
            }
        }

    #region Server Calls
        
        /// <summary>
        /// Called by the server to validate placement
        /// </summary>
        public void TryPlace(NetworkGamePlayerHex player, Int16 x, Int16 z, Cells.CellTypes type)
        {
            if (!IsActionAllowed(player)) return;
            
            var playerData = Data.GetPlayerData(player);
            var cost = Cells.GetCost(type);
            
            if (playerData.Credits < cost) return;

            if (grid.TryPlace(x, z, player.ID, type))
            {
                Data.ModifyCurrency(playerData, -cost);
                Data.NextTurn();
            }
        }

        public void TryRemove(NetworkGamePlayerHex player, int cellX, int cellZ)
        {
            if (!IsActionAllowed(player)) return;

            if (grid.TryRemove((Int16)cellX, (Int16)cellZ, (Int16)player.ID))
            {
                Data.NextTurn();
            }
        }

        public void MoveCell(HexCoordinates from, HexCoordinates to)
        {
            grid.RpcMoveCell((Int16)from.X, (Int16)from.Z, (Int16)to.X, (Int16)to.Z);
            
            Data.NextTurn();
        }

        public void AttackCell(UnitCell attacker, HexCell target)
        {
            var isKillingBlow = target.CurrentHP <= attacker.AttackDamage;
            
            grid.RpcAttack((Int16)attacker.Coordinates.X,
                (Int16)attacker.Coordinates.Z,
                (Int16)target.Coordinates.X,
                (Int16)target.Coordinates.Z,
                (Int16)attacker.AttackDamage);
            
            if (isKillingBlow)
            {
                var player = room.GamePlayers.First(p => p.ID == attacker.PlayerId);
                TryRemove(player, target.Coordinates.X, target.Coordinates.Z);
            }
            
            Data.NextTurn();
        }

    #endregion Server Calls

        private bool IsActionAllowed(NetworkGamePlayerHex player)
        {
            return Data.IsCurrentPlayer(player);
        }

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