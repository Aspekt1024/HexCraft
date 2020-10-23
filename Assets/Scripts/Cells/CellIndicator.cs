using System.Collections.Generic;
using System.Linq;
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
        
        private HexCell projectedCell;
        private UnitCell attackingUnit;
        private UnitCell movingUnit;
        
        private readonly List<GameObject> placementIndicatorPool = new List<GameObject>();
        private readonly List<GameObject> activePlacementIndicators = new List<GameObject>();

        public CellIndicator(Cells cells)
        {
            this.cells = cells;
        }

        public UnitCell GetMovingUnit() => movingUnit;
        public UnitCell GetAttackingUnit() => attackingUnit;

#region Indication API
        
        public void ShowBuild(Cells.CellTypes type, int playerID, HexCell origin)
        {
            HideAll();

            CellType = type;
            projectedCell = cells.Create(type);
            projectedCell.DisplayAsIndicator(cells.HoloMaterial, cells.GetColour(playerID));

            ShowPlacementGrid(type, origin);
        }

        public void IndicateAttack(UnitCell unit)
        {
            HideAll();
            attackingUnit = unit;
            // TODO show attack range
        }

        public void ShowMoveRange(UnitCell unit)
        {
            HideAll();
            movingUnit = unit;
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
            return activePlacementIndicators.Any(i =>
                HexCoordinates.FromPosition(i.transform.position).Equals(projectedCell.Coordinates));
        }

        private void ShowPlacementGrid(Cells.CellTypes type, HexCell origin)
        {
            var coords = cells.GetValidPlacement(type, origin);
            var indicators = GetPlacementIndicators(coords.Count);
            for (int i = 0; i < coords.Count; i++)
            {
                indicators[i].transform.position = HexCoordinates.ToPosition(coords[i]);
            }
        }

        private List<GameObject> GetPlacementIndicators(int numRequired)
        {
            var numIndicators = 0;
            foreach (var indicator in placementIndicatorPool)
            {
                if (indicator.activeSelf) continue;
                indicator.SetActive(true);
                activePlacementIndicators.Add(indicator);
                
                numIndicators++;
                if (numIndicators == numRequired)
                {
                    return activePlacementIndicators;
                }
            }

            var indicatorPrefab = cells.GetIndicatorPrefab();
            for (int i = 0; i < numRequired - numIndicators; i++)
            {
                var newIndicator = Object.Instantiate(indicatorPrefab, cells.transform);
                placementIndicatorPool.Add(newIndicator);
                activePlacementIndicators.Add(newIndicator);
            }

            return activePlacementIndicators;
        }

        private void HideIndicators()
        {
            foreach (var indicator in activePlacementIndicators)
            {
                indicator.SetActive(false);
            }
            activePlacementIndicators.Clear();
            
            // TODO set default cursor
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
                // TODO set cursor to invalid
            }
            else if (cells.IsValidAttackTarget(attackingUnit, target, attackingUnit.PlayerId))
            {
                // TODO set cursor to valid target
            }
            else
            {
                // TODO set cursor to invalid target 
            }
        }

        private void UpdateMoveIndication(Vector3 boardPosition)
        {
            if (movingUnit == null) return;

            var coords = HexCoordinates.FromPosition(boardPosition);
            if (cells.IsPieceInCell(coords))
            {
                // TODO set cursor to invalid
            }
            else if (cells.IsValidMove(movingUnit, coords, movingUnit.PlayerId))
            {
                // TODO set cursor to valid
            }
            else
            {
                // TODO set cursor to invalid position
            }
        }
    }
}