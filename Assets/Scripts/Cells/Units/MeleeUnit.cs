using System;
using System.Collections;
using System.Linq;
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

        private int armorLevel;
        private int weaponLevel;
        private int shieldLevel;
        
        public override Technology Technology { get; } = Technology.None;

        private void Awake()
        {
            SetModel(groundUnitModel);
            Setup();
        }

        public override void SetupTech(GameData data, int playerId)
        {
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeArmor, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeShields, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeWeapons, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeWarMount, playerId));
        }

        [ContextMenu("setup")]
        public void Setup()
        {
            SetModel(groundUnitModel);
            SetMount(GetUpgradeForLevel(TechGroups.UpgradeWarMount, 0));
            SetArmor(GetUpgradeForLevel(TechGroups.UpgradeArmor, 0));
            SetWeapon(GetUpgradeForLevel(TechGroups.UpgradeWeapons, 0));
            SetShield(GetUpgradeForLevel(TechGroups.UpgradeShields, 0));

            Stats.Range = 1;
        }

#region Debug and Test

        [ContextMenu("armor0")]
        public void SetArmor0() => SetArmor(GetUpgradeForLevel(TechGroups.UpgradeArmor, 0));
        [ContextMenu("armor1")]
        public void SetArmor1() => SetArmor(GetUpgradeForLevel(TechGroups.UpgradeArmor, 1));
        [ContextMenu("armor2")]
        public void SetArmor2() => SetArmor(GetUpgradeForLevel(TechGroups.UpgradeArmor, 2));
        [ContextMenu("armor3")]
        public void SetArmor3() => SetArmor(GetUpgradeForLevel(TechGroups.UpgradeArmor, 3));
        
        [ContextMenu("weapon0")]
        public void SetWeapon0() => SetWeapon(GetUpgradeForLevel(TechGroups.UpgradeWeapons, 0));
        [ContextMenu("weapon1")]
        public void SetWeapon1() => SetWeapon(GetUpgradeForLevel(TechGroups.UpgradeWeapons, 1));
        [ContextMenu("weapon2")]
        public void SetWeapon2() => SetWeapon(GetUpgradeForLevel(TechGroups.UpgradeWeapons, 2));
        [ContextMenu("weapon3")]
        public void SetWeapon3() => SetWeapon(GetUpgradeForLevel(TechGroups.UpgradeWeapons, 3));

        [ContextMenu("shield0")]
        public void SetShield0() => SetShield(GetUpgradeForLevel(TechGroups.UpgradeShields, 0));
        [ContextMenu("shield1")]
        public void SetShield1() => SetShield(GetUpgradeForLevel(TechGroups.UpgradeShields, 1));
        [ContextMenu("shield2")]
        public void SetShield2() => SetShield(GetUpgradeForLevel(TechGroups.UpgradeShields, 2));
        [ContextMenu("shield3")]
        public void SetShield3() => SetShield(GetUpgradeForLevel(TechGroups.UpgradeShields, 3));
        
        [ContextMenu("mount0")]
        public void SetMount0() => SetMount(GetUpgradeForLevel(TechGroups.UpgradeWarMount, 0));
        [ContextMenu("mount1")]
        public void SetMount1() => SetMount(GetUpgradeForLevel(TechGroups.UpgradeWarMount, 1));
        [ContextMenu("mount2")]
        public void SetMount2() => SetMount(GetUpgradeForLevel(TechGroups.UpgradeWarMount, 2));
        [ContextMenu("mount3")]
        public void SetMount3() => SetMount(GetUpgradeForLevel(TechGroups.UpgradeWarMount, 3));
        
#endregion Debug and Test

        public override void OnTechAdded(Technology tech)
        {
            if (tech == Technology.None) return;
            
            var stats = upgradeStats.FirstOrDefault(s => s.IsTechGroup(tech));
            if (stats.techGroup == TechGroups.Undefined) return;
            
            var upgrade = stats.upgrades.FirstOrDefault(u => u.tech == tech);
            switch (stats.techGroup)
            {
                case TechGroups.UpgradeWeapons:
                    SetWeapon(upgrade);
                    break;
                case TechGroups.UpgradeArmor:
                    SetArmor(upgrade);
                    break;
                case TechGroups.UpgradeWarMount:
                    SetMount(upgrade);
                    break;
                case TechGroups.UpgradeShields:
                    SetShield(upgrade);
                    break;
            }
        }

        public override void ShowAttack(HexCell target, Action attackHitCallback)
        {
            StartCoroutine(AttackRoutine(target, attackHitCallback));
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

        private void SetArmor(UpgradeStats.Upgrade upgrade)
        {
            armorLevel = upgrade.level;
            Stats.Defense = upgrade.value;

            var lostHP = MaxHP - CurrentHP;
            MaxHP = upgrade.value;
            CurrentHP = MaxHP - lostHP;
            
            currentModel.SetArmor(upgrade.level);
        }

        private void SetWeapon(UpgradeStats.Upgrade upgrade)
        {
            weaponLevel = upgrade.level;
            Stats.Attack = upgrade.value;
            currentModel.SetWeapon(upgrade.level);
        }

        private void SetShield(UpgradeStats.Upgrade upgrade)
        {
            shieldLevel = upgrade.level;
            Stats.Shield = upgrade.value;
            currentModel.SetShield(upgrade.level);
        }

        private void SetMount(UpgradeStats.Upgrade upgrade)
        {
            Stats.Speed = upgrade.value;
            if (upgrade.level == 0)
            {
                SetModel(groundUnitModel);
            }
            else
            {
                SetModel(mountedUnitModel);
                mountedUnitModel.SetMountLevel(upgrade.level - 1);
            }
            
            SetEquipmentLevels();
        }

        private void SetModel(UnitModel model)
        {
            Model = model.transform;
            if (currentModel != null)
            {
                Model.rotation = currentModel.transform.rotation;
            }
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

        private IEnumerator AttackRoutine(HexCell target, Action attackHitCallback)
        {
            Model.LookAt(target.transform);
            Anim.SetTrigger(AnimAttackTrigger);
            yield return new WaitForSeconds(0.3f);
            attackHitCallback?.Invoke();
        }
    }
}