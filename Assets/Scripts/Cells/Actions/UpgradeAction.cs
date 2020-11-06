using System;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    // TODO move to Aspekt.Hex namespace
    public enum UpgradeTypes
    {
        Armor = 1000,
        Weapon = 2000,
        Shield = 3000,
        Mount = 4000,
    }
    
    [CreateAssetMenu(menuName = "Hex/UpgradeAction")]
    public class UpgradeAction : ActionDefinition
    {
        [Serializable]
        public struct UpgradeDetails
        {
            public Sprite icon;
            public string title;
            public int cost;
            public string description;
        }

        [Tooltip("Defines the upgrade details per level")]
        public UpgradeDetails[] upgradeDetails;
        
        public override Tooltip.Details GetTooltipDetails()
        {
            var currentLevel = GetCurrentLevel();
            var details = upgradeDetails[currentLevel];
            
            return new Tooltip.Details(
                details.title, 
                details.cost, 0, 0, 
                1,
                new[] {details.description}
            );
        }

        private int GetCurrentLevel()
        {
            // TODO get upgrade level from player data
            return 0;
        }
    }
}