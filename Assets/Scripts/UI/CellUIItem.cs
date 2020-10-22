using System;
using UnityEngine;
using UnityEngine.UI;

namespace Aspekt.Hex.UI
{
    public class CellUIItem : UIElement
    {
#pragma warning disable 649
        [SerializeField] private Image spriteRenderer;
#pragma warning restore 649

        private Action onClickCallback;

        private bool hasContent = false;
        
        public struct Details
        {
            public Sprite Sprite;
            public Action Callback;
        }
        
        public void Setup(Details details)
        {
            spriteRenderer.sprite = details.Sprite;
            onClickCallback = details.Callback;
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