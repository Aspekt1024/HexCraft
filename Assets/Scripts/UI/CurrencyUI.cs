using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CurrencyUI : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI credits;
#pragma warning restore 649

        public void SetCredits(int amount)
        {
            credits.text = amount.ToString();
        }
    }
}