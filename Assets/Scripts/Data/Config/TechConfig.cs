using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using UnityEngine;

namespace Aspekt.Hex.Config
{
    [CreateAssetMenu(menuName = "Hex/Tech Config", fileName = "TechConfig")]
    public class TechConfig : ScriptableObject
    {
        public UpgradeAction[] upgrades;
        public BuildAction[] buildActions;
        
        private readonly Dictionary<Technology, UpgradeAction.UpgradeDetails> techDict = new Dictionary<Technology, UpgradeAction.UpgradeDetails>();

        private readonly HashSet<Technology> buildingTech = new HashSet<Technology>
        {
            Technology.Farm,
            Technology.Barracks,
            Technology.Blacksmith,
        };

        public bool IsBuildingTech(Technology tech) => buildingTech.Contains(tech);
        
        public UpgradeAction.UpgradeDetails GetDetails(Technology tech)
        {
            if (techDict.ContainsKey(tech))
            {
                return techDict[tech];
            }
            
            foreach (var upgradeGroup in upgrades)
            {
                foreach (var detail in upgradeGroup.upgradeDetails)
                {
                    if (detail.tech == tech)
                    {
                        techDict.Add(tech, detail);
                        return detail;
                    }
                }
            }
            return new UpgradeAction.UpgradeDetails{ tech = Technology.None };
        }

        public bool CanAddTech(Technology tech, PlayerData data)
        {
            if (buildingTech.Contains(tech)) return true;
            
            var techData = GetDetails(tech);
            return CanAddTech(techData, data);
        }

        public bool CanAddTech(UpgradeAction.UpgradeDetails details, PlayerData data)
        {
            if (data == null || details.tech == Technology.None) return false;

            if (data.TechnologyData.HasTechnology(details.tech)) return false;
            if (!data.TechnologyData.HasTechnologies(details.requiredTech)) return false;
            
            return data.CurrencyData.CanAfford(details.cost);
        }

        public bool CanRemoveTech(Technology tech, IEnumerable<HexCell> playerOwnedCells)
        {
            return playerOwnedCells.All(c => c.Technology != tech);
        }
    }
}