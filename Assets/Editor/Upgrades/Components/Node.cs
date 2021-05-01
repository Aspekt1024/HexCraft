using System;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public abstract class Node : MouseManipulator
    {
        [SerializeField] protected int hash;
        [SerializeField] protected Vector2 position;

        protected bool IsDragged;
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

        public Vector2 GetPosition() => position;

        protected bool MoveDisabled;

        public int GetHash() => hash;
        
        public abstract object GetObject();
        public abstract ActionDefinition GetAction(TechConfig techConfig);

        public abstract bool HasValidObject();

        public abstract VisualElement GetElement();

        protected virtual string ActivatingLinkClass => "node-newlink";

        public virtual void ActivatingLinkStart()
        {
            GetElement().AddToClassList(ActivatingLinkClass);
        }

        public virtual void ActivatingLinkEnd()
        {
            GetElement().RemoveFromClassList(ActivatingLinkClass);
        }
        
        public virtual Vector2 GetOutputPosition()
        {
            return new Vector2(position.x + 60f, position.y + 25f);
        }

        public virtual Vector2 GetInputPosition()
        {
            return new Vector2(position.x + 60f, position.y + 25f);
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
            if ( e.button == 0)
            {
                IsDragged = true;
                startMousePos = e.mousePosition;
                startPos = position;
                element.AddToClassList("node-dragged");
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (IsDragged && e.button == 0)
            {
                IsDragged = false;
                element.RemoveFromClassList("node-dragged");
                target.ReleaseMouse();
                e.StopPropagation();
            }
        }

        protected virtual void OnMouseMove(MouseMoveEvent e)
        {
            if (IsDragged)
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
            if (MoveDisabled) return;

            position = startPos + mousePos - startMousePos;
            
            element.style.top = position.y;
            element.style.left = position.x;
        }
        
    }
}