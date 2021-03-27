using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private TurnIndicator turnIndicator;
        [SerializeField] private Tooltip tooltip;
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
        private bool isCursorInUI;
        private HexCursor gameCursor;
        private HexCursor uiCursor;

        public void Init(NetworkGamePlayerHex player)
        {
            this.player = player;
            controlPanel.RegisterSingleObserver(player);
            turnIndicator.RegisterSingleObserver(player);
            SetGameCursor(HexCursor.Default);
            SetUICursor(HexCursor.Default);

            tooltip.SetPlayer(player);

            currency.RegisterObserver(tooltip);
            controlPanel.Init(tooltip);
            turnIndicator.Init(tooltip);
            
            tooltip.EnableTooltips();
        }

        private void Update()
        {
            tooltip.Tick();
        }

        public void Refresh()
        {
            controlPanel.Refresh();
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
                turnIndicator.SetTurn(null);
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
            
            turnIndicator.SetTurn(playerData);
        }

        public void SetPlayerActionCount(PlayerData data, int actionCount)
        {
            turnIndicator.SetActionCount(data, actionCount);
        }

        public void UpdateCurrency(CurrencyData currencyData)
        {
            currency.UpdateCurrency(currencyData);
        }

        public void ShowCellInfo(HexCell cell)
        {
            controlPanel.SetCellSelected(cell, player);
        }

        public void HideCellInfo()
        {
            controlPanel.SetCellSelected(null, null);
        }

        public void SetCursorInUI(bool isInUI)
        {
            isCursorInUI = isInUI;
            if (isInUI)
            {
                SetCursor(uiCursor);
            }
            else
            {
                SetCursor(gameCursor);
            }
        }

        public void SetGameCursor(HexCursor type)
        {
            gameCursor = type;
            if (isCursorInUI) return;
            SetCursor(type);
        }

        public void SetUICursor(HexCursor type)
        {
            uiCursor = type;
            if (isCursorInUI)
            {
                SetCursor(type);
            }
        }

        private void SetCursor(HexCursor type)
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
            cell.RegisterHealthObserver(healthBars);
        }

        public void OnCellRemoved(HexCell cell)
        {
            // TODO show alert for removed unit on side panel
            healthBars.OnCellRemoved(cell);
            cell.UnregisterHealthObserver(healthBars);
        }

        private IEnumerator GameWonSequence(PlayerData winner)
        {
            yield return new WaitForSeconds(0.6f);
            gameOverUI.SetWinner(winner);
            gameOverUI.Show();
        }
    }
}