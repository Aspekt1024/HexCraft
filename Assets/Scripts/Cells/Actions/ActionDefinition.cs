using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    public abstract class ActionDefinition : ScriptableObject
    {
        public Sprite icon;

        public abstract Tooltip.Details GetTooltipDetails();
    }
}