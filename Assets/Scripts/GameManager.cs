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
        public HexCamera Camera { get; private set; }

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
            grid = dependencies.Grid;

            Data.Init(this, config);
            
            StartCoroutine(AwaitNetworkedPlayerRoutine());
        }

        public void StartGame()
        {
            SetupPlayers();
            Data.NextTurn();
            Data.SetGameStarted();
            
            Camera.ScrollTo(grid.GetStartLocation(gamePlayer.ID));
            
            Debug.Log("Game started");
        }

        private void SetupPlayers()
        {
            foreach (var player in room.GamePlayers)
            {
                grid.SetStartingLocation(player);
                Data.SetCurrency(player, config.StartingCredits);
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
            
            if (playerData.Credits < cost) return;

            if (grid.TryPlace(x, z, player.ID, type))
            {
                Data.ModifyCurrency(playerData, -cost);
                Data.AddActionPoints(1);
            }
        }

        public void MoveCell(HexCoordinates from, HexCoordinates to)
        {
            grid.RpcMoveCell((Int16)from.X, (Int16)from.Z, (Int16)to.X, (Int16)to.Z);
            Data.AddActionPoints(1);
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
                var gameWon = CheckGameWon(target);
                
                grid.RpcRemoveCell((Int16)target.Coordinates.X, (Int16)target.Coordinates.Z, (Int16)player.ID);

                if (gameWon)
                {
                    Data.RpcGameWon((Int16)attacker.Owner.ID);
                    return;
                }
            }
            
            Data.AddActionPoints(1);
        }

    #endregion Server Calls

        private bool CheckGameWon(HexCell lastDestroyedTarget)
        {
            var owner = lastDestroyedTarget.Owner;
            var homeCells = Cells.GetHomeCells(owner.ID);
            return homeCells.All(c => c == lastDestroyedTarget);
        }

        public bool IsCurrentPlayer(NetworkGamePlayerHex player) => Data.IsCurrentPlayer(player);

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