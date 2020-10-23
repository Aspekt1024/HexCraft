using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex
{
    public class CellIndicator
    {
        public bool IsPlacingCell => projectedCell != null;
        public Cells.CellTypes CellType { get; private set; }

        private readonly Cells cells;
        
        private HexCell projectedCell;
        private readonly List<GameObject> placementIndicatorPool = new List<GameObject>();
        private readonly List<GameObject> activePlacementIndicators = new List<GameObject>();

        public CellIndicator(Cells cells)
        {
            this.cells = cells;
        }

        public void Show(Cells.CellTypes type, int playerID, HexCell origin)
        {
            if (projectedCell != null)
            {
                Object.Destroy(projectedCell.gameObject);
            }

            CellType = type;
            projectedCell = cells.Create(type);
            projectedCell.DisplayAsIndicator(cells.HoloMaterial, cells.GetColour(playerID));

            ShowPlacementGrid(type, origin);
        }

        public void Hide()
        {
            if (projectedCell == null) return;
            Object.Destroy(projectedCell.gameObject);
            projectedCell = null;
            HidePlacementIndicators();
        }
        
        public void Update(Vector3 boardPosition)
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

        private void HidePlacementIndicators()
        {
            foreach (var indicator in activePlacementIndicators)
            {
                indicator.SetActive(false);
            }
            activePlacementIndicators.Clear();
        }
    }
}