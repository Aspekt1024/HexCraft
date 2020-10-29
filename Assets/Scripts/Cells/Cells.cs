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

        public enum Colours
        {
            White,
            Black
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
        [SerializeField] private PathIndicator pathIndicator;
        [SerializeField] private GameObject indicator;
        [SerializeField] private Shader holoShader;
#pragma warning restore 649
        
        public List<HexCell> AllCells { get; } = new List<HexCell>();

        private readonly List<ICellEventObserver> cellEventObservers = new List<ICellEventObserver>();

        private CellPathfinder pathfinder;

        private void Awake()
        {
            pathfinder = new CellPathfinder(this);
        }

        public void RegisterCellEventObserver(ICellEventObserver cellEventObserver)
        {
            cellEventObservers.Add(cellEventObserver);
        }

        public HexCell CreateIndicator(CellTypes type)
        {
            return CreateCell(type);
        }

        public void ShowPath(List<Vector3> path)
        {
            pathIndicator.ShowPath(path);
        }

        public void HidePath() => pathIndicator.Hide();

        public Colours GetColour(int playerId)
        {
            return playerId == 1 ? Colours.White : Colours.Black;
        }

        public HexCell Create(CellTypes type)
        {
            var cell = CreateCell(type);
            if (cell != null)
            {
                cell.Init(this);
                foreach (var observer in cellEventObservers)
                {
                    cell.RegisterObserver(observer);
                }
                AllCells.Add(cell);
            }

            return cell;
        }

        public Shader HoloShader => holoShader;
        
        public bool IsPieceInCell(HexCoordinates coordinates)
        {
            return AllCells.Any(c => c.IsPlaced && c.Coordinates.Equals(coordinates));
        }

        public bool IsPieceInCell(Vector2Int v2)
        {
            return AllCells.Any(c => c.IsPlaced && c.Coordinates.EqualsV2(v2));
        }

        public HexCell GetCellAtPosition(HexCoordinates coordinates)
        {
            return AllCells.FirstOrDefault(c => c.IsPlaced && c.Coordinates.Equals(coordinates));
        }

        public void RemoveCell(HexCoordinates coordinates)
        {
            var cell = AllCells.FirstOrDefault(c => c.Coordinates.Equals(coordinates));
            if (cell != null)
            {
                AllCells.Remove(cell);
                cell.Remove();
            }
        }
        
        public GameObject GetIndicatorPrefab()
        {
            return indicator.gameObject;
        }

        public List<HexCoordinates> GetValidPlacement(CellTypes type, HexCell origin, bool omitNonEmpty)
        {
            var radius = GetPrefab(type).PlacementRadius;
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
            
            foreach (var cell in AllCells)
            {
                if (cell.PlayerId == playerId && cell.CanCreate(type))
                {
                    var cellPrefab = GetPrefab(type);
                    if (PlacementRules.IsValidPlacementDistance(cellPrefab.PlacementRadius, cellCoords, cell.Coordinates))
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

        public List<Vector3> GetPathWithValidityCheck(HexCell cell, HexCoordinates targetCoords, int playerId)
        {
            if (!(cell is UnitCell unit) || cell.PlayerId != playerId) return null;
            return GetPath(unit, targetCoords);
        }

        public List<Vector3> GetPath(UnitCell unit, HexCoordinates targetCoords)
        {
            return pathfinder.FindPath(unit.Coordinates, targetCoords, unit.MoveRange);
        }
        
        public int GetCost(CellTypes type)
        {
            return GetPrefab(type).Cost;
        }

        private HexCell CreateCell(CellTypes type)
        {
            var cellPrefab = GetPrefab(type);
            return cellPrefab == null ? null : Instantiate(cellPrefab);
        }

        private HexCell GetPrefab(CellTypes type)
        {
            switch (type)
            {
                case CellTypes.Base:
                    return home;
                case CellTypes.Training:
                    return training;
                case CellTypes.Income:
                    return income;
                case CellTypes.UnitT1:
                    return unit1;
                case CellTypes.UnitT2:
                    return unit2;
                default:
                    Debug.LogError("invalid cell type: " + type);
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

        public List<HexCoordinates> GetCellsWithinDistance(HexCoordinates origin, int distance)
        {
            var pathCells = pathfinder.GetCellsWithinDistance(origin, distance);
            var coords = pathCells.Select(c => new HexCoordinates(c.X, c.Z)).ToList();
            return coords;
        }

    }
}