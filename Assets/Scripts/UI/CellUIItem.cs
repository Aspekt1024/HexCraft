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

        public struct Details
        {
            public Sprite Sprite;
            public Action Callback;
        }
        
        public void Setup(Details details)
        {
            spriteRenderer.sprite = details.Sprite;
            onClickCallback = details.Callback;
        }

        public void CellItemClicked()
        {
            onClickCallback?.Invoke();
        }
    }
}