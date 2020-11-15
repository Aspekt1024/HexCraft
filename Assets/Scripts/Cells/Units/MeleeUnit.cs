using System;
using UnityEngine;

namespace Aspekt.Hex
{
    public class MeleeUnit : UnitCell
    {
        public override Technology Technology { get; } = Technology.None;
        
#pragma warning disable 649
        [SerializeField] private GroundUnitModel groundUnitModel;
        [SerializeField] private MountedUnitModel mountedUnitModel;
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
            switch (tech)
            {
                case Technology.UpgradeWeapons1:
                    SetWeaponLevel(1);
                    break;
                case Technology.UpgradeWeapons2:
                    SetWeaponLevel(2);
                    break;
                case Technology.UpgradeWeapons3:
                    SetWeaponLevel(3);
                    break;
                case Technology.UpgradeArmor1:
                    SetArmorLevel(1);
                    break;
                case Technology.UpgradeArmor2:
                    SetArmorLevel(2);
                    break;
                case Technology.UpgradeArmor3:
                    SetArmorLevel(3);
                    break;
                case Technology.UpgradeWarMount1:
                    SetMountLevel(1);
                    break;
                case Technology.UpgradeWarMount2:
                    SetMountLevel(2);
                    break;
                case Technology.UpgradeWarMount3:
                    SetMountLevel(3);
                    break;
                case Technology.UpgradeShields1:
                    SetShieldLevel(1);
                    break;
                case Technology.UpgradeShields2:
                    SetShieldLevel(2);
                    break;
                case Technology.UpgradeShields3:
                    SetShieldLevel(3);
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
            armorLevel = level;
            currentModel.SetArmor(level);
        }

        private void SetWeaponLevel(int level)
        {
            weaponLevel = level;
            currentModel.SetWeapon(level);
        }

        private void SetShieldLevel(int level)
        {
            shieldLevel = level;
            currentModel.SetShield(level);
        }

        private void SetMountLevel(int level)
        {
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