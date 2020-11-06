using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class GameOverUI : UIElement
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI winnerText;
#pragma warning restore 649

        public void SetWinner(PlayerData winner)
        {
            winnerText.text = winner.Player.DisplayName + " wins!";
        }
    }
}