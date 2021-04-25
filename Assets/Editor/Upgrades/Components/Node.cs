using System;
using Aspekt.Hex.Actions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class Node : MouseManipulator
    {
        [SerializeField] private int hash;
        [SerializeField] private Vector2 position;
        
        private ActionDefinition action;
        
        private bool isDragged;

        private VisualElement element;
        
        public Node(ActionDefinition action)
        {
            Setup(action);
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        public int GetHash() => hash;
        
        public void Setup(ActionDefinition action)
        {
            this.action = action;
            
            hash = action.GetHashCode();

            element = GetElement();
            
        }
        
        public VisualElement GetElement()
        {
            if (element != null) return element;
            
            element = new VisualElement();
            element.AddToClassList("node");

            var tech = Technology.None;
            if (action is BuildAction buildAction)
            {
                tech = buildAction.prefab.Technology;
            }
            else if (action is UpgradeAction upgradeAction)
            {
                tech = upgradeAction.GetNextTech();
            }
            
            element.Add(new Label(action.name + ": " + tech.ToString()));
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);
            
            return element;
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private Vector2 startMousePos;
        private Vector2 startPos;
        
        private void OnMouseDown(MouseDownEvent e)
        {
            isDragged = true;
            startMousePos = e.mousePosition;
            startPos = position;
            element.AddToClassList("node-dragged");
            target.CaptureMouse();
            e.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            isDragged = false;
            element.RemoveFromClassList("node-dragged");
            target.ReleaseMouse();
            e.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (isDragged)
            {
                UpdatePosition(e.mousePosition);
                e.StopPropagation();
            }
        }

        private void UpdatePosition(Vector2 mousePos)
        {
            position = startPos + mousePos - startMousePos;
            
            element.style.top = position.y;
            element.style.left = position.x;
        }
        
    }
}