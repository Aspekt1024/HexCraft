using System.Collections.Generic;

namespace Aspekt.Hex
{
    public enum Upgrades
    {
        Warhorses = 1000,
        TransportHorses = 1010,
        MediumArmor = 2000,
        HeavyArmor = 2010,
        
    }
    
    public class UpgradeData
    {
        private readonly HashSet<Upgrades> upgrades = new HashSet<Upgrades>();

        public void AddUpgrade(Upgrades upgrade)
        {
            upgrades.Add(upgrade);
        }

        public bool HasUpgrade(Upgrades upgrade)
        {
            return upgrades.Contains(upgrade);
        }

        public bool HasUpgrades(params Upgrades[] upgradeList)
        {
            foreach (var upgrade in upgradeList)
            {
                if (!upgrades.Contains(upgrade)) return false;
            }

            return true;
        }
    }
}