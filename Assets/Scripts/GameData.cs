using System.Linq;
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
        
        public NetworkGamePlayerHex CurrentPlayer { get; private set; }

        private enum GameStates
        {
            Initializing,
            Started,
        }
        
        [SyncVar] private GameStates state = GameStates.Initializing;

        public bool IsRunning => state == GameStates.Started;
        
        public override void OnStartClient()
        {
            room = FindObjectOfType<NetworkManagerHex>();
            game = room.Game;
            room.RegisterGameData(this);
        }

        public void SetGameStarted()
        {
            state = GameStates.Started;
        }
        
        public void SetNextPlayer()
        {
            if (!isServer) return;
            
            if (CurrentPlayer == null)
            {
                room.GamePlayers[0].IsCurrentPlayer = true;
                RpcSetCurrentPlayer(room.GamePlayers[0].netId);
                return;
            }

            var currentPlayerIndex = room.GamePlayers.FindIndex(p => p == CurrentPlayer);
            currentPlayerIndex = (currentPlayerIndex + 1) % room.GamePlayers.Count;
            
            CurrentPlayer.IsCurrentPlayer = false;
            room.GamePlayers[currentPlayerIndex].IsCurrentPlayer = true;
            
            RpcSetCurrentPlayer(room.GamePlayers[currentPlayerIndex].netId);
        }
        
        [ClientRpc]
        private void RpcSetCurrentPlayer(uint playerNetId)
        {
            CurrentPlayer = room.GamePlayers.FirstOrDefault(p => p.netId == playerNetId);
            game.UI.SetPlayerTurn(CurrentPlayer);
        }
    }
}