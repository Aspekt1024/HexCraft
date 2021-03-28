using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex.UI.Control
{
    public class CellControl : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private CellDetails cellDetails;
        [SerializeField] private List<CellUIItem> cellActions;
#pragma warning restore 649

        private HexCell currentCell;
        private NetworkGamePlayerHex queryingPlayer;

        private void Awake()
        {
            ClearDetails();
        }

        public void Init(Tooltip tooltip)
        {
            cellDetails.Init(tooltip);
            
            foreach (var actionItem in cellActions)
            {
                actionItem.RegisterObserver(tooltip);
            }
        }

        public void Refresh()
        {
            if (currentCell == null || queryingPlayer == null) return;
            ShowCellDetails(currentCell, queryingPlayer);
        }

        public void SetCellDetails(HexCell cell, NetworkGamePlayerHex queryingPlayer)
        {
            this.queryingPlayer = queryingPlayer;
            if (cell == null)
            {
                ClearDetails();
            }
            else
            {
                ShowCellDetails(cell, queryingPlayer);
            }
        }

        public void OnCellRemoved(HexCell cell)
        {
            if (cell == currentCell)
            {
                ClearDetails();
            }
        }

        private void ShowCellDetails(HexCell cell, NetworkGamePlayerHex queryingPlayer)
        {
            currentCell = cell;
            cellDetails.DisplayCellDetails(cell);

            if (cell.PlayerId == queryingPlayer.ID)
            {
                ShowCellActions(cell);
            }
            else
            {
                HideCellActions();
            }
        }

        private void ShowCellActions(HexCell cell)
        {
            var availableActions = cell.Actions.Where(a => a.CanDisplay(queryingPlayer.ID)).ToArray();
            for (int i = 0; i < availableActions.Length; i++)
            {
                if (i >= cellActions.Count)
                {
                    Debug.LogWarning("There are more actions than action item slots");
                    return;
                }
                
                var action = availableActions[i];
                cellActions[i].ShowAction(action, queryingPlayer.ID, cell.OnActionClicked);
            }
            HideCellActions(availableActions.Length);
        }

        private void ClearDetails()
        {
            currentCell = null;

            cellDetails.Clear();
            
            HideCellActions();
        }

        private void HideCellActions(int fromIndex = 0)
        {
            for (int i = fromIndex; i < cellActions.Count; i++)
            {
                cellActions[i].gameObject.SetActive(false);
            }
        }
    }
}