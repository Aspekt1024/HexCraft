using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Behaviours;
using UnityEngine;

namespace Aspekt.Hex
{
    public class BuildingCell : HexCell
    {
#pragma warning disable 649
        [SerializeField] private Technology technology;
        [SerializeField] private BuildingBehaviour behaviour;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private List<UpgradeDetail> upgradeDetails;
#pragma warning disable 649

        [Serializable]
        public struct UpgradeDetail
        {
            public Technology tech;
            public string displayName;
            public Currency currencyBonus;
            public GameObject model;
        }
        
        private UpgradeDetail currentUpgrade;
        
        public override string DisplayName => string.IsNullOrEmpty(currentUpgrade.displayName)
            ? upgradeDetails[0].displayName
            : currentUpgrade.displayName;

        public Transform GetTransform() => transform;
        public Currency GetCurrencyBonus() => currentUpgrade.currencyBonus;

        public Currency GetCurrencyBonus(Technology techLevel) => upgradeDetails.FirstOrDefault(d => d.tech == techLevel).currencyBonus;
        public override float GetDamageMitigation() => 0f;

        public override Technology Technology => technology;

        public int CalculateSupplies(PlayerData data) => behaviour.CalculateSupplies(this, data);
        public int CalculateProduction(PlayerData data) => behaviour.CalculateProduction(this, data);
        public int CalculatePopulation(PlayerData data) => behaviour.CalculatePopulation(this, data);

        protected override void OnInit()
        {
            SetupTech(GameData, PlayerId);
        }

        public override void SetupTech(GameData data, int playerId)
        {
            if (!upgradeDetails.Any()) return;
            SetUpgradeLevel(Technology.None);
        }

        public override void OnTechAdded(Technology tech)
        {
            if (tech == Technology.None) return;
            SetUpgradeLevel(tech);
        }

        public override void OnTechRemoved(Technology tech)
        {
            if (tech == Technology.None) return;
            SetUpgradeLevel(tech);
        }

        public override void OnActionClicked(ActionDefinition actionDefinition)
        {
            switch (actionDefinition)
            {
                case BuildAction buildAction:
                    EventObservers.ForEach(o => o.IndicateBuildCell(buildAction, this));
                    break;
                case UpgradeAction upgradeAction:
                {
                    var tech = upgradeAction.GetNextTech();
                    EventObservers.ForEach(o => o.AddTech(tech));
                    break;
                }
                default:
                    Debug.LogError("only build and upgrade actions are defined for building cells");
                    break;
            }
        }

        public void SetUpgradeLevel(Technology tech)
        {
            var upgradeIndex = upgradeDetails.FindIndex(d => d.tech == tech);
            if (upgradeIndex < 0) return;
            
            currentUpgrade = upgradeDetails[upgradeIndex];
            foreach (var detail in upgradeDetails)
            {
                detail.model.SetActive(currentUpgrade.tech == detail.tech);
            }
        }

        protected override void SetMaterial(Material material)
        {
            CellMaterial = material;
            foreach (var r in renderers)
            {
                r.materials = new[] {material};
            }
        }

        protected override void SetColor(Color color)
        {
            foreach (var r in renderers)
            {
                var material = r.material;
                material.color = color;
                r.material = material;
            }
        }
    }
}