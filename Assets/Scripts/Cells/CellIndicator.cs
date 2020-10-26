using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class CellIndicator
    {
        public bool IsPlacingCell => projectedCell != null;
        public bool IsMovingUnit => movingUnit != null;
        public bool IsAttacking => attackingUnit != null;
        
        public Cells.CellTypes CellType { get; private set; }

        private readonly Cells cells;
        private readonly GameUI ui;
        
        private HexCell projectedCell;
        private UnitCell attackingUnit;
        private UnitCell movingUnit;
        
        private readonly List<GameObject> indicatorPool = new List<GameObject>();
        private readonly List<GameObject> activeIndicators = new List<GameObject>();

        public CellIndicator(Cells cells, GameUI ui)
        {
            this.cells = cells;
            this.ui = ui;
        }

        public UnitCell GetMovingUnit() => movingUnit;
        public UnitCell GetAttackingUnit() => attackingUnit;

#region Indication API
        
        public void ShowBuild(Cells.CellTypes type, int playerID, HexCell origin)
        {
            HideAll();

            CellType = type;
            projectedCell = cells.CreateIndicator(type);
            projectedCell.DisplayAsIndicator(cells.HoloMaterial, cells.GetColour(playerID));

            ShowIndicatorGrid(type, origin, new Color(0f, 1f, 0f, 0.5f), true);
            ui.SetCursor(HexCursor.None);
        }

        public void IndicateAttack(UnitCell unit)
        {
            HideAll();
            attackingUnit = unit;
            ShowIndicatorGrid(unit.Coordinates, unit.AttackRange, new Color(1f, 0.5f, 0.3f, 0.5f), false);
        }

        public void ShowMoveRange(UnitCell unit)
        {
            HideAll();
            movingUnit = unit;
            ShowIndicatorGrid(unit.Coordinates, unit.MoveRange, new Color(1f, 1f, 0f, 0.5f), true);
        }
        
#endregion Indication API
        
        public void HideAll()
        {
            if (IsPlacingCell)
            {
                Object.Destroy(projectedCell.gameObject);
                projectedCell = null;
            }

            if (IsAttacking)
            {
                attackingUnit = null;
            }

            if (IsMovingUnit)
            {
                movingUnit = null;
            }

            HideIndicators();
        }
        
        public void Update(Vector3 boardPosition)
        {
            UpdateBuildPlacementProjection(boardPosition);
            UpdateAttackIndication(boardPosition);
            UpdateMoveIndication(boardPosition);
        }

        public bool IsProjectedCellInPlacementGrid()
        {
            if (projectedCell == null) return false;
            return activeIndicators.Any(i =>
                HexCoordinates.FromPosition(i.transform.position).Equals(projectedCell.Coordinates));
        }

        private void ShowIndicatorGrid(HexCoordinates origin, int radius, Color color, bool omitNonEmpty)
        {
            var coords = cells.GetSurroundingCells(origin, radius, omitNonEmpty);
            ShowIndicatorGrid(coords, color, omitNonEmpty);
        }

        private void ShowIndicatorGrid(Cells.CellTypes type, HexCell origin, Color color, bool omitNonEmpty)
        {
            var coords = cells.GetValidPlacement(type, origin, omitNonEmpty);
            ShowIndicatorGrid(coords, color, omitNonEmpty);
        }

        private void ShowIndicatorGrid(List<HexCoordinates> coords, Color color, bool omitNonEmpty)
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
            
            ui.SetCursor(HexCursor.Default);
        }

        private void UpdateBuildPlacementProjection(Vector3 boardPosition)
        {
            if (projectedCell == null) return;
            
            projectedCell.SetCoordinates(HexCoordinates.FromPosition(boardPosition));

            if (IsProjectedCellInPlacementGrid())
            {
                projectedCell.ShowAsValid();
            }
            else
            {
                projectedCell.ShowAsInvalid();
            }
        }

        private void UpdateAttackIndication(Vector3 boardPosition)
        {
            if (attackingUnit == null) return;
            
            var coords = HexCoordinates.FromPosition(boardPosition);
            var target = cells.GetCellAtPosition(coords);
            if (target == null)
            {
                ui.SetCursor(HexCursor.Invalid);
            }
            else if (cells.IsValidAttackTarget(attackingUnit, target, attackingUnit.PlayerId))
            {
                ui.SetCursor(HexCursor.Attack);
            }
            else
            {
                ui.SetCursor(HexCursor.Invalid);
            }
        }

        private void UpdateMoveIndication(Vector3 boardPosition)
        {
            if (movingUnit == null) return;

            var coords = HexCoordinates.FromPosition(boardPosition);
            if (cells.IsPieceInCell(coords))
            {
                ui.SetCursor(HexCursor.Invalid);
            }
            else if (cells.IsValidMove(movingUnit, coords, movingUnit.PlayerId))
            {
                ui.SetCursor(HexCursor.Move);
            }
            else
            {
                ui.SetCursor(HexCursor.Invalid);
            }
        }
    }
}