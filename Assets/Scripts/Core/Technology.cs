namespace Aspekt.Hex
{
    public enum Technology
    {
        Barracks = 1000,
        Blacksmith = 1100,
        Farm = 2000,
        Stables = 2100,
        Market = 2200,
        
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
        
    }

    public enum TechGroups
    {
        Buildings,
        UpgradeWeapons,
        UpgradeArmor,
        UpgradeWarMount,
        UpgradeShields,
        Undefined
    }
}