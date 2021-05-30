using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    [CreateAssetMenu(menuName = "Hex/UpgradeAction")]
    public class UpgradeAction : ActionDefinition
    {
        public const string AssetPath = "Bundles/Data/UpgradeActions";
        
        [Serializable]
        public struct UpgradeDetails
        {
            public Technology tech;
            public Sprite icon;
            public string title;
            public Currency cost;
            public string description;
            public List<Technology> requiredTech;
        }

        public TechGroups group;

        [Tooltip("Defines the upgrade details per level")]
        public UpgradeDetails[] upgradeDetails;

        private UpgradeDetails currentLevelTech;

        public override void Refresh(int playerId)
        {
            GetCurrentLevelTechDetails(playerId);
        }

        public override bool CanAfford(int playerId)
        {
            return Data.CanAfford(playerId, currentLevelTech.cost);
        }

        public override Sprite GetIcon()
        {
            return currentLevelTech.icon;
        }

        public override bool CanDisplay(int playerId)
        {
            var allTech = upgradeDetails.Select(d => d.tech).ToList();
            return !Data.IsTechAvailable(allTech, playerId);
        }

        public Technology GetNextTech()
        {
            return currentLevelTech.tech;
        }
        
        public override bool IsRequirementsMet(int playerId)
        {
            return IsTechAvailable(currentLevelTech.requiredTech, playerId);
        }

        protected override Tooltip.Details GetTooltipRequirementsMet()
        {
            return new Tooltip.Details(
                currentLevelTech.title, 
                currentLevelTech.cost,
                0,
                new[] {currentLevelTech.description}
            );
        }

        protected override Tooltip.Details GetTooltipRequirementsNotMet()
        {
            return new Tooltip.Details(
                currentLevelTech.title,
                GenerateRequirementsText(currentLevelTech.requiredTech));
        }

        private void GetCurrentLevelTechDetails(int playerId)
        {
            for (int i = 0; i < upgradeDetails.Length; i++)
            {
                if (Data.IsTechAvailable(upgradeDetails[i].tech, playerId)) continue;
                if (Data.IsTechAvailable(upgradeDetails[i].requiredTech, playerId))
                {
                    currentLevelTech = upgradeDetails[i];
                    return;
                }
            }
        }
    }
}