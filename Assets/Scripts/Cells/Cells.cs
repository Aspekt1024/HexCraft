using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex
{
    public class Cells : MonoBehaviour
    {
        public enum CellTypes
        {
            Base = 1000,
            Training = 1200,
            Income = 1300,
            UnitT1 = 2010,
            UnitT2 = 2020,
        }
        
#pragma warning disable 649
        [Header("Buildings")]
        [SerializeField] private HomeCell home;
        [SerializeField] private TrainingCell training;
        [SerializeField] private IncomeCell income;
        
        [Header("Units")]
        [SerializeField] private HexCell unit1;
        [SerializeField] private HexCell unit2;
        
        [Header("Display")]
        [SerializeField] private GameObject indicator;
        [SerializeField] private Material standardMaterial;
        [SerializeField] private Material holoMaterial;
        [SerializeField] private Color blackColour;
        [SerializeField] private Color whiteColour;
#pragma warning restore 649
        
        private readonly List<HexCell> cells = new List<HexCell>();

        private readonly List<ICellEventObserver> cellEventObservers = new List<ICellEventObserver>();
        
        public void RegisterCellEventObserver(ICellEventObserver cellEventObserver)
        {
            cellEventObservers.Add(cellEventObserver);
        }

        public HexCell Create(CellTypes type)
        {
            HexCell cell = null;
            switch (type)
            {
                case CellTypes.Base:
                    cell = Instantiate(home);
                    break;
                case CellTypes.Training:
                    cell = Instantiate(training);
                    break;
                case CellTypes.Income:
                    cell = Instantiate(income);
                    break;
                case CellTypes.UnitT1:
                    cell = Instantiate(unit1);
                    break;
                case CellTypes.UnitT2:
                    cell = Instantiate(unit2);
                    break;
                default:
                    return null;
            }

            if (cell != null)
            {
                foreach (var observer in cellEventObservers)
                {
                    cell.RegisterObserver(observer);
                }
                cells.Add(cell);
            }

            return cell;
        }

        public Material HoloMaterial => holoMaterial;
        
        public Color GetColour(int playerId)
        {
            return playerId == 1 ? whiteColour : blackColour;
        }
        
        public bool IsPieceInCell(HexCoordinates coordinates)
        {
            return cells.Any(c => c.IsPlaced && c.Coordinates.Equals(coordinates));
        }

        public HexCell GetCellAtPosition(HexCoordinates coordinates)
        {
            return cells.FirstOrDefault(c => c.IsPlaced && c.Coordinates.Equals(coordinates));
        }

        public void RemoveCell(HexCoordinates coordinates)
        {
            var cell = cells.FirstOrDefault(c => c.Coordinates.Equals(coordinates));
            if (cell != null)
            {
                cells.Remove(cell);
                cell.Remove();
            }
        }
        
        public GameObject GetIndicatorPrefab()
        {
            return indicator.gameObject;
        }

        public List<HexCoordinates> GetValidPlacement(CellTypes type, HexCell origin)
        {
            var radius = PlacementRules.GetRadius(type);
            if (radius > 0)
            {
                return GetEmptySurroundingCells(origin.Coordinates, radius);
            }
            return new List<HexCoordinates>();
        }

        public bool IsValidPlacement(CellTypes type, HexCoordinates cellCoords, int playerId)
        {
            if (IsPieceInCell(cellCoords)) return false;

            if (type == CellTypes.Base) return true;
            
            foreach (var cell in cells)
            {
                if (cell.PlayerId == playerId && cell.CanCreate(type))
                {
                    if (PlacementRules.IsValidPlacementDistance(type, cellCoords, cell.Coordinates))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private List<HexCoordinates> GetEmptySurroundingCells(HexCoordinates center, int radius)
        {
            var coords = new List<HexCoordinates>();
            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                var zMin = center.Z - (x < center.X ? radius - (center.X - x) : radius);
                var zMax = center.Z + (x > center.X ? radius - (x - center.X) : radius);
                for (int z = zMin; z <= zMax; z++)
                {
                    if (x == center.X && z == center.Z) continue;
                    
                    var coord = new HexCoordinates(x, z);
                    if (!IsPieceInCell(coord))
                    {
                        coords.Add(coord);;
                    }
                }
            }
            
            return coords;
        }
    }
}