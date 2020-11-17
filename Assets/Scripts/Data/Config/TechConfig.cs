using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;

namespace Aspekt.Hex.Config
{
    [Serializable]
    public class TechConfig
    {
        public TechGroup[] techGroups;
        public BuildAction[] buildActions;
        
        private readonly Dictionary<Technology, TechDetails> techDict = new Dictionary<Technology, TechDetails>();

        private readonly HashSet<Technology> buildingTech = new HashSet<Technology>
        {
            Technology.Farm,
            Technology.Barracks,
            Technology.Blacksmith,
        };

        public bool IsBuildingTech(Technology tech) => buildingTech.Contains(tech);
        
        public TechDetails GetDetails(Technology tech)
        {
            if (techDict.ContainsKey(tech))
            {
                return techDict[tech];
            }
            
            foreach (var group in techGroups)
            {
                foreach (var detail in group.details)
                {
                    if (detail.technology == tech)
                    {
                        techDict.Add(tech, detail);
                        return detail;
                    }
                }
            }
            return null;
        }

        public bool CanAddTech(Technology tech, PlayerData data)
        {
            if (buildingTech.Contains(tech)) return true;
            
            var techData = GetDetails(tech);
            return CanAddTech(techData, data);
        }

        public bool CanAddTech(TechDetails techData, PlayerData data)
        {
            if (data == null || techData == null) return false;

            if (data.TechnologyData.HasTechnology(techData.technology)) return false;
            if (!data.TechnologyData.HasTechnologies(techData.requiredTech)) return false;
            
            return data.Credits >= techData.cost;
        }

        public bool CanRemoveTech(Technology tech, IEnumerable<HexCell> playerOwnedCells)
        {
            return playerOwnedCells.All(c => c.Technology != tech);
        }
    }

    [Serializable]
    public class TechGroup
    {
        public TechGroups group;
        public TechDetails[] details;
    }
    
    [Serializable]
    public class TechDetails
    {
        public string title;
        public int cost;
        public string description;
        public Technology technology;
        public List<Technology> requiredTech;
    }
    
}