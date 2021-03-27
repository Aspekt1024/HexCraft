using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.UI;
using Mirror;
using UnityEngine;

namespace Aspekt.Hex
{
    public class NetworkGamePlayerHex : NetworkBehaviour, ICellEventObserver, ControlPanel.IEventReceiver, TurnIndicator.IEventReceiver
    {
        public bool IsReady { get; private set; }
        public PlayerData PlayerData { get; private set; }
        
        [SyncVar(hook = nameof(HandleDisplayNameChanged))]
        private string displayName;
        
        [SyncVar] public int ID;

        public bool IsCurrentPlayer = false;
        
        private NetworkManagerHex room;
        private GameManager game;

        private PlayerActions actions;
        
        private readonly List<MonoBehaviour> uiObjectsInMouseover = new List<MonoBehaviour>();

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
                actions = new PlayerActions(this, game);
            }
        }

        public void SetPlayerData(PlayerData playerData) => PlayerData = playerData;
        
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

        public void OnEndTurnClicked()
        {
            CmdEndTurn();
        }

        public void SetCursorInUI(MonoBehaviour caller, bool isInUI)
        {
            if (isInUI)
            {
                if (uiObjectsInMouseover.Contains(caller)) return;
                uiObjectsInMouseover.Add(caller);
            }
            else
            {
                uiObjectsInMouseover.Remove(caller);
            }
            
            game.UI.SetCursorInUI(uiObjectsInMouseover.Any());
        }

        private void HandleDisplayNameChanged(string oldName, string newName)
        {
            if (game == null) return; // When created by the network manager, the main scene hasn't loaded yet
            game.UI.UpdatePlayerInfo(room.GamePlayers);
        }

        public void UpdateCurrentPlayerStatus(bool isCurrentPlayer)
        {
            IsCurrentPlayer = isCurrentPlayer;
            
            if (hasAuthority)
            {
                actions.UpdatePlayerTurn(isCurrentPlayer);
            }
        }

        public void IndicateBuildCell(BuildAction buildAction, HexCell originator)
        {
            if (!IsCurrentPlayer || originator.PlayerId != ID) return;
            if (!buildAction.CanAfford(ID) || !buildAction.IsRequirementsMet(ID)) return;
            
            actions.SetBuild(originator, buildAction.prefab.cellType);
        }

        public void IndicateUnitAction(UnitCell unit, UnitAction unitAction)
        {
            if (!IsCurrentPlayer) return;
            actions.SetUnitAction(unit, unitAction);
        }

        public void AddTech(Technology tech)
        {
            Debug.Log(ID + " add " + tech);
            if (!hasAuthority || !IsCurrentPlayer) return;
            Debug.Log("current player");
            if (game.Data.CanAddTech(tech, ID))
            {
                Debug.Log("cmd");
                CmdAddTech((Int16)tech);
            }
        }
        
        public void RemoveTech(Technology tech)
        {
            if (!hasAuthority || !IsCurrentPlayer) return;
            if (game.Data.CanRemoveTech(tech, ID))
            {
                game.Data.RemoveTech(tech, ID);
            }
        }

        [ClientRpc]
        public void RpcNotifyGameStart()
        {
            game.StartGameClient();
        }

        [Command]
        private void CmdAddTech(Int16 techId)
        {
            if (Enum.IsDefined(typeof(Technology), (Int32) techId))
            {
                var tech = (Technology) techId;
                if (game.Data.CanAddTech(tech, ID))
                {
                    game.Data.AddTech(tech, ID);
                }
            }
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
            if (attackingUnit.HasAttacked) return;
            
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
            if (!IsCurrentPlayer) return;
            game.Data.NextTurn();
        }
    }
}