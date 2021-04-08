using System.Linq;
using UnityEngine;

namespace Aspekt.Hex
{
    public class MagicUnit : UnitCell
    {
#pragma warning disable 649
        [SerializeField] private UnitModel model;
#pragma warning restore 649

        private int armorLevel;
        private int spellLevel;
        
        public override Technology Technology { get; } = Technology.None;
        
        protected override AttackTypes AttackType => AttackTypes.Spell;

        private void Awake()
        {
            Setup();
        }

        public override void SetupTech(GameData data, int playerId)
        {
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeMageArmor, playerId));
            OnTechAdded(data.GetCurrentLevelTech(TechGroups.UpgradeMageSpell, playerId));
        }

        [ContextMenu("setup")]
        public void Setup()
        {
            SetArmor(GetUpgradeForLevel(TechGroups.UpgradeMageArmor, 0));
            SetSpell(GetUpgradeForLevel(TechGroups.UpgradeMageSpell, 0));

            Stats.Range = 3;

            Model = model.transform;
            Anim = model.GetComponent<Animator>();
            
            model.SetArmor(armorLevel);
            model.SetWeapon(spellLevel);
        }
        
#region Debug and Test

        [ContextMenu("armor0")]
        public void SetArmor0() => SetArmor(GetUpgradeForLevel(TechGroups.UpgradeMageArmor, 0));
        [ContextMenu("armor1")]
        public void SetArmor1() => SetArmor(GetUpgradeForLevel(TechGroups.UpgradeMageArmor, 1));
        [ContextMenu("armor2")]
        public void SetArmor2() => SetArmor(GetUpgradeForLevel(TechGroups.UpgradeMageArmor, 2));
        [ContextMenu("armor3")]
        public void SetArmor3() => SetArmor(GetUpgradeForLevel(TechGroups.UpgradeMageArmor, 3));
    
        [ContextMenu("spell0")]
        public void SetSpell0() => SetSpell(GetUpgradeForLevel(TechGroups.UpgradeMageSpell, 0));
        [ContextMenu("spell1")]
        public void SetSpell1() => SetSpell(GetUpgradeForLevel(TechGroups.UpgradeMageSpell, 1));
        [ContextMenu("spell2")]
        public void SetSpell2() => SetSpell(GetUpgradeForLevel(TechGroups.UpgradeMageSpell, 2));
        [ContextMenu("spell3")]
        public void SetSpell3() => SetSpell(GetUpgradeForLevel(TechGroups.UpgradeMageSpell, 3));

#endregion Debug and Test

        public override void OnTechAdded(Technology tech)
        {
            if (tech == Technology.None) return;
            
            var stats = upgradeStats.FirstOrDefault(s => s.IsTechGroup(tech));
            if (stats.techGroup == TechGroups.Undefined) return;
            
            var upgrade = stats.upgrades.FirstOrDefault(u => u.tech == tech);
            switch (stats.techGroup)
            {
                case TechGroups.UpgradeMageArmor:
                    SetArmor(upgrade);
                    break;
                case TechGroups.UpgradeMageSpell:
                    SetSpell(upgrade);
                    break;
            }
        }

        protected override void SetColor(Color color)
        {
            model.SetColor(color);
        }

        protected override void SetMaterial(Material material)
        {
            CellMaterial = material;
            model.SetMaterial(material);
        }
        
        private void SetArmor(UpgradeStats.Upgrade upgrade)
        {
            armorLevel = upgrade.level;
            Stats.Defense = upgrade.value;

            var lostHP = MaxHP - CurrentHP;
            MaxHP = upgrade.value;
            CurrentHP = MaxHP - lostHP;
            
            model.SetArmor(upgrade.level);
        }

        private void SetSpell(UpgradeStats.Upgrade upgrade)
        {
            spellLevel = upgrade.level;
            Stats.Attack = upgrade.value;
            
            model.SetWeapon(upgrade.level);
        }
    }
}