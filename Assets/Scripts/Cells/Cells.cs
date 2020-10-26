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

        public HexCell CreateIndicator(CellTypes type)
        {
            return CreateCell(type);
        }

        public HexCell Create(CellTypes type)
        {
            var cell = CreateCell(type);
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

        public List<HexCoordinates> GetValidPlacement(CellTypes type, HexCell origin, bool omitNonEmpty)
        {
            var radius = PlacementRules.GetRadius(type);
            if (radius > 0)
            {
                return GetSurroundingCells(origin.Coordinates, radius, omitNonEmpty);
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

        public bool IsValidAttackTarget(HexCell attackingUnit, HexCell target, int playerId)
        {
            if (!(attackingUnit is UnitCell unit)) return false;
            if (attackingUnit == null || attackingUnit.PlayerId != playerId) return false;
            if (target == null || target.PlayerId == playerId) return false;
            
            return unit.AttackRange >= HexCoordinates.Distance(attackingUnit.Coordinates, target.Coordinates);
        }
        
        public bool IsValidMove(HexCell cell, HexCoordinates targetCoords, int playerId)
        {
            if (!(cell is UnitCell unit)) return false;
            if (cell == null || cell.PlayerId != playerId) return false;
            if (IsPieceInCell(targetCoords)) return false;

            return unit.MoveRange >= HexCoordinates.Distance(cell.Coordinates, targetCoords);
        }

        private HexCell CreateCell(CellTypes type)
        {
            switch (type)
            {
                case CellTypes.Base:
                    return Instantiate(home);
                case CellTypes.Training:
                    return Instantiate(training);
                case CellTypes.Income:
                    return Instantiate(income);
                case CellTypes.UnitT1:
                    return Instantiate(unit1);
                case CellTypes.UnitT2:
                    return Instantiate(unit2);
                default:
                    return null;
            }
        }

        public List<HexCoordinates> GetSurroundingCells(HexCoordinates center, int radius, bool omitNonEmpty)
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
                    if (omitNonEmpty & IsPieceInCell(coord)) continue;
                    
                    coords.Add(coord);;
                }
            }
            
            return coords;
        }
    }
}