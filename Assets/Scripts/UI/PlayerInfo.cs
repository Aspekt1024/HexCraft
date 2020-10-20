using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class PlayerInfo : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TextMeshProUGUI turnIndicator;
#pragma warning restore 649

        public NetworkGamePlayerHex player { get; private set; }

        public void SetupPlayerInfo(NetworkGamePlayerHex player)
        {
            this.player = player;
            
            var color = player.hasAuthority ? "blue" : "red";
            playerName.text = $"<color={color}>{player.DisplayName}</color>";
        }

        public void SetTurnIndicator(bool isPlayerTurn)
        {
            if (isPlayerTurn)
            {
                turnIndicator.text = player.hasAuthority ? "Your turn" : "Their turn";
            }
            else
            {
                turnIndicator.text = "";
            }
        }
    }
}