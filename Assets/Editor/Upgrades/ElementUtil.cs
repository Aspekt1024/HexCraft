using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public static class ElementUtil
    {
        private const string HiddenClassName = "hidden";
        
        public static void HideElements(params VisualElement[] elements)
        {
            foreach (var element in elements)
            {
                HideElement(element);
            }
        }
        
        public static void HideElement(VisualElement element)
        {
            element.RemoveFromClassList(HiddenClassName);
            element.AddToClassList(HiddenClassName);
        }

        public static void ShowElement(VisualElement element, string className = "")
        {
            element.RemoveFromClassList(HiddenClassName); // clears the 'hidden' class tag
            
            if (string.IsNullOrEmpty(className)) return;
            element.AddToClassList(className);
        }

        public static void ToggleShow(VisualElement element)
        {
            if (element.ClassListContains(HiddenClassName))
            {
                element.RemoveFromClassList(HiddenClassName);
            }
            else
            {
                element.AddToClassList(HiddenClassName);
            }
        }
    }
}