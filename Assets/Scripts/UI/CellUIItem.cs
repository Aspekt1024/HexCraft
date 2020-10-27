using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aspekt.Hex.UI
{
    public class CellUIItem : UIElement
    {
#pragma warning disable 649
        [SerializeField] private Image spriteRenderer;
        [SerializeField] private TextMeshProUGUI costText;
#pragma warning restore 649

        private Action onClickCallback;

        private bool hasContent = false;
        
        public struct Details
        {
            public readonly Sprite Sprite;
            public readonly Action Callback;
            public readonly int Cost;

            public Details(Sprite sprite, Action callback, int cost)
            {
                Sprite = sprite;
                Callback = callback;
                Cost = cost;
            }
        }
        
        public void Setup(Details details)
        {
            spriteRenderer.sprite = details.Sprite;
            onClickCallback = details.Callback;
            costText.text = details.Cost == 0 ? "" : details.Cost.ToString();
            
            hasContent = true;
        }

        public void CellItemClicked()
        {
            onClickCallback?.Invoke();
        }

        public override void Show()
        {
            if (!hasContent) return;
            base.Show();
        }

        public override void Hide()
        {
            onClickCallback = null;
            hasContent = false;
            base.Hide();
        }
    }
}