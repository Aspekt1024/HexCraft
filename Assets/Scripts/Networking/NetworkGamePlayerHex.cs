using System;
using Aspekt.Hex.UI;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    public class NetworkGamePlayerHex : NetworkBehaviour, ICellEventObserver, ControlPanel.IEventReceiver
    {
        public bool IsReady { get; private set; }
        
        [SyncVar(hook = nameof(HandleDisplayNameChanged))]
        private string displayName;
        
        [SyncVar] public int ID;

        [SyncVar(hook = nameof(HandleCurrentPlayerChanged))]
        public bool IsCurrentPlayer = false;
        
        private NetworkManagerHex room;
        private GameManager game;

        private CellActions actions;

        public string DisplayName => displayName;
        
        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);

            room = FindObjectOfType<NetworkManagerHex>();
            room.AddGamePlayer(this);
        }

        public void Init(GameManager game)
        {
            this.game = game;
            if (hasAuthority)
            {
                game.SetGamePlayer(this);
                actions = new CellActions(this, game);
            }
        }
        
        public override void OnStopClient()
        {
            room.RemoveGamePlayer(this);
        }

        [Server]
        public void SetPlayerId(int id)
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
            actions.Update();
        }

        public void OnEndTurnRequested()
        {
            CmdEndTurn();
        }

        public void SetCursorInUI(bool isInUI) => game.UI.SetCursorInUI(isInUI);

        private void HandleDisplayNameChanged(string oldName, string newName)
        {
            if (game == null) return; // When created by the network manager, the main scene hasn't loaded yet
            game.UI.UpdatePlayerInfo(room.GamePlayers);
        }

        private void HandleCurrentPlayerChanged(bool oldStatus, bool newStatus)
        {
            if (!hasAuthority) return;
            actions.UpdatePlayerTurn(IsCurrentPlayer);
            
            if (IsCurrentPlayer)
            {
                // TODO set UI to indicate to player
            }
        }

        public void IndicateBuildCell(Cells.CellTypes type, HexCell originator)
        {
            if (!IsCurrentPlayer) return;
            actions.SetBuild(originator, type);
        }

        public void UpgradeCell(HexCell originator)
        {
            // TODO upgrade
            Debug.Log("upgrade cell " + originator.DisplayName);
        }

        public void IndicateUnitAttack(UnitCell unit)
        {
            if (!IsCurrentPlayer) return;
            actions.SetUnitAttack(unit);
        }

        public void IndicateUnitMove(UnitCell unit)
        {
            if (!IsCurrentPlayer) return;
            actions.SetUnitMove(unit);
        }
        
        [Command]
        public void CmdPlaceCell(Int16 x, Int16 z, Int16 cellTypeIndex)
        {
            if (!game.IsCurrentPlayer(this)) return;
            
            if (Enum.IsDefined(typeof(Cells.CellTypes), (Int32)cellTypeIndex))
            {
                var cellType = (Cells.CellTypes) cellTypeIndex;
                game.TryPlace(this, x, z, cellType);
            }
        }

        [Command]
        public void CmdAttackCell(Int16 originX, Int16 originZ, Int16 targetX, Int16 targetZ)
        {
            if (!game.IsCurrentPlayer(this)) return;
            
            var attackingCell = game.Cells.GetCellAtPosition(new HexCoordinates(originX, originZ));
            if (attackingCell == null || !(attackingCell is UnitCell attackingUnit)) return;
            var target = game.Cells.GetCellAtPosition(new HexCoordinates(targetX, targetZ));
            if (game.Cells.IsValidAttackTarget(attackingUnit, target, ID))
            {
                game.AttackCell(attackingUnit, target);
            }
        }
        
        [Command]
        public void CmdMoveCell(Int16 originX, Int16 originZ, Int16 targetX, Int16 targetZ)
        {
            if (!game.IsCurrentPlayer(this)) return;
            
            var movingUnit = game.Cells.GetCellAtPosition(new HexCoordinates(originX, originZ));
            var target = new HexCoordinates(targetX, targetZ);
            var path = game.Cells.GetPathWithValidityCheck(movingUnit, target, ID);
            if (path != null)
            {
                game.MoveCell(movingUnit.Coordinates, target);
            }
        }
        
        [Command]
        public void CmdSetReady()
        {
            IsReady = true;
            room.UpdatePlayerReady();
        }

        [Command]
        private void CmdEndTurn()
        {
            game.Data.NextTurn();
        }
    }
}