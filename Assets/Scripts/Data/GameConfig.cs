using System;
using Aspekt.Hex.Config;

namespace Aspekt.Hex
{
    [Serializable]
    public class GameConfig
    {
        public int startingSupply = 5;

        public TechConfig techConfig;

        public TechDetails GetTechDetails(Technology tech)
        {
            return techConfig.GetDetails(tech);
        }
    }
}