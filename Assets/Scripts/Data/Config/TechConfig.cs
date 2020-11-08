using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex.Config
{
    [Serializable]
    public class TechConfig
    {
        public TechGroup[] techGroups;

        public TechDetails GetDetails(Technology tech)
        {
            foreach (var group in techGroups)
            {
                foreach (var detail in group.details)
                {
                    if (detail.technology == tech) return detail;
                }
            }
            return null;
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