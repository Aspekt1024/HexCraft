using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex
{
    public class CellPathfinder
    {
        private readonly Cells cells;
        private readonly HexGrid grid;

        private readonly Vector2Int[] neighbourOffsets =
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1), 
        };

        private readonly List<PathCell> reachableCells = new List<PathCell>();
        private Vector2Int neighbour;
        
        public CellPathfinder(Cells cells, HexGrid grid)
        {
            this.cells = cells;
            this.grid = grid;
        }

        public List<Vector3> FindPath(HexCoordinates origin, HexCoordinates target, int maxDistance)
        {
            if (cells.IsPieceInCell(target)) return null;
            
            var distance = HexCoordinates.Distance(origin, target);
            var pathableCells = GetCellsWithinDistance(origin, distance);
            
            foreach (var cell in pathableCells)
            {
                if (cell.X == target.X && cell.Z == target.Z)
                {
                    return ConstructPath(cell, maxDistance);
                }
            }

            return null;
        }
        
        public List<PathCell> GetCellsWithinDistance(HexCoordinates origin, int distance)
        {
            reachableCells.Clear();
            reachableCells.Add(new PathCell(origin.X, origin.Z, 0, null));
            
            var cellsThisDepth = reachableCells;
            
            for (int dist = 1; dist <= distance; dist++)
            {
                cellsThisDepth = GetNewEmptyNeighbourCells(cellsThisDepth);
                reachableCells.AddRange(cellsThisDepth);
            }

            return reachableCells;
        }

        private List<PathCell> GetNewEmptyNeighbourCells(List<PathCell> pathCells)
        {
            var newCells = new List<PathCell>();
            foreach (var cell in pathCells)
            {
                foreach (var offset in neighbourOffsets)
                {
                    neighbour.x = cell.X + offset.x;
                    neighbour.y = cell.Z + offset.y;

                    if (cells.IsPieceInCell(neighbour)) continue;
                    
                    var reachableCell = new PathCell(neighbour.x, neighbour.y, cell.Dist + 1, cell);
                    if (!newCells.Contains(reachableCell) && !reachableCells.Contains(reachableCell))
                    {
                        if (grid.IsWithinGridBoundary(reachableCell.X, reachableCell.Z))
                        {
                            newCells.Add(reachableCell);
                        }
                    }
                }
            }
            return newCells;
        }

        private static List<Vector3> ConstructPath(PathCell target, int maxDistance)
        {
            var path = new Stack<Vector3>();
            var cell = target;
            while (cell != null)
            {
                if (path.Count - 1 >= maxDistance) return null;
                
                path.Push(HexCoordinates.ToPosition(cell.X, cell.Z));
                cell = cell.PreviousCell;
            }

            return path.ToList();
        }
    }
}