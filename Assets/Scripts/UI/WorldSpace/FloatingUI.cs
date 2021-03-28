using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class FloatingUI : MonoBehaviour, FloatingUIElement.IObserver
    {
        public enum Style
        {
            None = 0,
            Combat = 100,
        }
        
#pragma warning disable 649
        [SerializeField] private FloatingUIElement elementPrefab;
#pragma warning restore 649

        private readonly List<FloatingUIElement> freeElements = new List<FloatingUIElement>();
        private readonly List<FloatingUIElement> activeElements = new List<FloatingUIElement>();
        
        public void Show(Transform tf, Sprite icon, string text, Style style = Style.None)
        {
            var element = GetFreeElement();
            element.Begin(tf, icon, text, style);
        }

        private FloatingUIElement GetFreeElement()
        {
            if (freeElements.Any())
            {
                var element = freeElements[0];
                freeElements.RemoveAt(0);
                activeElements.Add(element);
                return element;
            }
            else
            {
                var element = Instantiate(elementPrefab, transform);
                activeElements.Add(element);
                element.RegisterObserver(this);
                return element;
            }
        }

        public void OnComplete(FloatingUIElement element)
        {
            activeElements.Remove(element);
            freeElements.Add(element);
        }
    }
}