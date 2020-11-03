using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    [CreateAssetMenu(menuName = "Hex/BuildAction")]
    public class BuildAction : ActionDefinition
    {
        public HexCell prefab;
        
        public override Tooltip.Details GetTooltipDetails()
        {
            return new Tooltip.Details(
                GetBuildTitle(), 
                prefab.Cost, 0, 0, 
                1,
                new[] {prefab.BasicDescription}
            );
        }

        private string GetBuildTitle()
        {
            if (prefab is UnitCell unit)
            {
                return "Train " + unit.DisplayName;
            }
            return "Build " + prefab.DisplayName;
        }
    }
}