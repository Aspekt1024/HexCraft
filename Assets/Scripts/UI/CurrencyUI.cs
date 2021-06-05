using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CurrencyUI : TooltipElement
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI supplies;
        [SerializeField] private TextMeshProUGUI production;
        [SerializeField] private TextMeshProUGUI population;
#pragma warning restore 649

        private CurrencyData data;

        public void UpdateCurrency(CurrencyData currencyData)
        {
            data = currencyData;
            supplies.text = currencyData.Supplies.ToString();
            production.text = currencyData.Production.Available + " (" + currencyData.Production.maximum + ")";
            population.text = currencyData.Population.Available + " (" + currencyData.Population.maximum + ")";
        }

        public override Tooltip.Details GetTooltipDetails(int playerId)
        {
            return new Tooltip.Details("Currency", new string[]
            {
                "Supplies: " + data.Supplies,
                "Max Production: " + data.Production.maximum + "\n" +
                "Utilised Production: " + data.Production.utilised + "\n" +
                "Available Production: " + data.Production.Available,
                "Max Population: " + data.Population.maximum + "\n" +
                "Utilised Population: " + data.Population.utilised + "\n" +
                "Available Population: " + data.Population.Available
            });
        }
    }
}