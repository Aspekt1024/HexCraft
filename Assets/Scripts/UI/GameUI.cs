using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class GameUI : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private PlayerInfo player1;
        [SerializeField] private PlayerInfo player2;
        [SerializeField] private CellUI cellUI;
#pragma warning restore 649

        public void Init(NetworkGamePlayerHex player)
        {
            cellUI.Init(player);
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
    }
}