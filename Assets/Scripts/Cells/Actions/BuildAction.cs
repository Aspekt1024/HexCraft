using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    [CreateAssetMenu(menuName = "Hex/BuildAction")]
    public class BuildAction : ActionDefinition
    {
        public HexCell prefab;
        public int placementRadius = 1;
        public List<Technology> techRequirements;

        public override bool CanAfford(int playerId)
        {
            return Data.CanAfford(playerId, prefab.Cost);
        }
        
        public override bool IsRequirementsMet(int playerId)
        {
            return IsTechAvailable(techRequirements, playerId);
        }

        protected override Tooltip.Details GetTooltipRequirementsMet()
        {
            return new Tooltip.Details(
                GetBuildTitle(),
                prefab.Cost,
                0,
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