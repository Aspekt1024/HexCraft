using System;
using Aspekt.Hex.Config;

namespace Aspekt.Hex
{
    [Serializable]
    public class GameConfig
    {
        public int startingCredits = 5;

        public TechConfig techConfig;

        public TechDetails GetTechDetails(Technology tech)
        {
            return techConfig.GetDetails(tech);
        }
    }
}