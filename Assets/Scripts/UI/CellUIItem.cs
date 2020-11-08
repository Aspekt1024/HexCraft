using System;
using Aspekt.Hex.Actions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aspekt.Hex.UI
{
    public class CellUIItem : TooltipElement
    {
#pragma warning disable 649
        [SerializeField] private Image spriteRenderer;
#pragma warning restore 649

        private UnityEvent<ActionDefinition> onClickCallback;
        private Details details;

        [Serializable]
        public struct Details
        {
            public ActionDefinition definiton;
            public UnityEvent<ActionDefinition> callback;

            public bool IsValid()
            {
                return definiton != null && callback != null;
            }

            public bool CanDisplay() => definiton.CanDisplay();

            public void Update() => definiton.Update();
        }

        public void ShowAction(Details details)
        {
            if (!details.IsValid()) return;
            
            gameObject.SetActive(true);

            this.details = details;
            details.Update();
            spriteRenderer.sprite = details.definiton.GetIcon();
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