using UnityEditor;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public abstract class Page
    {
        public abstract string Title { get; }
        public VisualElement Root { get; private set; }

        public abstract void UpdateContents();
        
        protected void AddToRoot(VisualElement editorRoot, string templateName)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UpgradeEditor.DirectoryRoot}/Templates/{templateName}.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UpgradeEditor.DirectoryRoot}/Templates/{templateName}.uss");
            
            visualTree.CloneTree(editorRoot);
 
            Root = editorRoot.Q(templateName);
            Root.styleSheets.Add(styleSheet);
        }
    }
}