using System.Collections;
using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public abstract class HexCell : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private CellUIItem.Details[] actions;
        [SerializeField] private CellHexOutline hexOutline;
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
        public string BasicDescription = "";
        
        public const float OuterRadius = 1f;
        public const float InnerRadius = OuterRadius * 0.866025404f;
        
        [Header("Info")]
        public HexCoordinates Coordinates;
        public int PlayerId;
        public NetworkGamePlayerHex Owner;

        public CellUIItem.Details[] Actions => actions;
        
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
        
        protected readonly List<ICellEventObserver> EventObservers = new List<ICellEventObserver>();
        private readonly List<ICellHealthObserver> healthObservers = new List<ICellHealthObserver>();

        protected Cells CellData;
        
        protected Material CellMaterial;

        public void Init(Cells cells, GameData data, NetworkGamePlayerHex owner)
        {
            CellData = cells;
            Owner = owner;
            PlayerId = Owner.ID;
            
            foreach (var action in actions)
            {
                action.definiton.Init(data, PlayerId);
            }
            OnInit();
        }
        
        public void RegisterEventObserver(ICellEventObserver observer)
        {
            EventObservers.Add(observer);
        }

        public void RegisterHealthObserver(ICellHealthObserver observer)
        {
            healthObservers.Add(observer);
        }

        public void Place(HexCoordinates coords)
        {
            IsPlaced = true;
            CurrentHP = MaxHP;
            
            SetCoordinates(coords);
            SetMaterialFromColour(CellData.GetColour(PlayerId));
            ShowPlaced();
        }

        public virtual void TakeDamage(UnitCell attacker, int damage)
        {
            var prevHealthPercent = (float) CurrentHP / MaxHP;
            CurrentHP = Mathf.Max(CurrentHP - damage, 0);
            var newHealthPercent = (float) CurrentHP / MaxHP;

            foreach (var observer in healthObservers)
            {
                observer.OnCellHealthChanged(this, prevHealthPercent, newHealthPercent);
            }
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
            hexOutline.SetInvalid();
        }

        public void ShowAsValid()
        {
            SetColor(new Color(1f, 1f, 1f, 0.5f));
            hexOutline.SetValid();
        }

        public void SetSelected() => hexOutline.SetSelected();
        public void SetUnselected() => hexOutline.SetUnselected();

        public abstract bool CanCreate(Cells.CellTypes cellType);
        protected virtual void OnInit() {}

        protected abstract void SetColor(Color color);
        
        private void ApplyHoloShader(Shader shader)
        {
            var material = new Material(CellMaterial)
            {
                shader = shader
            };
            SetMaterial(material);
        }

        protected abstract void SetMaterial(Material material);

        private void SetMaterialFromColour(Cells.Colours colour)
        {
            switch (colour)
            {
                case Cells.Colours.Blue:
                    SetMaterial(blueMaterial);
                    break;
                case Cells.Colours.Red:
                    SetMaterial(redMaterial);
                    break;
                default:
                    Debug.LogError("Invalid colour: " + colour);
                    SetMaterial(blackMaterial);
                    break;
            }
            
            hexOutline.SetColour(colour);
        }

        public virtual void Remove()
        {
            StartCoroutine(RemoveRoutine());
        }

        private IEnumerator RemoveRoutine()
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }
}