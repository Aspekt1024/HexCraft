using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class Cells : MonoBehaviour
    {
        public enum CellTypes
        {
            None = 0,
            
            Home = 1000,
            Barracks = 1200,
            Farm = 1300,
            Blacksmith = 1400,
            Market = 1500,
            Stables = 1600,
            Archery = 1610,
            Granary = 1620,
            House = 1630,
            Library = 1640,
            MageTower = 1650,
            Temple = 1660,
            Tower = 1670,
            Workshop = 1680,
            
            MeleeUnit = 2000,
            MageUnit = 2200,
            ArcherUnit = 2400,
        }

        public enum Colours
        {
            Blue,
            Red
        }
        
#pragma warning disable 649
        [SerializeField] private List<HexCell> cellPrefabs;

        [Header("Display")]
        [SerializeField] private PathIndicator pathIndicator;
        [SerializeField] private GameObject indicator;
        [SerializeField] private Shader holoShader;
#pragma warning restore 649

        public List<HexCell> AllCells => allCells.Values.ToList();
        public List<HexCell> GetAllPrefabs() => cellPrefabs;
        private readonly Dictionary<int, HexCell> allCells = new Dictionary<int, HexCell>();
        private int lastCellIndex;

        private readonly List<ICellEventObserver> cellEventObservers = new List<ICellEventObserver>();
        private readonly List<ICellLifecycleObserver> cellLifecycleObservers = new List<ICellLifecycleObserver>();

        private CellPathfinder pathfinder;
        private HexGrid grid;
        private GameData data;
        private GameManager game;

        public void Init(GameManager game, HexGrid grid)
        {
            this.game = game;
            this.grid = grid;
            
            data = game.Data;
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

        public int GetUniqueCellID() => lastCellIndex++;

        public HexCell CreateIndicator(CellTypes type, int playerId)
        {
            var cell = CreateCell(type);
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

        public HexCell Create(CellTypes type, NetworkGamePlayerHex owner, int cellID)
        {
            var cell = CreateCell(type);
            if (cell == null) return cell;
            
            cell.SetupTech(data, owner.ID);
            
            cell.Init(this, data, owner);
            allCells.Add(cellID, cell);
            
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
        
        public HexCell GetCell(int cellID)
        {
            return allCells.ContainsKey(cellID) ? allCells[cellID] : null;
        }

        public void RemoveCell(HexCell c)
        {
            if (c == null) return;
            
            allCells.Remove(c.ID);
            cellLifecycleObservers.ForEach(o => o.OnCellRemoved(c));
            c.Remove();
        }
        
        public GameObject GetIndicatorPrefab()
        {
            return indicator.gameObject;
        }

        public List<HexCoordinates> GetValidPlacement(CellTypes type, HexCell origin, bool omitNonEmpty)
        {
            var radius = origin.GetPlacementRadius(type);
            if (radius > 0)
            {
                return GetSurroundingCells(origin.Coordinates, radius, omitNonEmpty);
            }
            return new List<HexCoordinates>();
        }

        public bool IsValidPlacement(CellTypes type, HexCoordinates cellCoords, int playerId)
        {
            if (!grid.IsWithinGridBoundary(cellCoords) || IsPieceInCell(cellCoords)) return false;

            if (type == CellTypes.Home) return true;
            
            foreach (var cell in AllCells)
            {
                if (cell.PlayerId == playerId && cell.CanCreate(type))
                {
                    var radius = cell.GetPlacementRadius(type);
                    if (PlacementRules.IsValidPlacementDistance(radius, cellCoords, cell.Coordinates))
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
        
        public Currency GetCost(CellTypes type)
        {
            return GetPrefab(type).Cost;
        }

        public HexCell GetPrefab(CellTypes type)
        {
            var cell = cellPrefabs.FirstOrDefault(c => c.cellType == type);
            if (cell == null)
            {
                Debug.LogError($"Invalid cell type: {type}");
            }
            return cell;
        }

        private HexCell CreateCell(CellTypes type)
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

        public void OnNewTurn(PlayerData playerData)
        {
            if (playerData.Player.hasAuthority && playerData.TurnNumber > 1)
            {
                var suppliers = GetSuppliers(playerData.Player.ID);
                suppliers.ForEach(s => game.UI.ShowFloatingUI(s.GetTransform(), $"+{s.CalculateSupplies(playerData)}", FloatingUI.Style.Supplies));
            }
                
            foreach (var cell in AllCells)
            {
                if (cell is UnitCell unit)
                {
                    unit.TurnReset();
                }
            }
        }

        public List<BuildingCell> GetSuppliers(int playerID)
        {
            var playerCells = game.Cells.AllCells.Where(c => c.Owner.ID == playerID).ToList();
            return GetSuppliers(playerCells);
        }

        public static List<BuildingCell> GetSuppliers(List<HexCell> playerCells)
        {
            var suppliers = new List<BuildingCell>();
            var hasMarket = false;
            foreach (var cell in playerCells)
            {
                if (!(cell is BuildingCell buildingCell) || buildingCell.GetCurrencyBonus().supplies <= 0) continue;
                if (buildingCell.cellType == CellTypes.Market)
                {
                    if (!hasMarket)
                    {
                        hasMarket = true;
                        suppliers.Add(buildingCell);
                    }
                }
                else
                {
                    suppliers.Add(buildingCell);
                }
            }

            return suppliers;
        }
    }
}