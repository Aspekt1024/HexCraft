using System;
using Aspekt.Hex.Actions;
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
        private Details details;

        [Serializable]
        public struct Details
        {
            public ActionDefinition definiton;
            public EventTrigger.TriggerEvent callback;

            public bool IsValid()
            {
                return definiton != null && callback != null;
            }
        }
        
        public void ShowActions(Details details)
        {
            if (!details.IsValid()) return;
            
            gameObject.SetActive(true);

            this.details = details;
            spriteRenderer.sprite = details.definiton.icon;
            onClickCallback = details.callback;
        }

        public void CellItemClicked()
        {
            onClickCallback?.Invoke(null);
        }

        public override Tooltip.Details GetTooltipDetails()
        {
            return details.definiton.GetTooltipDetails();
        }
    }
}