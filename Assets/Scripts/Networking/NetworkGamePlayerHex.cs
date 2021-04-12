using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Commands;
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
        private Int16 lastActionID = 0;
        
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

        public Int16 GetNewActionID() => lastActionID++;

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
            if (!hasAuthority || !IsCurrentPlayer) return;
            if (game.Data.CanAddTech(tech, ID))
            {
                CmdAddTech((Int16)tech);
            }
        }

        public void OnCellRemoved(HexCell cell)
        {
            RemoveTech(cell.Technology);
            var cost = game.Cells.GetCost(cell.cellType);
            PlayerData.CurrencyData.RefundLostCell(cost);
        }
        
        private void RemoveTech(Technology tech)
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
        public void CmdAttackCell(Int16 playerID, Int16 actionID, Int16 attackerID, Int16 targetID)
        {
            if (!game.IsCurrentPlayer(playerID)) return;
            
            var attackingCell = game.Cells.GetCell(attackerID);
            if (attackingCell == null || !(attackingCell is UnitCell attackingUnit)) return;
            if (attackingUnit.HasAttacked) return;
            
            var target = game.Cells.GetCell(targetID);
            if (!game.Cells.IsValidAttackTarget(attackingUnit, target, ID)) return;

            var damage = ValidatedAttack.GetDamage(attackingUnit, target);
            var isDestroyed = target.CurrentHP <= damage;
            var gameWon = game.DestroyingCellWinsGame(target); 
            
            game.Grid.RpcAttack(
                playerID, actionID,
                attackerID, targetID,
                (Int16) damage, isDestroyed
            );

            if (gameWon)
            {
                game.Data.RpcGameWon((Int16)attackingCell.PlayerId);
            }
        }
        
        [Command]
        public void CmdMoveCell(Int16 playerID, Int16 cellID, Int16 targetX, Int16 targetZ)
        {
            if (!game.IsCurrentPlayer(playerID)) return;
            
            var cell = game.Cells.GetCell(cellID);
            
            var target = new HexCoordinates(targetX, targetZ);
            var path = game.Cells.GetPathWithValidityCheck(cell, target, ID);
            if (path != null)
            {
                game.MoveCell(cell, target);
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