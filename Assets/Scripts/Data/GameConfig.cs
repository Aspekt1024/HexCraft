using System;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;

namespace Aspekt.Hex
{
    [Serializable]
    public class GameConfig
    {
        public int startingSupply = 5;

        public TechConfig techConfig;

        public UpgradeAction.UpgradeDetails GetUpgradeDetails(Technology tech)
        {
            return techConfig.GetDetails(tech);
        }
    }
}