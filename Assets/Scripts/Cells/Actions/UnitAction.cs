using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    [CreateAssetMenu(menuName = "Hex/UnitAction")]
    public class UnitAction : ActionDefinition
    {
        public string title;
        public string description;
        
        public override Tooltip.Details GetTooltipDetails()
        {
            return new Tooltip.Details(
                title, 
                0, 0, 0, 
                1,
                new[] {description}
            );
        }
    }
}