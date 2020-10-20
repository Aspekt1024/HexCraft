using System;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    public class NetworkGamePlayerHex : NetworkBehaviour, IInputObserver
    {
        [SyncVar(hook = nameof(HandleReadyStateChanged))]
        public bool IsReady = false;
        
        [SyncVar(hook = nameof(HandleDisplayNameChanged))]
        private string displayName;
        
        [SyncVar] public int ID;

        [SyncVar(hook = nameof(HandleCurrentPlayerChanged))]
        public bool IsCurrentPlayer = false;
        
        private NetworkManagerHex room;
        private GameManager game;
        private PlayerInput input;

        private CellIndicator indicator;

        private bool isPlacingCell;

        public string DisplayName => displayName;
        
        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);

            room = FindObjectOfType<NetworkManagerHex>();
            game = room.Game;
            game.SetGamePlayer(this);
            room.AddGamePlayer(this);
            
            input = new PlayerInput();
            input.RegisterNotify(this);

            indicator = new CellIndicator(FindObjectOfType<Cells>());

            if (hasAuthority)
            {
                IsReady = true;
            }
        }
        
        public override void OnStopClient()
        {
            room.RemoveGamePlayer(this);
        }

        [Server]
        public void SetPlayerID(int id)
        {
            ID = id;
        }
        
        [Server]
        public void SetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        private void Update()
        {
            if (!hasAuthority || !game.IsRunning()) return;
            
            input.HandleInput();
            indicator.Update(input.GetMousePositionOnBoard());
        }

        public void BoardClickedPrimary(Vector3 position)
        {
            var coords = HexCoordinates.FromPosition(position);
            if (isPlacingCell)
            {
                CmdClickBoardPrimary((Int16)coords.X, (Int16)coords.Z);
            }
            else
            {
                var cell = game.Cells.GetCellAtPosition(coords);
                if (cell != null)
                {
                    game.UI.ShowCellInfo(cell);
                }
                else
                {
                    game.UI.HideCellInfo();
                }
            }
        }

        public void BoardClickedSecondary(Vector3 position)
        {
            var coords = HexCoordinates.FromPosition(position);
            CmdClickBoardSecondary((Int16) coords.X, (Int16) coords.Z);
        }
        
        [Command]
        private void CmdClickBoardPrimary(Int16 x, Int16 z)
        {
            game.TryPlace(this, x, z);
        }
        
        [Command]
        private void CmdClickBoardSecondary(Int16 x, Int16 z)
        {
            game.TryRemove(this, x, z);
        }

        private void HandleReadyStateChanged(bool oldStatus, bool newStatus)
        {
            if (isServer && IsReady)
            {
                room.UpdatePlayerReady();
            }
        }

        private void HandleDisplayNameChanged(string oldName, string newName)
        {
            game.UI.UpdatePlayerInfo(room.GamePlayers);
        }

        private void HandleCurrentPlayerChanged(bool oldStatus, bool newStatus)
        {
            if (!hasAuthority) return;
            
            if (IsCurrentPlayer)
            {
                
            }
            else
            {
                indicator.Hide();
            }
        }

    }
}