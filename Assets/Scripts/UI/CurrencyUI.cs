using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CurrencyUI : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI currency1;
        [SerializeField] private TextMeshProUGUI currency2;
        [SerializeField] private TextMeshProUGUI currency3;
#pragma warning restore 649

        public void UpdateCurrency(CurrencyData currencyData)
        {
            currency1.text = currencyData.Supplies.ToString();
            
            var availableProduction = currencyData.MaxProduction - currencyData.UtilisedProduction;
            currency2.text = availableProduction + " (" + currencyData.MaxProduction + ")";
        }
    }
}