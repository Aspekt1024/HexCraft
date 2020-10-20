using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CellInfo : UIElement
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI cellName;
#pragma warning restore 649

        private Camera mainCam;
        private HexCell cell;
        private List<CellUIItem.Details> itemDetails;
        private Transform tf;
        
        private List<CellUIItem> items;

        public void Setup(HexCell cell, List<CellUIItem> items)
        {
            this.cell = cell;
            this.items = items;
            cellName.text = cell.DisplayName;
        }

        public override void Show()
        {
            base.Show();
            SetupItems(items);
        }

        public override void Hide()
        {
            RemoveItems();
        }

        private void Start()
        {
            tf = transform;
            mainCam = FindObjectOfType<HexCamera>().Camera;
        }

        private void Update()
        {
            if (!IsVisible) return;

            var pos = mainCam.WorldToScreenPoint(cell.transform.position);
            pos.z = tf.position.z;
            tf.position = pos;
        }

        private void SetupItems(List<CellUIItem> items)
        {
            const float startPos = 0f;
            const float spread = 45f;
            for (int i = 0; i < items.Count; i++)
            {
                
            }
        }

        private void RemoveItems()
        {
            foreach (var item in items)
            {
                item.Hide();
            }
            base.Hide();
        }
    }
}