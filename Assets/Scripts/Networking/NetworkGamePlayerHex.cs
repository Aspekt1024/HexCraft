using System;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    public class NetworkGamePlayerHex : NetworkBehaviour, IInputObserver, ICellEventObserver
    {
        public bool IsReady { get; private set; }
        
        [SyncVar(hook = nameof(HandleDisplayNameChanged))]
        private string displayName;
        
        [SyncVar] public int ID;

        [SyncVar(hook = nameof(HandleCurrentPlayerChanged))]
        public bool IsCurrentPlayer = false;
        
        private NetworkManagerHex room;
        private GameManager game;
        private PlayerInput input;

        private CellIndicator indicator;

        public string DisplayName => displayName;
        
        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);

            room = FindObjectOfType<NetworkManagerHex>();
            game = room.Game;

            room.AddGamePlayer(this);
            
            if (hasAuthority)
            {
                game.SetGamePlayer(this);
            
                input = new PlayerInput();
                input.RegisterNotify(this);

                indicator = new CellIndicator(FindObjectOfType<Cells>());
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
            if (indicator.IsPlacingCell)
            {
                CmdPlaceCell((Int16)coords.X, (Int16)coords.Z, (Int16)indicator.CellType);
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
        private void CmdPlaceCell(Int16 x, Int16 z, Int16 cellTypeIndex)
        {
            if (Enum.IsDefined(typeof(Cells.CellTypes), (Int32)cellTypeIndex))
            {
                var cellType = (Cells.CellTypes) cellTypeIndex;
                game.TryPlace(this, x, z, cellType);
            }
        }
        
        [Command]
        private void CmdClickBoardSecondary(Int16 x, Int16 z)
        {
            game.TryRemove(this, x, z);
        }

        [Command]
        public void CmdSetReady()
        {
            IsReady = true;
            room.UpdatePlayerReady();
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

        public void BuildCell(Cells.CellTypes type, HexCell originator)
        {
            indicator.Show(type, ID);
        }

        public void UpgradeCell(HexCell originator)
        {
            // TODO upgrade
            Debug.Log("upgrade cell " + originator.DisplayName);
        }
    }
}