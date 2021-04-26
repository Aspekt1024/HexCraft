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

        private HexCell cell;
        private ActionDefinition action;
        
        private bool isDragged;
        private Vector2 startMousePos;
        private Vector2 startPos;

        private VisualElement element;

        public Action OnMove;
        
        public Node(ActionDefinition action)
        {
            Setup(action);
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        public Node(HexCell cell)
        {
            Setup(cell);
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        public int GetHash() => hash;

        public static int GenerateHash(ActionDefinition action)
        {
            if (action is BuildAction buildAction && buildAction.prefab != null)
            {
                return GenerateHash(buildAction.prefab);
            }
            return Hash128.Compute("Action" + action.name).GetHashCode();
        }
        
        public static int GenerateHash(HexCell cell) => Hash128.Compute("Cell" + cell.name).GetHashCode();
        
        public void Setup(ActionDefinition action)
        {
            this.action = action;
            if (action is BuildAction buildAction)
            {
                cell = buildAction.prefab;
                hash = GenerateHash(cell);
            }
            else
            {
                hash = GenerateHash(action);
            }
            
            element = GetElement();
        }

        public void Setup(HexCell cell)
        {
            this.cell = cell;
            hash = GenerateHash(cell);
            element = GetElement();
        }

        public bool HasValidObject()
        {
            return action != null || cell != null;
        }

        public object GetObject()
        {
            if (cell != null) return cell;
            return action;
        }
        
        public VisualElement GetElement()
        {
            if (element != null) return element;
            
            element = new VisualElement();
            element.AddToClassList("node");

            if (cell != null)
            {
                element.Add(new Label(cell.DisplayName));
                element.AddToClassList(cell is UnitCell ? "node-unit" : "node-building");
            }
            else if (action != null)
            {
                element.Add(new Label(action.name));
                if (action is UpgradeAction)
                {
                    element.AddToClassList("node-upgrade");
                }
            }
            
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);
            
            return element;
        }

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
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }
        
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
                OnMove?.Invoke();
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