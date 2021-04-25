using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class Toolbar : UnityEditor.AssetModificationProcessor
    {
        private readonly VisualElement items;
        private readonly VisualElement modifyIndicator;
        
        private static Toolbar current;
        private readonly List<Button> buttons = new List<Button>();
        private readonly List<Page> pages = new List<Page>();
        private readonly UpgradeEditorData data;
        
        public Toolbar(VisualElement editorRoot, UpgradeEditorData data)
        {
            current = this;
            this.data = data;
            
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UpgradeEditor.DirectoryRoot}/Templates/Toolbar.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UpgradeEditor.DirectoryRoot}/Templates/Toolbar.uss");

            visualTree.CloneTree(editorRoot);
            var toolbarRoot = editorRoot.Q("Toolbar");
            toolbarRoot.styleSheets.Add(styleSheet);
            
            items = toolbarRoot.Q("ItemContainer");
            modifyIndicator = toolbarRoot.Q("ModifyIndicator");
        }

        public void Init()
        {
            if (pages.Any())
            {
                if (!string.IsNullOrEmpty(data.currentPage))
                {
                    var pageIndex = pages.FindIndex(p => p.Title == data.currentPage);
                    if (pageIndex < 0)
                    {
                        SelectDefaultPage();
                    }
                    else
                    {
                        HighlightButton(buttons[pageIndex]);
                        OnPageSelected(pages[pageIndex]);
                    }
                }
                else
                {
                    SelectDefaultPage();
                }
            }
        }

        private void SelectDefaultPage()
        {
            HighlightButton(buttons[0]);
            OnPageSelected(pages[0]);
            data.currentPage = pages[0].Title;
        }

        public void AddPage(Page page)
        {
            pages.Add(page);
            CreateToolbarButton(page);
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (current == null) return null;
            current.modifyIndicator.RemoveFromClassList("modify-indicator");
            return paths;
        }

        public void ShowModified()
        {
            modifyIndicator.AddToClassList("modify-indicator");
        }
        
        private Button CreateToolbarButton(Page page)
        {
            var btn = new Button { text = page.Title };
            btn.clicked += () =>
            {
                HighlightButton(btn);
                OnPageSelected(page);
                
            };
            items.Add(btn);
            buttons.Add(btn);
            
            return btn;
        }

        private void OnPageSelected(Page selectedPage)
        {
            data.currentPage = selectedPage.Title;
            foreach (var page in pages)
            {
                if (page.Root == selectedPage.Root)
                {
                    ElementUtil.ShowElement(page.Root);
                }
                else
                {
                    ElementUtil.HideElement(page.Root);
                }
            }
        }

        private void HighlightButton(Button button)
        {
            const string activeClassName = "active-button";
            
            foreach (var btn in buttons)
            {
                if (btn == button)
                {
                    button.AddToClassList(activeClassName);
                }
                else
                {
                    btn.RemoveFromClassList(activeClassName);
                }
            }
        }
    }
}