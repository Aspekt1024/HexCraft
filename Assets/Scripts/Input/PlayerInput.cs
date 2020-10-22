using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Hex
{
    internal interface IInputObserver
    {
        void BoardClickedPrimary(Vector3 position);
        void BoardClickedSecondary(Vector3 position);
    }
    
    internal class PlayerInput : UIBackplate.IMouseEventObserver
    {
        private readonly List<IInputObserver> observers = new List<IInputObserver>();

        public PlayerInput()
        {
            Object.FindObjectOfType<UIBackplate>().RegisterNotify(this);
        }
        
        public void RegisterNotify(IInputObserver observer)
        {
            observers.Add(observer);
        }
        
        public void HandleInput()
        {
            
        }

        public void OnMouseClick(PointerEventData.InputButton buttonId)
        {
            if (buttonId == PointerEventData.InputButton.Left)
            {
                HandleClickPrimary();
            }
            else if (buttonId == PointerEventData.InputButton.Right)
            {
                HandleClickSecondary();
            }
        }

        public Vector3 GetMousePositionOnBoard()
        {
            // TODO setup camera manager and set as dependency for PlayerInput
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, Vector3.zero);
            
            if (!plane.Raycast(ray, out float enter)) return Vector3.down * 10f;
            
            return ray.GetPoint(enter);
        }
        
        private void HandleClickPrimary()
        {
            var boardPosition = GetMousePositionOnBoard();
            if (boardPosition.y < -5f) return;
            
            foreach (var observer in observers)
            {
                observer.BoardClickedPrimary(boardPosition);
            }
        }

        private void HandleClickSecondary()
        {
            var boardPosition = GetMousePositionOnBoard();
            if (boardPosition.y < -5f) return;

            foreach (var observer in observers)
            {
                observer.BoardClickedSecondary(boardPosition);
            }
        }
    }
}