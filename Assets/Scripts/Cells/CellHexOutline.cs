using System;
using UnityEngine;

namespace Aspekt.Hex
{
    public class CellHexOutline : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material redSelectedMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material blueSelectedMaterial;
        [SerializeField] private Material invalidPlacement;
#pragma warning restore 649

        private Material normalMaterial;
        private Material selectedMaterial;
        private MeshRenderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetColour(Cells.Colours colour)
        {
            switch (colour)
            {
                case Cells.Colours.Blue:
                    normalMaterial = blueMaterial;
                    selectedMaterial = blueSelectedMaterial;
                    break;
                case Cells.Colours.Red:
                    normalMaterial = redMaterial;
                    selectedMaterial = redSelectedMaterial;
                    break;
                default:
                    Debug.LogError("Invalid colour: " + colour);
                    normalMaterial = blueMaterial;
                    selectedMaterial = blueSelectedMaterial;
                    break;
            }
            SetUnselected();
        }

        public void SetSelected()
        {
            meshRenderer.materials = new[] {selectedMaterial};
        }

        public void SetUnselected()
        {
            meshRenderer.materials = new[] {normalMaterial};
        }

        public void SetInvalid()
        {
            meshRenderer.materials = new[] {invalidPlacement};
        }

        public void SetValid()
        {
            SetUnselected();
        }
    }
}