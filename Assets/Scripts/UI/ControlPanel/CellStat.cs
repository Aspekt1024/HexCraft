using Aspekt.Hex.UI;
using TMPro;
using UnityEngine;

namespace UI.ControlPanel
{
    public class CellStat : TooltipElement
    {
#pragma warning disable 649
        [SerializeField] private string statName;
        [SerializeField] private string statDescription;
        [SerializeField] private TextMeshProUGUI statText;
#pragma warning restore 649

        private int statValue;

        public void SetStat(int stat)
        {
            statValue = stat;
            
            if (stat == 0)
            {
                Hide();
                return;
            }
            
            gameObject.SetActive(true);
            statText.text = stat.ToString();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public override Tooltip.Details GetTooltipDetails(int playerId)
        {
            return new Tooltip.Details(statName, new[] {statValue.ToString(), statDescription});
        }
    }
}