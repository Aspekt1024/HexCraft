using System;
using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public abstract class HexCell : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Renderer cellRenderer;
        [SerializeField] private Material blackMaterial;
        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material greenMaterial;
        [SerializeField] private Material yellowMaterial;
        [SerializeField] private Material brownMaterial;
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
        public NetworkGamePlayerHex Owner;
        
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
        
        private Material cellMaterial;
        
        public event Action<float> OnHealthUpdated = delegate {  };

        public void Init(Cells cells)
        {
            CellData = cells;
            OnInit();
        }
        
        public void RegisterObserver(ICellEventObserver observer)
        {
            Observers.Add(observer);
        }

        public void Place(HexCoordinates coords, Cells.Colours colour, NetworkGamePlayerHex owner)
        {
            IsPlaced = true;
            Owner = owner;
            PlayerId = owner.ID;
            CurrentHP = MaxHP;
            
            SetCoordinates(coords);
            SetMaterialFromColour(colour);
            ShowPlaced();
        }

        public virtual void TakeDamage(UnitCell attacker, int damage)
        {
            CurrentHP = Mathf.Max(CurrentHP - damage, 0);
            OnHealthUpdated((float) CurrentHP / MaxHP);
        }

        public void DisplayAsIndicator(Shader holoShader, Cells.Colours colour)
        {
            IsPlaced = false;
            SetMaterialFromColour(colour);
            ApplyHoloShader(holoShader);
            ShowAsInvalid();
        }

        public void SetCoordinates(HexCoordinates coords)
        {
            Coordinates = coords;
            transform.position = HexCoordinates.ToPosition(coords);
        }

        public virtual void MoveTo(HexCoordinates coords)
        {
            SetCoordinates(coords);
        }

        private void ShowPlaced()
        {
            // TODO animate
        }

        public void ShowAsInvalid()
        {
            SetColor(new Color(1f, 0f, 0f, 0.5f));
        }

        public void ShowAsValid()
        {
            SetColor(new Color(1f, 1f, 1f, 0.5f));
        }

        public abstract bool CanCreate(Cells.CellTypes cellType);
        protected abstract void OnInit();

        private void SetColor(Color color)
        {
            var material = cellRenderer.material;
            material.color = color;
            cellRenderer.material = material;
        }
        
        private void ApplyHoloShader(Shader shader)
        {
            var material = new Material(cellMaterial)
            {
                shader = shader
            };
            SetMaterial(material);
        }

        private void SetMaterial(Material material)
        {
            cellMaterial = material;
            cellRenderer.materials = new[] {material};
        }

        private void SetMaterialFromColour(Cells.Colours colour)
        {
            switch (colour)
            {
                case Cells.Colours.White:
                    SetMaterial(blueMaterial);
                    break;
                case Cells.Colours.Black:
                    SetMaterial(redMaterial);
                    break;
                default:
                    Debug.LogError("Invalid colour: " + colour);
                    SetMaterial(blackMaterial);
                    break;
            }
        }

        public virtual void Remove()
        {
            Destroy(gameObject);
        }
    }
}