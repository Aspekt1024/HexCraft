using UnityEditor;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class TechTree : Page
    {
        private readonly UpgradeEditor editor;

        public override string Title => "Tech Tree";
        public TechTree(UpgradeEditor editor, VisualElement root)
        {
            this.editor = editor;
            AddToRoot(root, "TechTree");
            SetupUI();
        }

        public override void UpdateContents()
        {
            Reset();
        }

        private void SetupUI()
        {
            
            UpdateContents();
        }

        private void Reset()
        {
            
        }
    }
}