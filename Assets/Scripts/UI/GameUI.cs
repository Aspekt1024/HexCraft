using System;
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
    
    public class GameUI : MonoBehaviour
    {
#pragma warning disable 649
        [Header("HUD")]
        [SerializeField] private PlayerInfo player1;
        [SerializeField] private PlayerInfo player2;
        
        [Header("Overlay")]
        [SerializeField] private CellUI cellUI;
        
        [Header("Cursors")]
        [SerializeField] private Texture2D defaultCursor;
        [SerializeField] private Texture2D attackCursor;
        [SerializeField] private Texture2D moveCursor;
        [SerializeField] private Texture2D invalidCursor;
#pragma warning restore 649

        public void Init(NetworkGamePlayerHex player)
        {
            cellUI.Init(player);
            SetCursor(HexCursor.Default);
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

        public void SetPlayerTurn(NetworkGamePlayerHex player)
        {
            if (player1.player == player)
            {
                player1.SetTurnIndicator(true);
                player2.SetTurnIndicator(false);
            }
            else
            {
                player1.SetTurnIndicator(false);
                player2.SetTurnIndicator(true);
            }
        }

        public void ShowCellInfo(HexCell cell)
        {
            cellUI.Show(cell);
        }

        public void HideCellInfo()
        {
            cellUI.HideAll();
        }

        public void SetCursor(HexCursor type)
        {
            Cursor.visible = type != HexCursor.None;
            var center = new Vector2(16f, 16f);
            switch (type)
            {
                case HexCursor.Default:
                    Cursor.SetCursor(defaultCursor, center, CursorMode.ForceSoftware);
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
    }
}