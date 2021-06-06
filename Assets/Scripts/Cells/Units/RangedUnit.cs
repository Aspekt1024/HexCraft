using System;
using System.Collections;
using System.Linq;
using Aspekt.Hex.Effects;
using UnityEngine;

namespace Aspekt.Hex
{
    public class RangedUnit : UnitCell
    {
#pragma warning disable 649
        [SerializeField] private GroundUnitModel groundUnitModel;
        [SerializeField] private MountedUnitModel mountedUnitModel;
        [SerializeField] private Arrow projectile;
#pragma warning restore 649
        
        private UnitModel currentModel;

        private int armorLevel;
        private int rangeLevel;
        private int damageLevel;

        public override string DisplayName => "Archer";
        public override Technology Technology { get; } = Technology.Archer;
        
        private void Awake()
        {
            Setup();
        }

        public override void SetupTech(GameData data, int playerId)
        {
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeRangedDamage, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeRangedDistance, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeArmor, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeWarMount, playerId));
        }

        [ContextMenu("setup")]
        public void Setup()
        {
            Stats.Range = 1;
            Stats.Speed = 1;
            
            SetModel(groundUnitModel);
            SetMount(GetUpgradeForLevel(TechGroups.UpgradeWarMount, 0));
            SetArmor(GetUpgradeForLevel(TechGroups.UpgradeArmor, 0));
            SetDamage(GetUpgradeForLevel(TechGroups.UpgradeRangedDamage, 0));
            SetRange(GetUpgradeForLevel(TechGroups.UpgradeRangedDistance, 0));
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
    
        [ContextMenu("range0")]
        public void SetRange0() => SetRange(GetUpgradeForLevel(TechGroups.UpgradeRangedDistance, 0));
        [ContextMenu("range1")]
        public void SetRange1() => SetRange(GetUpgradeForLevel(TechGroups.UpgradeRangedDistance, 1));
        [ContextMenu("range2")]
        public void SetRange2() => SetRange(GetUpgradeForLevel(TechGroups.UpgradeRangedDistance, 2));
        
        [ContextMenu("damage0")]
        public void SetDamage0() => SetMount(GetUpgradeForLevel(TechGroups.UpgradeRangedDamage, 0));
        [ContextMenu("damage1")]
        public void SetDamage1() => SetMount(GetUpgradeForLevel(TechGroups.UpgradeRangedDamage, 1));
        
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
                case TechGroups.UpgradeArmor:
                    SetArmor(upgrade);
                    break;
                case TechGroups.UpgradeWarMount:
                    SetMount(upgrade);
                    break;
                case TechGroups.UpgradeRangedDamage:
                    SetDamage(upgrade);
                    break;
                case TechGroups.UpgradeRangedDistance:
                    SetRange(upgrade);
                    break;
            }
        }

        public override void ShowAttack(HexCell target, Action attackHitCallback)
        {
            StartCoroutine(AttackRoutine(target, attackHitCallback));
        }

        protected override void SetColor(Color color)
        {
            currentModel.SetColor(color);
        }

        protected override void SetMaterial(Material material)
        {
            CellMaterial = material;
            currentModel.SetMaterial(material);
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

        private void SetDamage(UpgradeStats.Upgrade upgrade)
        {
            damageLevel = upgrade.level;
            Stats.Attack = upgrade.value;
            currentModel.SetBack(upgrade.level);
        }

        private void SetRange(UpgradeStats.Upgrade upgrade)
        {
            rangeLevel = upgrade.level;
            Stats.Range = upgrade.value;
            currentModel.SetWeapon(upgrade.level);
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
            currentModel.SetWeapon(rangeLevel);
            currentModel.SetBack(damageLevel);
        }
        
        private IEnumerator AttackRoutine(HexCell target, Action attackHitCallback)
        {
            Model.LookAt(target.transform);
            
            yield return new WaitForSeconds(0.3f);
            
            Anim.SetTrigger(AnimRangedAttackTrigger);
            yield return new WaitForSeconds(0.15f);

            var arrow = Instantiate(projectile);
            var startPos = currentModel.GetProjectileStartPos();
            var endPos = target.transform.position;
            endPos.y = 0.4f;
            
            arrow.Shoot(startPos, endPos, 4f, attackHitCallback);
        }
    }
}