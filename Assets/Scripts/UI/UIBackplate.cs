using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Hex.UI
{
    public class UIBackplate : MonoBehaviour, IPointerClickHandler
    {
        public interface IMouseEventObserver
        {
            void OnMouseClick(PointerEventData.InputButton buttonId);
        }
        
        private readonly List<IMouseEventObserver> observers = new List<IMouseEventObserver>();
        
        private void OnMouseDown()
        {
            Debug.Log("clicked");
        }

        public void RegisterNotify(IMouseEventObserver observer)
        {
            observers.Add(observer);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            foreach (var observer in observers)
            {
                observer.OnMouseClick(eventData.button);
            }
        }
    }
}