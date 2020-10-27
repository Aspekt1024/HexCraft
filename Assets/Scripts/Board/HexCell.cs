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

        [Header("Cell Settings")]
        public string DisplayName = "Cell";
        public int PlacementRadius = 2;
        public int Cost = 2;
        public int MaxHP = 1;
        
        public const float OuterRadius = 1f;
        public const float InnerRadius = OuterRadius * 0.866025404f;

        public abstract List<CellUIItem.Details> ItemDetails { get; protected set; }
        
        [Header("Info")]
        public HexCoordinates Coordinates;
        public int PlayerId;
        
        public int CurrentHP { get; protected set; }

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

        protected Cells CellData;
        
        private Color cellColour;

        public void Init(Cells cells)
        {
            CellData = cells;
            OnInit();
        }
        
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
            ShowPlaced();
            CurrentHP = MaxHP;
        }

        public void TakeDamage(int damage)
        {
            CurrentHP = Mathf.Max(CurrentHP - damage, 0);
        }

        public void DisplayAsIndicator(Material holoMaterial, Color colour)
        {
            IsPlaced = false;
            ApplyHoloMaterial(holoMaterial);
            cellColour = colour;
            SetColor(colour, 0.5f);
            ShowAsInvalid();
        }

        public void SetCoordinates(HexCoordinates coords)
        {
            Coordinates = coords;
            transform.position = HexCoordinates.ToPosition(coords);
        }

        private void ShowPlaced()
        {
            // TODO animate
        }

        public void ShowAsInvalid()
        {
            SetColor(Color.red, 0.5f);
        }

        public void ShowAsValid()
        {
            SetColor(cellColour, 0.5f);
        }

        public abstract bool CanCreate(Cells.CellTypes cellType);
        protected abstract void OnInit();

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