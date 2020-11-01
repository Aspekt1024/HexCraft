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
        
        private readonly List<GameObject> indicatorPool = new List<GameObject>();
        private readonly List<GameObject> activeIndicators = new List<GameObject>();

        public CellIndicator(Cells cells, GameUI ui)
        {
            this.cells = cells;
            this.ui = ui;
        }

#region Indication API
        
        public void ShowBuild(Cells.CellTypes type, int playerID, HexCell origin)
        {
            Clear();
            projectedCell = cells.CreateIndicator(type);
            projectedCell.DisplayAsIndicator(cells.HoloShader, cells.GetColour(playerID));

            ShowIndicatorGrid(type, origin, new Color(0f, 1f, 0f, 0.5f), true);
            ui.SetGameCursor(HexCursor.None);
        }

        public void ShowAttackGrid(HexCoordinates origin, int range, bool clearPreviousIndication = true)
        {
            if (clearPreviousIndication)
            {
                Clear();
            }
            var color = new Color(1f, 0.5f, 0.3f, 0.5f);
            var coords = cells.GetSurroundingCells(origin, range, false);
            ShowIndicatorGrid(coords, color);
        }

        public void ShowMovementGrid(HexCoordinates origin, int movement, bool clearPreviousIndication = true)
        {
            if (clearPreviousIndication)
            {
                Clear();
            }
            var color = new Color(1f, 1f, 0f, 0.5f);
            var coords = cells.GetCellsWithinDistance(origin, movement);
            ShowIndicatorGrid(coords, color);
        }
        
#endregion Indication API
        
#region Updates

        public void UpdateBuildPlacement(HexCoordinates coords)
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

        public void UpdateUnitIndication(UnitCell unit, HexCoordinates coords)
        {
            if (unit == null) return;
            
            var target = cells.GetCellAtPosition(coords);
            if (target != null)
            {
                if (cells.IsValidAttackTarget(unit, target, unit.PlayerId))
                {
                    ui.SetGameCursor(HexCursor.Attack);
                }
                else
                {
                    ui.SetGameCursor(HexCursor.Invalid);
                }
                return;
            }

            UpdateMoveIndication(unit, coords);
        }

        public void UpdateAttackIndication(UnitCell unit, HexCoordinates coords)
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

        public void UpdateMoveIndication(UnitCell unit, HexCoordinates coords)
        {
            if (cells.IsPieceInCell(coords))
            {
                ui.SetGameCursor(HexCursor.Invalid);
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
        
#endregion Updates

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
            if (projectedCell == null) return false;
            return activeIndicators.Any(i =>
                HexCoordinates.FromPosition(i.transform.position).Equals(projectedCell.Coordinates));
        }
        
        private void ShowIndicatorGrid(Cells.CellTypes type, HexCell origin, Color color, bool omitNonEmpty)
        {
            var coords = cells.GetValidPlacement(type, origin, omitNonEmpty);
            ShowIndicatorGrid(coords, color);
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
            var numIndicators = 0;
            foreach (var indicator in indicatorPool)
            {
                if (indicator.activeSelf) continue;
                indicator.SetActive(true);
                activeIndicators.Add(indicator);
                
                numIndicators++;
                if (numIndicators == numRequired)
                {
                    return activeIndicators;
                }
            }

            var indicatorPrefab = cells.GetIndicatorPrefab();
            for (int i = 0; i < numRequired - numIndicators; i++)
            {
                var newIndicator = Object.Instantiate(indicatorPrefab, cells.transform);
                indicatorPool.Add(newIndicator);
                activeIndicators.Add(newIndicator);
            }

            return activeIndicators;
        }

        private void HideIndicators()
        {
            foreach (var indicator in activeIndicators)
            {
                indicator.SetActive(false);
            }
            activeIndicators.Clear();
            
            ui.SetGameCursor(HexCursor.Default);
        }
    }
}