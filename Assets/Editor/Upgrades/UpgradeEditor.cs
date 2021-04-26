using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspekt.Hex.Upgrades
{
    public class UpgradeEditor : EditorWindow
    {
        public const string DirectoryRoot = "Assets/Editor/Upgrades";
        
        private VisualElement root;
        private Toolbar toolbar;
        private UpgradeEditorData data;
        
        private readonly List<Page> pages = new List<Page>();

        [MenuItem("Tools/Upgrade Editor _%#U")]
        private static void ShowWindow()
        {
            var window = GetWindow<UpgradeEditor>();
            window.titleContent = new GUIContent("Upgrade Editor");
            window.Show();
            
            window.minSize = new Vector2(450f, 300f);
        }

        /// <summary>
        /// Records an object for Undo operations and displays an indicator that signifies modifications
        /// to the Upgrade Data
        /// </summary>
        public void RecordUndo(Object data, string undoMessage)
        {
            Undo.RecordObject(data, undoMessage);
            toolbar.ShowModified();
        }
        
        private void OnEnable()
        {
            root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DirectoryRoot}/UpgradeEditorWindow.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DirectoryRoot}/UpgradeEditorWindow.uss");

            visualTree.CloneTree(root);
            root.styleSheets.Add(styleSheet);

            data ??= UpgradeEditorData.Load();
            toolbar = new Toolbar(root, data);
            
            AddPage(new UpgradeTester(this, root));
            AddPage(new TechTree(this, root, data));

            toolbar.Init();
            
            Undo.undoRedoPerformed += DataFilesUpdated;
            this.SetAntiAliasing(4);
        }

        private void OnGUI()
        {
            
        }

        private void DataFilesUpdated()
        {
            pages.ForEach(p => p.UpdateContents());
        }
        

        private void OnDisable()
        {
            UpgradeEditorData.Save(data);
            Undo.undoRedoPerformed -= DataFilesUpdated;
        }
        
        ~UpgradeEditor()
        {
            UpgradeEditorData.Save(data);
            Undo.undoRedoPerformed -= DataFilesUpdated;
        }

        private void AddPage(Page page)
        {
            pages.Add(page);
            toolbar.AddPage(page);
        }
    }
}