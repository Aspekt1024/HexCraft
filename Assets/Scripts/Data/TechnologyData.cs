using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex
{
    public class TechnologyData
    {
        private readonly HashSet<Technology> upgrades = new HashSet<Technology>();

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
    }
}