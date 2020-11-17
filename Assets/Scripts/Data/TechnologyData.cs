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
            foreach (var groupData in config.techGroups)
            {
                if (groupData.group != techGroup) continue;
                
                for (int i = groupData.details.Length - 1; i >= 0; i--)
                {
                    if (HasTechnology(groupData.details[i].technology))
                    {
                        return groupData.details[i].technology;
                    }
                }
            }
            return Technology.None;
        }
    }
}