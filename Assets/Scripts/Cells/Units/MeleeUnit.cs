using System;
using UnityEngine;

namespace Aspekt.Hex
{
    public class MeleeUnit : UnitCell
    {
#pragma warning disable 649
        [SerializeField] private GroundUnitModel groundUnitModel;
        [SerializeField] private MountedUnitModel mountedUnitModel;
#pragma warning restore 649

        private UnitModel currentModel;

        public void Init(int armorLevel, int weaponLevel, int mountLevel)
        {
            SetArmorLevel(armorLevel);
            SetWeaponLevel(weaponLevel);
            SetMountLevel(mountLevel);
        }

        [ContextMenu("setup")]
        public void Setup()
        {
            SetMount0();
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
        
        [ContextMenu("mount0")]
        public void SetMount0() => SetMountLevel(0);
        [ContextMenu("mount1")]
        public void SetMount1() => SetMountLevel(1);
        [ContextMenu("mount2")]
        public void SetMount2() => SetMountLevel(2);
        [ContextMenu("mount3")]
        public void SetMount3() => SetMountLevel(3);

        private int armorLevel;
        private int weaponLevel;
        
        public void SetArmorLevel(int level)
        {
            armorLevel = level;
            currentModel.SetArmor(level);
        }

        public void SetWeaponLevel(int level)
        {
            weaponLevel = level;
            currentModel.SetWeapon(level);
        }

        public void SetMountLevel(int level)
        {
            if (level == 0)
            {
                groundUnitModel.SetEnabled();
                mountedUnitModel.SetDisabled();
                currentModel = groundUnitModel;
                SetEquipmentLevels();
                return;
            }
            
            groundUnitModel.SetDisabled();
            mountedUnitModel.SetEnabled();
            currentModel = mountedUnitModel;
            mountedUnitModel.SetMountLevel(level - 1);
            
            SetEquipmentLevels();
        }

        private void SetEquipmentLevels()
        {
            currentModel.SetArmor(armorLevel);
            currentModel.SetWeapon(armorLevel);
        }
    }
}