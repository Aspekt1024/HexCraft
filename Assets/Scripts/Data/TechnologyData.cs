using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Config;

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