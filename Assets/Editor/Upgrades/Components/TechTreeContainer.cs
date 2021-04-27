using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public enum UpgradeDependencyMode
    {
        CreateBuild,
        CreateTechRequirement,
        RemoveBuild,
        RemoveTechRequirement,
    }
    
    public class TechTreeContainer : MouseManipulator
    {
        private bool isDragged;
        private VisualElement element;
        
        public interface IObserver
        {
            void OnStartDependency();
            void OnEndDependency(UpgradeDependencyMode mode);
            void OnDependencyDrag(Vector2 mousePos);
        }

        private IObserver observer;

        public void RegisterSingleObserver(IObserver observer) => this.observer = observer;
        
        public TechTreeContainer()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
        }

        public VisualElement GetElement()
        {
            if (element != null) return element;

            element = new VisualElement();
            element.AddToClassList("tech-tree");
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

        private void OnMouseDown(MouseDownEvent e)
        {
            if (e.button == 1)
            {
                isDragged = true;
                e.StopPropagation();
                observer?.OnStartDependency();
                target.CaptureMouse();
            }
        }
        
        private void OnMouseUp(MouseUpEvent e)
        {
            if (isDragged && e.button == 1)
            {
                isDragged = false;
                e.StopPropagation();
                observer?.OnEndDependency(GetDependencyMode(e));
                target.ReleaseMouse();
            }
        }
        
        private void OnMouseMove(MouseMoveEvent e)
        {
            if (isDragged)
            {
                observer?.OnDependencyDrag(e.mousePosition);
            }
        }

        private UpgradeDependencyMode GetDependencyMode(MouseUpEvent e)
        {
            if (e.ctrlKey)
            {
                return e.shiftKey ? UpgradeDependencyMode.RemoveTechRequirement : UpgradeDependencyMode.RemoveBuild;
            }
            else
            {
                return e.shiftKey ? UpgradeDependencyMode.CreateTechRequirement : UpgradeDependencyMode.CreateBuild;
            }
        }
        
    }
}