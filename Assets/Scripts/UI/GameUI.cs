using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public enum HexCursor
    {
        Default,
        Attack,
        Move,
        None,
        Invalid,
    }
    
    public class GameUI : MonoBehaviour, ICellLifecycleObserver
    {
#pragma warning disable 649
        [Header("HUD")]
        [SerializeField] private PlayerInfo player1;
        [SerializeField] private PlayerInfo player2;
        [SerializeField] private CurrencyUI currency;
        [SerializeField] private ControlPanel controlPanel;
        [SerializeField] private GameOverUI gameOverUI;
        
        [Header("Cursors")]
        [SerializeField] private Texture2D defaultCursor;
        [SerializeField] private Texture2D attackCursor;
        [SerializeField] private Texture2D moveCursor;
        [SerializeField] private Texture2D invalidCursor;

        [Header("Units")]
        [SerializeField] private HealthBars healthBars;
#pragma warning restore 649

        private NetworkGamePlayerHex player;

        public void Init(NetworkGamePlayerHex player)
        {
            this.player = player;
            controlPanel.RegisterSingleObserver(player);
            SetCursor(HexCursor.Default);
        }

        public void ShowWinner(PlayerData winner)
        {
            StartCoroutine(GameWonSequence(winner));
        }
        
        public void UpdatePlayerInfo(List<NetworkGamePlayerHex> players)
        {
            for (var i = 0; i < players.Count; i++)
            {
                SetPlayerInfo(players[i], i);
            }
        }
        
        private void SetPlayerInfo(NetworkGamePlayerHex player, int playerNumber)
        {
            if (playerNumber == 0)
            {
                player1.SetupPlayerInfo(player);
            }
            else
            {
                player2.SetupPlayerInfo(player);
            }
        }

        public void SetPlayerTurn(PlayerData playerData)
        {
            if (playerData == null)
            {
                player1.SetTurnIndicator(false);
                player2.SetTurnIndicator(false);
                controlPanel.SetPlayerTurn(null);
                return;
            }
            
            if (player1.player == playerData.Player)
            {
                player1.SetTurnIndicator(true);
                player2.SetTurnIndicator(false);
            }
            else
            {
                player1.SetTurnIndicator(false);
                player2.SetTurnIndicator(true);
            }

            controlPanel.SetPlayerTurn(playerData);
        }

        public void SetCurrency(int credits)
        {
            currency.SetCredits(credits);
        }

        public void ShowCellInfo(HexCell cell)
        {
            controlPanel.SetCellSelected(cell, player);
        }

        public void HideCellInfo()
        {
            controlPanel.SetCellSelected(null, null);
        }

        public void SetCursor(HexCursor type)
        {
            Cursor.visible = type != HexCursor.None;
            var center = new Vector2(16f, 16f);
            switch (type)
            {
                case HexCursor.Default:
                    Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.ForceSoftware);
                    break;
                case HexCursor.Attack:
                    Cursor.SetCursor(attackCursor, center, CursorMode.ForceSoftware);
                    break;
                case HexCursor.Move:
                    Cursor.SetCursor(moveCursor, center, CursorMode.ForceSoftware);
                    break;
                case HexCursor.Invalid:
                    Cursor.SetCursor(invalidCursor, center, CursorMode.ForceSoftware);
                    break;
            }
        }

        public void OnCellCreated(HexCell cell)
        {
            // TODO show alert for created unit on side panel
            healthBars.LinkHealthBar(cell);
        }

        private IEnumerator GameWonSequence(PlayerData winner)
        {
            yield return new WaitForSeconds(0.6f);
            gameOverUI.SetWinner(winner);
            gameOverUI.Show();
        }
    }
}