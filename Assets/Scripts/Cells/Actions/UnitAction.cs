using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    [CreateAssetMenu(menuName = "Hex/UnitAction")]
    public class UnitAction : ActionDefinition
    {
        public string title;
        public string description;
        
        protected override Tooltip.Details GetTooltipRequirementsMet()
        {
            return new Tooltip.Details(
                title, 
                0, 0, 0, 
                1,
                new[] {description}
            );
        }

        protected override Tooltip.Details GetTooltipRequirementsNotMet()
        {
            return new Tooltip.Details(
                title,
                "You cannot perform this action right now.");
        }

        protected override bool IsRequirementsMet()
        {
            // TODO return is player turn
            return true;
        }
    }
}