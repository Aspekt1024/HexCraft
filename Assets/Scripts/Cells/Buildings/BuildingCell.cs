using Aspekt.Hex.Actions;
using UnityEngine;

namespace Aspekt.Hex
{
    public abstract class BuildingCell : HexCell
    {
#pragma warning disable 649
        [SerializeField] private Renderer[] renderers;
#pragma warning disable 649
        
        public struct BuildingStats
        {
            public int Supplies;
            public int Production;
        }

        protected BuildingStats Stats;

        public BuildingStats GetStats() => Stats;

        public override void SetupTech(GameData data, int playerId)
        {
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

        public override float GetDamageMitigation() => 0f;

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