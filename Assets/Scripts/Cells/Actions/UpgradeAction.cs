using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Config;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    [CreateAssetMenu(menuName = "Hex/UpgradeAction")]
    public class UpgradeAction : ActionDefinition
    {
        [Serializable]
        public struct UpgradeDetails
        {
            public Sprite icon;
            public Technology tech;
        }

        [Tooltip("Defines the upgrade details per level")]
        public UpgradeDetails[] upgradeDetails;

        private UpgradeDetails currentLevelTech;
        private TechDetails currentLevelTechDetails;

        public override void Update()
        {
            GetCurrentLevelTechDetails();
        }

        public override Sprite GetIcon()
        {
            return currentLevelTech.icon;
        }

        public override bool CanDisplay()
        {
            var allTech = upgradeDetails.Select(d => d.tech).ToList();
            return !Data.IsTechAvailable(allTech, PlayerId);
        }

        public Technology GetNextTech()
        {
            return currentLevelTechDetails.technology;
        }
        
        protected override bool IsRequirementsMet()
        {
            return IsTechAvailable(currentLevelTechDetails.requiredTech);
        }

        protected override Tooltip.Details GetTooltipRequirementsMet()
        {
            return new Tooltip.Details(
                currentLevelTechDetails.title, 
                currentLevelTechDetails.cost, 0, 0, 
                1,
                new[] {currentLevelTechDetails.description}
            );
        }

        protected override Tooltip.Details GetTooltipRequirementsNotMet()
        {
            return new Tooltip.Details(
                currentLevelTechDetails.title,
                GenerateRequirementsText(currentLevelTechDetails.requiredTech));
        }

        private void GetCurrentLevelTechDetails()
        {
            // TODO this is an inefficient and potentially expensive operation. Profile for this!
            for (int i = 0; i < upgradeDetails.Length; i++)
            {
                if (Data.IsTechAvailable(upgradeDetails[i].tech, PlayerId)) continue;

                var details = Data.Config.GetTechDetails(upgradeDetails[i].tech);
                if (Data.IsTechAvailable(details.requiredTech, PlayerId))
                {
                    currentLevelTech = upgradeDetails[i];
                    currentLevelTechDetails = details;
                    return;
                }
            }
        }
    }
}