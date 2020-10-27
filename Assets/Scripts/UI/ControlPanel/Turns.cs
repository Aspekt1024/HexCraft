using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI.Control
{
    public class Turns : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private GameObject playerControls;
        [SerializeField] private GameObject opponentDetails;
        [SerializeField] private TextMeshProUGUI opponentTurnText;
        [SerializeField] private TextMeshProUGUI turnText;
#pragma warning restore 649

        private void Awake()
        {
            playerControls.SetActive(false);
            opponentDetails.SetActive(false);
            SetTurnNumber(1);
        }

        public void SetTurnNumber(int turnNumber)
        {
            turnText.text = "Turn: " + turnNumber;
        }

        public void SetPlayerTurn(NetworkGamePlayerHex player)
        {
            if (player == null)
            {
                playerControls.SetActive(false);
                opponentDetails.SetActive(false);
                return;
            }
            
            opponentTurnText.text = player.DisplayName + "'s turn";
                
            playerControls.SetActive(player.hasAuthority);
            opponentDetails.SetActive(!player.hasAuthority);
        }
    }
}