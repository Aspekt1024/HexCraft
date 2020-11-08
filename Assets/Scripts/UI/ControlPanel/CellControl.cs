using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI.Control
{
    public class CellControl : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI cellName;
        [SerializeField] private TextMeshProUGUI health;
        [SerializeField] private TextMeshProUGUI tier;
        [SerializeField] private TextMeshProUGUI owner;
        [SerializeField] private List<CellUIItem> cellActions;
#pragma warning restore 649

        private void Awake()
        {
            ClearDetails();
        }

        public void Init(Tooltip tooltip)
        {
            foreach (var actionItem in cellActions)
            {
                actionItem.RegisterObserver(tooltip);
            }
        }

        public void SetCellDetails(HexCell cell, NetworkGamePlayerHex queryingPlayer)
        {
            if (cell == null)
            {
                ClearDetails();
            }
            else
            {
                cellName.text = cell.DisplayName;
                health.text = "HP: " + cell.CurrentHP + " / " + cell.MaxHP;
                tier.text = "Tier 1";
                owner.text = "Owned by " + cell.Owner.DisplayName;

                if (cell.PlayerId == queryingPlayer.ID)
                {
                    ShowCellActions(cell);
                }
                else
                {
                    HideCellActions();
                }
            }
        }

        private void ShowCellActions(HexCell cell)
        {
            var availableActions = cell.Actions.Where(a => a.CanDisplay()).ToArray();
            for (int i = 0; i < availableActions.Length; i++)
            {
                if (i >= cellActions.Count)
                {
                    Debug.LogWarning("There are more actions than action item slots");
                    return;
                }
                
                var action = availableActions[i];
                cellActions[i].ShowAction(action);
            }
            HideCellActions(availableActions.Length);
        }

        private void ClearDetails()
        {
            cellName.text = "";
            health.text = "";
            tier.text = "";
            owner.text = "";
            
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