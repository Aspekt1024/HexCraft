using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aspekt.Hex.UI
{
    public class CellUIItem : TooltipElement
    {
#pragma warning disable 649
        [SerializeField] private Image spriteRenderer;
#pragma warning restore 649

        private EventTrigger.TriggerEvent onClickCallback;
        private Details currentDetails;

        [Serializable]
        public struct Details
        {
            public Tooltip.Details tooltipDetails;
            public Sprite sprite;
            public EventTrigger.TriggerEvent callback;
        }
        
        public void ShowActions(Details details)
        {
            gameObject.SetActive(true);

            currentDetails = details;
            spriteRenderer.sprite = details.sprite;
            onClickCallback = details.callback;
        }

        public void CellItemClicked()
        {
            onClickCallback?.Invoke(null);
        }

        public override Tooltip.Details GetTooltipDetails()
        {
            return currentDetails.tooltipDetails;
        }
    }
}