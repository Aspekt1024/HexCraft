using System;
using Aspekt.Hex.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace Aspekt.Hex.UI
{
    public class CellUIItem : TooltipElement
    {
#pragma warning disable 649
        [SerializeField] private Image spriteRenderer;
#pragma warning restore 649

        private ActionDefinition actionDefinition;
        private Action<ActionDefinition> actionCallback;

        public void ShowAction(ActionDefinition actionDefinition, int playerId, Action<ActionDefinition> actionCallback)
        {
            if (actionDefinition == null) return;
            
            gameObject.SetActive(true);

            this.actionDefinition = actionDefinition;
            this.actionCallback = actionCallback;
            
            actionDefinition.Refresh(playerId);
            spriteRenderer.sprite = actionDefinition.GetIcon();
        }

        public void CellItemClicked()
        {
            actionCallback?.Invoke(actionDefinition);
        }

        public override Tooltip.Details GetTooltipDetails(int playerId)
        {
            return actionDefinition.GetTooltipDetails(playerId);
        }
    }
}