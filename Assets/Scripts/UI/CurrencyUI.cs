using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CurrencyUI : TooltipElement
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI currency1;
        [SerializeField] private TextMeshProUGUI currency2;
#pragma warning restore 649

        private CurrencyData data;

        public void UpdateCurrency(CurrencyData currencyData)
        {
            data = currencyData;
            currency1.text = currencyData.Supplies.ToString();
            
            var availableProduction = currencyData.MaxProduction - currencyData.UtilisedProduction;
            currency2.text = availableProduction + " (" + currencyData.MaxProduction + ")";
        }

        public override Tooltip.Details GetTooltipDetails(int playerId)
        {
            return new Tooltip.Details("Currency", new string[]
            {
                "Supplies: " + data.Supplies,
                "Max Production: " + data.MaxProduction + "\n" +
                "Utilised Production: " + data.UtilisedProduction + "\n" +
                "Available Production: " + data.AvailableProduction
            });
        }
    }
}