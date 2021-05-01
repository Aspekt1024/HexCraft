namespace Aspekt.Hex
{
    public enum Technology
    {
        None = 0,
        
        TownHall = 100,
        Barracks = 1000,
        Blacksmith = 1100,
        Farm = 2000,
        Stables = 2100,
        Market = 2200,
        
        Soldier = 2500,
        Mage = 2510,
        Archer = 2520,
        
        UpgradeWeapons1 = 3010,
        UpgradeWeapons2 = 3011,
        UpgradeWeapons3 = 3012,
        UpgradeArmor1 = 3110,
        UpgradeArmor2 = 3111,
        UpgradeArmor3 = 3112,
        UpgradeWarMount1 = 3210,
        UpgradeWarMount2 = 3211,
        UpgradeWarMount3 = 3212,
        UpgradeShields1 = 3310,
        UpgradeShields2 = 3311,
        UpgradeShields3 = 3312,
        
        UpgradeMageArmor1 = 4010,
        UpgradeMageArmor2 = 4011,
        UpgradeMageArmor3 = 4012,
        UpgradeMageSpell1 = 4110,
        UpgradeMageSpell2 = 4111,
        UpgradeMageSpell3 = 4112,
    }

    public enum TechGroups
    {
        Undefined = 0,
        Buildings = 1000,
        
        UpgradeWeapons = 2000,
        UpgradeArmor = 2100,
        UpgradeWarMount = 2200,
        UpgradeShields = 2300,
        
        UpgradeMageArmor = 3000,
        UpgradeMageSpell = 3100,
    }
}