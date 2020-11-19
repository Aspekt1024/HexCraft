using System;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex
{
    public class MeleeUnit : UnitCell
    {
        public override Technology Technology { get; } = Technology.None;

        [Serializable]
        public struct UpgradeStats
        {
            [Serializable]
            public struct Upgrade
            {
                public Technology tech;
                public int level;
                public int value;    
            }
            
            public TechGroups techGroup;
            public Upgrade[] upgrades;

            public bool IsTechGroup(Technology tech)
            {
                return upgrades.Any(u => u.tech == tech);
            }
        }
        
#pragma warning disable 649
        [SerializeField] private GroundUnitModel groundUnitModel;
        [SerializeField] private MountedUnitModel mountedUnitModel;
        [SerializeField] private UpgradeStats[] upgradeStats;
#pragma warning restore 649

        private UnitModel currentModel;

        private int armorLevel;
        private int weaponLevel;
        private int shieldLevel;

        private void Awake()
        {
            SetModel(groundUnitModel);
        }

        public override void SetupTech(GameData data, int playerId)
        {
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeArmor, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeShields, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeWeapons, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeWarMount, playerId));
        }

#region Debug and Test

        [ContextMenu("setup")]
        public void Setup()
        {
            SetModel(groundUnitModel);
            SetMount0();
            SetArmor0();
            SetWeapon0();
            SetShield0();
        }

        [ContextMenu("armor0")]
        public void SetArmor0() => SetArmorLevel(0);
        [ContextMenu("armor1")]
        public void SetArmor1() => SetArmorLevel(1);
        [ContextMenu("armor2")]
        public void SetArmor2() => SetArmorLevel(2);
        [ContextMenu("armor3")]
        public void SetArmor3() => SetArmorLevel(3);
        
        [ContextMenu("weapon0")]
        public void SetWeapon0() => SetWeaponLevel(0);
        [ContextMenu("weapon1")]
        public void SetWeapon1() => SetWeaponLevel(1);
        [ContextMenu("weapon2")]
        public void SetWeapon2() => SetWeaponLevel(2);
        [ContextMenu("weapon3")]
        public void SetWeapon3() => SetWeaponLevel(3);
        
        [ContextMenu("shield0")]
        public void SetShield0() => SetShieldLevel(0);
        [ContextMenu("shield1")]
        public void SetShield1() => SetShieldLevel(1);
        [ContextMenu("shield2")]
        public void SetShield2() => SetShieldLevel(2);
        [ContextMenu("shield3")]
        public void SetShield3() => SetShieldLevel(3);
        
        [ContextMenu("mount0")]
        public void SetMount0() => SetMountLevel(0);
        [ContextMenu("mount1")]
        public void SetMount1() => SetMountLevel(1);
        [ContextMenu("mount2")]
        public void SetMount2() => SetMountLevel(2);
        [ContextMenu("mount3")]
        public void SetMount3() => SetMountLevel(3);
        
#endregion Debug and Test

        public override void OnTechAdded(Technology tech)
        {
            var stats = upgradeStats.FirstOrDefault(s => s.IsTechGroup(tech));
            if (stats.techGroup == TechGroups.Undefined) return;

            var upgrade = stats.upgrades.FirstOrDefault(u => u.tech == tech);
            if (upgrade.tech == Technology.None) return;
            
            switch (stats.techGroup)
            {
                case TechGroups.UpgradeWeapons:
                    SetWeaponLevel(upgrade.level);
                    Stats.Attack = upgrade.value;
                    break;
                case TechGroups.UpgradeArmor:
                    SetArmorLevel(upgrade.level);
                    Stats.Defense = upgrade.value;
                    break;
                case TechGroups.UpgradeWarMount:
                    SetMountLevel(upgrade.level);
                    Stats.Speed = upgrade.value;
                    break;
                case TechGroups.UpgradeShields:
                    SetShieldLevel(upgrade.level);
                    Stats.Shield = upgrade.value;
                    break;
            }
        }
        
        protected override void SetMaterial(Material material)
        {
            CellMaterial = material;
            groundUnitModel.SetMaterial(material);
            mountedUnitModel.SetMaterial(material);
        }

        protected override void SetColor(Color color)
        {
            groundUnitModel.SetColor(color);
            mountedUnitModel.SetColor(color);
        }

        private void SetArmorLevel(int level)
        {
            Stats.Defense = level;
            armorLevel = level;
            currentModel.SetArmor(level);
        }

        private void SetWeaponLevel(int level)
        {
            Stats.Attack = level;
            weaponLevel = level;
            currentModel.SetWeapon(level);
        }

        private void SetShieldLevel(int level)
        {
            Stats.Shield = level;
            shieldLevel = level;
            currentModel.SetShield(level);
        }

        private void SetMountLevel(int level)
        {
            Stats.Speed = level;
            if (level == 0)
            {
                SetModel(groundUnitModel);
            }
            else
            {
                SetModel(mountedUnitModel);
                mountedUnitModel.SetMountLevel(level - 1);
            }
            
            SetEquipmentLevels();
        }

        private void SetModel(UnitModel model)
        {
            Model = model.transform;
            currentModel = model;
            Anim = currentModel.GetComponent<Animator>();
            
            if (model == groundUnitModel)
            {
                groundUnitModel.SetEnabled();
                mountedUnitModel.SetDisabled();
            }
            else
            {
                groundUnitModel.SetDisabled();
                mountedUnitModel.SetEnabled();
            }
            
            SetEquipmentLevels();
        }

        private void SetEquipmentLevels()
        {
            currentModel.SetArmor(armorLevel);
            currentModel.SetWeapon(weaponLevel);
            currentModel.SetShield(shieldLevel);
        }
    }
}