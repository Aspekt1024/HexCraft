using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Config;
using UnityEngine;

namespace Aspekt.Hex
{
    public class TechnologyData
    {
        private readonly TechConfig config;
        private readonly HashSet<Technology> upgrades = new HashSet<Technology>();

        public TechnologyData(TechConfig config)
        {
            this.config = config;
            upgrades.Add(Technology.None);
        }
        
        public void AddTechnology(Technology tech)
        {
            upgrades.Add(tech);
        }

        public bool HasTechnology(Technology tech)
        {
            return upgrades.Contains(tech);
        }

        public bool HasTechnologyForCell(Cells.CellTypes cellType)
        {
            var action = config.buildActions.FirstOrDefault(a => a.prefab.cellType == cellType);
            if (action == null)
            {
                Debug.LogError("Failed to find build action for " + cellType);
                return false;
            }
            return HasTechnologies(action.techRequirements);
        }

        public bool HasTechnologies(List<Technology> tech)
        {
            return tech == null || !tech.Any() || tech.All(t => upgrades.Contains(t));
        }

        public Technology GetTechLevel(TechGroups techGroup)
        {
            foreach (var groupData in config.upgrades)
            {
                if (groupData.group != techGroup) continue;
                
                for (int i = groupData.upgradeDetails.Length - 1; i >= 0; i--)
                {
                    if (HasTechnology(groupData.upgradeDetails[i].tech))
                    {
                        return groupData.upgradeDetails[i].tech;
                    }
                }
            }
            return Technology.None;
        }

        public TechnologyData Clone()
        {
            var techDataCopy = new TechnologyData(config);
            foreach (var upgrade in upgrades)
            {
                techDataCopy.AddTechnology(upgrade);
            }
            return techDataCopy;
        }

        public bool IsEqual(TechnologyData data)
        {
            return data.upgrades.Count == upgrades.Count && data.upgrades.All(u => upgrades.Contains(u));
        }
    }
}