using UnityEngine;

namespace Aspekt.Hex
{
    public abstract class BuildingCell : HexCell
    {
#pragma warning disable 649
        [SerializeField] private Renderer[] renderers;
#pragma warning disable 649
        
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