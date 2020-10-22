using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CellUI : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private CellInfo cellInfoPrefab;
#pragma warning restore 649

        private readonly List<CellInfo> cellInfos = new List<CellInfo>();

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
            
            info.Setup(cell, player.ID == cell.PlayerId);
            info.Show();
        }

        public void HideAll()
        {
            currentCell = null;
            foreach (var info in cellInfos)
            {
                info.Hide();
            }
        }

        private CellInfo GetCellInfoPlate()
        {
            foreach (var cellInfo in cellInfos)
            {
                if (!cellInfo.gameObject.activeSelf)
                {
                    cellInfo.gameObject.SetActive(true);
                    return cellInfo;
                }
            }

            var info = Instantiate(cellInfoPrefab, transform);
            cellInfos.Add(info);
            
            return info;
        }
    }
}