using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CellInfo : UIElement
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI cellName;
        [SerializeField] private TextMeshProUGUI cellLevel;
#pragma warning restore 649

        private Camera mainCam;
        private HexCell cell;
        private List<CellUIItem.Details> itemDetails;
        private Transform tf;
        
        private List<CellUIItem> items;

        private Coroutine hideRoutine;

        public void Setup(HexCell cell, bool isOwnedByPlayer)
        {
            this.cell = cell;
            cellName.text = cell.DisplayName;

            if (isOwnedByPlayer)
            {
                SetupButtons(cell);
            }
        }

        public override void Show()
        {
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            
            gameObject.SetActive(true);
            base.Show();
            foreach (var item in items)
            {
                item.Show();
            }
        }

        public override void Hide()
        {
            if (IsHiding) return;
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideRoutine());
        }

        protected override void Awake()
        {
            base.Awake();
            
            tf = transform;
            mainCam = FindObjectOfType<HexCamera>().Camera;
            items = GetComponentsInChildren<CellUIItem>().ToList();
        }

        private void Update()
        {
            if (!IsVisible) return;

            var pos = mainCam.WorldToScreenPoint(cell.transform.position);
            pos.z = tf.position.z;
            tf.position = pos;
        }
        
        private void SetupButtons(HexCell cell)
        {
            if (cell.ItemDetails.Count > items.Count)
            {
                Debug.LogWarning("too many items for cell " + cell.DisplayName);
            }
            
            for (int i = 0; i < items.Count; i++)
            {
                if (i < cell.ItemDetails.Count)
                {
                    items[i].Setup(cell.ItemDetails[i]);
                }
                else
                {
                    items[i].Hide();
                }
            }
        }

        private IEnumerator HideRoutine()
        {
            foreach (var item in items)
            {
                item.Hide();
            }
            base.Hide();
            
            yield return new WaitForSeconds(FadeDuration);
            gameObject.SetActive(false);
        }
    }
}