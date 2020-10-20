using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex
{
    internal interface IInputObserver
    {
        void BoardClickedPrimary(Vector3 position);
        void BoardClickedSecondary(Vector3 position);
    }
    
    internal class PlayerInput
    {
        private readonly List<IInputObserver> observers = new List<IInputObserver>();
        
        public void RegisterNotify(IInputObserver observer)
        {
            observers.Add(observer);
        }
        
        public void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleClickPrimary();
            }
            else if (Input.GetMouseButtonDown(1))
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