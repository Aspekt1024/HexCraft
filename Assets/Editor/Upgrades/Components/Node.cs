using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public abstract class Node : MouseManipulator
    {
        public int test = 1;
        [SerializeField] protected int hash;
        [SerializeField] protected Vector2 position;

        private bool isDragged;
        private Vector2 startMousePos;
        private Vector2 startPos;

        protected VisualElement element;
        
        public Action OnMove;
        public Action<Node> OnEnter;
        public Action<Node> OnLeave;

        protected Node()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        public int GetHash() => hash;
        
        public abstract object GetObject();

        public abstract bool HasValidObject();

        public abstract VisualElement GetElement();
        
        public Vector2 GetOutputPosition()
        {
            return new Vector2(position.x + 60f, position.y + 25f);
        }

        public Vector3 GetInputPosition()
        {
            return new Vector3(position.x + 60f, position.y + 25f);
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            target.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
            target.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        
        private void OnMouseDown(MouseDownEvent e)
        {
            if (e.button == 0)
            {
                isDragged = true;
                startMousePos = e.mousePosition;
                startPos = position;
                element.AddToClassList("node-dragged");
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (isDragged && e.button == 0)
            {
                isDragged = false;
                element.RemoveFromClassList("node-dragged");
                target.ReleaseMouse();
                e.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (isDragged)
            {
                UpdatePosition(e.mousePosition);
                OnMove?.Invoke();
                e.StopPropagation();
            }
        }

        private void OnMouseEnter(MouseEnterEvent e)
        {
            OnEnter?.Invoke(this);
        }

        private void OnMouseLeave(MouseLeaveEvent e)
        {
            OnLeave?.Invoke(this);
        }

        private void UpdatePosition(Vector2 mousePos)
        {
            position = startPos + mousePos - startMousePos;
            
            element.style.top = position.y;
            element.style.left = position.x;
        }
        
    }
}