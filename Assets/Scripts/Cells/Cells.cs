using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex
{
    public class Cells : MonoBehaviour
    {
        public enum CellTypes
        {
            None = 0,
            
            Base = 1000,
            Training = 1200,
            Income = 1300,
            Blacksmith = 1400,
            Market = 1500,
            MeleeUnit = 2000,
        }

        public enum Colours
        {
            Blue,
            Red
        }
        
#pragma warning disable 649
        [Header("Buildings")]
        [SerializeField] private HomeCell home;
        [SerializeField] private TrainingCell training;
        [SerializeField] private IncomeCell income;
        [SerializeField] private BlacksmithCell blacksmith;
        [SerializeField] private MarketCell market;
        
        [Header("Units")]
        [SerializeField] private HexCell meleeUnit;

        [Header("Display")]
        [SerializeField] private PathIndicator pathIndicator;
        [SerializeField] private GameObject indicator;
        [SerializeField] private Shader holoShader;
#pragma warning restore 649
        
        public List<HexCell> AllCells { get; } = new List<HexCell>();

        private readonly List<ICellEventObserver> cellEventObservers = new List<ICellEventObserver>();
        private readonly List<ICellLifecycleObserver> cellLifecycleObservers = new List<ICellLifecycleObserver>();

        private CellPathfinder pathfinder;
        private HexGrid grid;
        private GameData data;

        public void Init(HexGrid grid, GameData data)
        {
            this.grid = grid;
            this.data = data;
            pathfinder = new CellPathfinder(this, grid);
        }

        public void RegisterCellEventObserver(ICellEventObserver cellEventObserver)
        {
            cellEventObservers.Add(cellEventObserver);
        }

        public void RegisterCellLifecycleObserver(ICellLifecycleObserver observer)
        {
            cellLifecycleObservers.Add(observer);
        }

        public HexCell CreateIndicator(CellTypes type, int playerId)
        {
            var cell = CreateCell(type, playerId);
            cell.SetupTech(data, playerId);
            return cell;
        }

        public void ShowPath(List<Vector3> path)
        {
            pathIndicator.ShowPath(path);
        }

        public void HidePath() => pathIndicator.Hide();

        public Colours GetColour(int playerId)
        {
            return playerId == 1 ? Colours.Blue : Colours.Red;
        }

        public HexCell Create(CellTypes type, NetworkGamePlayerHex owner)
        {
            var cell = CreateCell(type, owner.ID);
            if (cell == null) return cell;
            
            cell.SetupTech(data, owner.ID);
            
            cell.Init(this, data, owner);
            AllCells.Add(cell);
            
            foreach (var observer in cellEventObservers)
            {
                cell.RegisterEventObserver(observer);
            }
                
            foreach (var observer in cellLifecycleObservers)
            {
                observer.OnCellCreated(cell);
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
                cellLifecycleObservers.ForEach(o => o.OnCellRemoved(cell));
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
            if (!grid.IsWithinGridBoundary(cellCoords) || IsPieceInCell(cellCoords)) return false;

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
            
            return unit.GetStats().Range >= HexCoordinates.Distance(attackingUnit.Coordinates, target.Coordinates);
        }

        public List<Vector3> GetPathWithValidityCheck(HexCell cell, HexCoordinates targetCoords, int playerId)
        {
            if (!(cell is UnitCell unit) || cell.PlayerId != playerId || unit.HasMoved) return null;
            return GetPath(unit, targetCoords);
        }

        public List<Vector3> GetPath(UnitCell unit, HexCoordinates targetCoords)
        {
            return pathfinder.FindPath(unit.Coordinates, targetCoords, unit.GetStats().Speed);
        }
        
        public Cost GetCost(CellTypes type)
        {
            return GetPrefab(type).Cost;
        }

        public HexCell GetPrefab(CellTypes type)
        {
            switch (type)
            {
                case CellTypes.Base:
                    return home;
                case CellTypes.Training:
                    return training;
                case CellTypes.Income:
                    return income;
                case CellTypes.MeleeUnit:
                    return meleeUnit;
                case CellTypes.Blacksmith:
                    return blacksmith;
                case CellTypes.Market:
                    return market;
                default:
                    Debug.LogError("invalid cell type: " + type);
                    return null;
            }
        }

        private HexCell CreateCell(CellTypes type, int playerId)
        {
            var cellPrefab = GetPrefab(type);
            if (cellPrefab == null) return null;
            var cell = Instantiate(cellPrefab);
            return cell;
        }

        public List<HexCoordinates> GetAttackableCells(HexCell cell, int range)
        {
            var coords = new List<HexCoordinates>();
            var center = cell.Coordinates;
            for (int x = center.X - range; x <= center.X + range; x++)
            {
                var zMin = center.Z - (x < center.X ? range - (center.X - x) : range);
                var zMax = center.Z + (x > center.X ? range - (x - center.X) : range);
                for (int z = zMin; z <= zMax; z++)
                {
                    if (x == center.X && z == center.Z) continue;

                    var coord = new HexCoordinates(x, z);
                    var hexCell = GetCellAtPosition(coord);
                    if (hexCell != null && hexCell.PlayerId != cell.PlayerId)
                    {
                        coords.Add(coord);
                    }
                }
            }

            return coords;
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
                    if (grid.IsWithinGridBoundary(coord))
                    {
                        coords.Add(coord);;
                    }
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

        public void OnNewTurn()
        {
            foreach (var cell in AllCells)
            {
                if (cell is UnitCell unit)
                {
                    unit.TurnReset();
                }
            }
        }

    }
}