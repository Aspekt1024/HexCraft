using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    [CreateAssetMenu(menuName = "Hex/UnitAction")]
    public class UnitAction : ActionDefinition
    {
        public UnitActions actionType;
        public string title;
        public string description;
        
        protected override Tooltip.Details GetTooltipRequirementsMet()
        {
            return new Tooltip.Details(
                title, 
                new Currency(),
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

        public override bool CanAfford(int playerId) => true;
        public override bool IsRequirementsMet(int playerId) => true;
    }
}