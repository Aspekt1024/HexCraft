using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    [CreateAssetMenu(menuName = "Hex/BuildAction")]
    public class BuildAction : ActionDefinition
    {
        public HexCell prefab;
        public List<Technology> techRequirements;

        protected override bool IsRequirementsMet()
        {
            return IsTechAvailable(techRequirements);
        }

        protected override Tooltip.Details GetTooltipRequirementsMet()
        {
            return new Tooltip.Details(
                GetBuildTitle(),
                prefab.Cost,
                0, 0, 0,
                new[] {prefab.BasicDescription});
        }

        protected override Tooltip.Details GetTooltipRequirementsNotMet() {
            return new Tooltip.Details(
                GetBuildTitle(),
                GenerateRequirementsText(techRequirements)
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