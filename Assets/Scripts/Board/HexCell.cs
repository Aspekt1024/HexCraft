using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public abstract class HexCell : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private MeshRenderer meshRenderer;
#pragma warning restore 649
        
        public const float OuterRadius = 1f;
        public const float InnerRadius = OuterRadius * 0.866025404f;

        public abstract string DisplayName { get; }
        public abstract List<CellUIItem.Details> ItemDetails { get; protected set; }
        
        public HexCoordinates Coordinates;
        public int PlayerId;

        public bool IsPlaced { get; private set; }

        public static readonly Vector3[] corners = {
            new Vector3(0f, 0f, OuterRadius),
            new Vector3(InnerRadius, 0f, 0.5f * OuterRadius),
            new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
            new Vector3(0f, 0f, -OuterRadius),
            new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius),
            new Vector3(-InnerRadius, 0f, 0.5f * OuterRadius),
            new Vector3(0f, 0f, OuterRadius), 
        };
        
        protected readonly List<ICellEventObserver> Observers = new List<ICellEventObserver>();

        public void RegisterObserver(ICellEventObserver observer)
        {
            Observers.Add(observer);
        }

        public void Place(HexCoordinates coords, Color colour, int playerId)
        {
            IsPlaced = true;
            SetCoordinates(coords);
            SetColor(colour);
            PlayerId = playerId;
        }

        public void DisplayAsIndicator(Material holoMaterial, Color colour)
        {
            IsPlaced = false;
            ApplyHoloMaterial(holoMaterial);
            SetColor(colour, 0.5f);
        }

        public void SetCoordinates(HexCoordinates coords)
        {
            Coordinates = coords;
            transform.position = HexCoordinates.ToPosition(coords);
        }

        public void ShowPlaced()
        {
            // TODO animate
        }

        private void SetColor(Color color, float alpha = 1f)
        {
            color.a = alpha;
            meshRenderer.material.color = color;
        }

        private void ApplyHoloMaterial(Material material)
        {
            var colour = material.color;
            colour.a = 0.5f;
            material.color = colour;
            
            foreach (var mr in GetComponentsInChildren<MeshRenderer>())
            {
                mr.materials = new[] {material};
            }
        }

        public void Remove()
        {
            // TODO particle effects!
            Destroy(gameObject);
        }
    }
}