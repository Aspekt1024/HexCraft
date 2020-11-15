using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class CellIndicator
    {
        private readonly Cells cells;
        private readonly GameUI ui;
        
        private HexCell projectedCell;
        
        private readonly List<GameObject> activeIndicators = new List<GameObject>();
        private readonly List<GameObject> inactiveIndicators = new List<GameObject>();

        public CellIndicator(Cells cells, GameUI ui)
        {
            this.cells = cells;
            this.ui = ui;
        }

#region Indication
        
        public void ShowBuild(Cells.CellTypes type, int playerID, HexCell origin)
        {
            Clear();
            
            ui.SetGameCursor(HexCursor.None);
            projectedCell = cells.CreateIndicator(type, playerID);
            projectedCell.DisplayAsIndicator(cells.HoloShader, cells.GetColour(playerID));

            var colour = new Color(0f, 1f, 0f, 0.5f);
            var coords = cells.GetValidPlacement(type, origin, true);
            ShowIndicatorGrid(coords, colour);
        }

        public void ShowMovementGrid(HexCoordinates origin, int movement)
        {
            Clear();
            var color = new Color(1f, 1f, 0f, 0.5f);
            var coords = cells.GetCellsWithinDistance(origin, movement);
            ShowIndicatorGrid(coords, color);
        }

        public void ShowAttackableCells(HexCell cell, int range, bool clearPreviousIndication = true)
        {
            if (clearPreviousIndication)
            {
                Clear();
            }
            var color = new Color(1f, 0.5f, 0.3f, 0.5f);
            var coords = cells.GetAttackableCells(cell, range);
            ShowIndicatorGrid(coords, color);
        }

        public void ShowAttackRange(HexCoordinates origin, int range)
        {
            Clear();
            var color = new Color(1f, 0.5f, 0.3f, 0.5f);
            var coords = cells.GetSurroundingCells(origin, range, false);
            ShowIndicatorGrid(coords, color);
        }
        
#endregion Indication
        
#region Cursor

        public void UpdateBuildCursor(HexCoordinates coords)
        {
            if (projectedCell == null) return;
            
            projectedCell.SetCoordinates(coords);

            if (IsProjectedCellInPlacementGrid())
            {
                projectedCell.ShowAsValid();
            }
            else
            {
                projectedCell.ShowAsInvalid();
            }
        }

        public void UpdateUnitCursor(UnitCell unit, HexCoordinates coords)
        {
            if (unit == null) return;

            if (!unit.HasAttacked)
            {
                var target = cells.GetCellAtPosition(coords);
                if (target != null)
                {
                    cells.HidePath();
                    if (target.PlayerId == unit.PlayerId)
                    {
                        ui.SetGameCursor(HexCursor.Default);
                    }
                    else if (cells.IsValidAttackTarget(unit, target, unit.PlayerId))
                    {
                        ui.SetGameCursor(HexCursor.Attack);
                    }
                    else
                    {
                        ui.SetGameCursor(HexCursor.Invalid);
                    }
                    return;
                }
            }

            if (unit.HasMoved)
            {
                ui.SetGameCursor(HexCursor.Default);
            }
            else
            {
                UpdateMoveCursor(unit, coords);
            }
        }

        public void UpdateAttackCursor(UnitCell unit, HexCoordinates coords)
        {
            var target = cells.GetCellAtPosition(coords);
            if (target == null)
            {
                ui.SetGameCursor(HexCursor.Invalid);
            }
            else if (cells.IsValidAttackTarget(unit, target, unit.PlayerId))
            {
                ui.SetGameCursor(HexCursor.Attack);
            }
            else
            {
                ui.SetGameCursor(HexCursor.Invalid);
            }
        }

        public void UpdateMoveCursor(UnitCell unit, HexCoordinates coords)
        {
            var cellAtCoord = cells.GetCellAtPosition(coords);
            if (cellAtCoord != null)
            {
                ui.SetGameCursor(cellAtCoord.PlayerId == unit.PlayerId
                    ? HexCursor.Default
                    : HexCursor.Invalid);
                
                cells.HidePath();
                return;
            }
            
            var path = cells.GetPath(unit, coords);
            if (path == null)
            {
                ui.SetGameCursor(HexCursor.Invalid);
                cells.HidePath();
                return;
            }
            
            ui.SetGameCursor(HexCursor.Move);
            cells.ShowPath(path);
        }
        
#endregion Cursor

        public void Clear()
        {
            if (projectedCell != null)
            {
                Object.Destroy(projectedCell.gameObject);
                projectedCell = null;
            }

            HideIndicators();
            cells.HidePath();
        }

        public bool IsProjectedCellInPlacementGrid()
        {
            // TODO this is only valid by technicality. Active indicators are all indicators,
            // and this will work only because during build, all active indicators are in range
            if (projectedCell == null) return false;
            return activeIndicators.Any(i =>
                HexCoordinates.FromPosition(i.transform.position).Equals(projectedCell.Coordinates));
        }

        private void ShowIndicatorGrid(List<HexCoordinates> coords, Color color)
        {
            var indicators = GetPlacementIndicators(coords.Count);
            for (int i = 0; i < coords.Count; i++)
            {
                indicators[i].transform.position = HexCoordinates.ToPosition(coords[i]);
                var mr = indicators[i].GetComponentInChildren<MeshRenderer>();
                mr.material.color = color;
            }
        }

        private List<GameObject> GetPlacementIndicators(int numRequired)
        {
            var retrievedIndiacators = new List<GameObject>();
            if (numRequired == 0) return retrievedIndiacators;
            
            for (int i = inactiveIndicators.Count - 1; i >= 0; i--)
            {
                var indicator = inactiveIndicators[i];
                inactiveIndicators.RemoveAt(i);
                
                indicator.SetActive(true);
                activeIndicators.Add(indicator);
                retrievedIndiacators.Add(indicator);
                
                if (retrievedIndiacators.Count == numRequired)
                {
                    return retrievedIndiacators;
                }
            }

            var indicatorPrefab = cells.GetIndicatorPrefab();
            var requiredNewIndicators = numRequired - retrievedIndiacators.Count;
            for (int i = 0; i < requiredNewIndicators; i++)
            {
                var newIndicator = Object.Instantiate(indicatorPrefab, cells.transform);
                activeIndicators.Add(newIndicator);
                retrievedIndiacators.Add(newIndicator);
            }

            return retrievedIndiacators;
        }

        private void HideIndicators()
        {
            foreach (var indicator in activeIndicators)
            {
                indicator.SetActive(false);
                inactiveIndicators.Add(indicator);
            }
            activeIndicators.Clear();
            
            ui.SetGameCursor(HexCursor.Default);
        }
    }
}