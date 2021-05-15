using System;
using System.Collections.Generic;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public struct TreeElement
    {
        public VisualElement VisualElement;
        public int SortOrder;
        public VisualElement Parent;
        public List<TreeElement> Children;
    }
    
    [Serializable]
    public abstract class Node : MouseManipulator
    {
        [SerializeField] protected int hash;
        [SerializeField] protected Vector2 position;

        public abstract int SortOrder { get; }
        
        protected bool IsDragged;
        private Vector2 startMousePos;
        private Vector2 startPos;

        protected TreeElement Element;
        
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

        public void ClearElement()
        {
            Element.VisualElement = null;
        }
        
        public abstract object GetObject();
        public abstract ActionDefinition GetAction(TechConfig techConfig);

        public abstract bool HasValidObject();

        public abstract TreeElement GetElement();

        protected virtual string ActivatingLinkClass => "node-newlink";

        public virtual void ActivatingLinkStart()
        {
            GetElement().VisualElement.AddToClassList(ActivatingLinkClass);
        }

        public virtual void ActivatingLinkEnd()
        {
            GetElement().VisualElement.RemoveFromClassList(ActivatingLinkClass);
        }

        public virtual Vector2 GetConnectingPosition(Vector2 fromPos)
        {
            var e = Element.VisualElement;
            var pos = position;

            var dist = pos - fromPos;
            if (Mathf.Abs(dist.y) > Mathf.Abs(dist.x))
            {
                pos.x += e.layout.width / 2f;

                if (fromPos.y > pos.y)
                {
                    pos.y += e.layout.height;
                }
            }
            else
            {
                pos.y += e.layout.height / 2f;
            
                if (fromPos.x > pos.x)
                {
                    pos.x += e.layout.width;
                }
            }

            return pos;
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
                Element.VisualElement.AddToClassList("node-dragged");
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (IsDragged && e.button == 0)
            {
                IsDragged = false;
                Element.VisualElement.RemoveFromClassList("node-dragged");
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

            const float snapLength = 10f;
            
            position = startPos + mousePos - startMousePos;
            position.x = Mathf.Round(position.x / snapLength) * snapLength;
            position.y = Mathf.Round(position.y / snapLength) * snapLength;
            
            Element.VisualElement.style.top = position.y;
            Element.VisualElement.style.left = position.x;
        }
    }
}