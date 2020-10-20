using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CellUI : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private CellInfo cellInfoPrefab;
        [SerializeField] private CellUIItem itemPrefab;
#pragma warning restore 649

        private readonly List<CellInfo> cellInfos = new List<CellInfo>();
        private readonly List<CellUIItem> cellItems = new List<CellUIItem>();

        private HexCell currentCell;

        private NetworkGamePlayerHex player;
        
        public void Init(NetworkGamePlayerHex player)
        {
            this.player = player;
        }

        public void Show(HexCell cell)
        {
            if (cell == null || currentCell == cell) return;
            
            HideAll();
            currentCell = cell;

            var info = GetCellInfoPlate();
            var items = new List<CellUIItem>();
            
            if (player.ID == cell.PlayerId)
            {
                foreach (var details in cell.ItemDetails)
                {
                    var item = GetCellUIItem();
                    item.Setup(details);
                    items.Add(item);
                }
            }
            
            info.Setup(cell, items);
            info.Show();
        }

        public void HideAll()
        {
            foreach (var info in cellInfos)
            {
                info.Hide();
            }
        }

        private CellInfo GetCellInfoPlate()
        {
            foreach (var cellInfo in cellInfos)
            {
                if (!cellInfo.IsVisible) return cellInfo;
            }

            var info = Instantiate(cellInfoPrefab, transform);
            cellInfos.Add(info);
            
            return info;
        }

        private CellUIItem GetCellUIItem()
        {
            foreach (var item in cellItems)
            {
                if (!item.IsVisible) return item;
            }

            var newItem = Instantiate(itemPrefab);
            cellItems.Add(newItem);
            return newItem;
        }
    }
}