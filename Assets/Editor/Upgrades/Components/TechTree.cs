using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Util;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspekt.Hex.Upgrades
{
    public class TechTree : Page
    {
        private readonly UpgradeEditor editor;
        private readonly UpgradeEditorData data;

        public override string Title => "Tech Tree";

        private VisualElement nodeRoot;
        private GameConfig config;

        public TechTree(UpgradeEditor editor, VisualElement root, UpgradeEditorData data)
        {
            this.editor = editor;
            this.data = data;
            AddToRoot(root, "TechTree");
            SetupUI();
        }

        public override void UpdateContents()
        {
            Reset();
        }
        
        private void SetupUI()
        {
            nodeRoot = Root.Q("Nodes");

            SetupData();
            UpdateTree();
        }

        private void Reset()
        {
            SetupData();
            UpdateTree();
        }

        private void UpdateTree()
        {
            UpdateTree2();
            return;
            
            foreach (var upgrade in config.techConfig.upgrades)
            {
                var node = data.techTreeData.GetNode(upgrade);
                foreach (var upgradeDetail in upgrade.upgradeDetails)
                {
                    var dependencies = GetDependencies(upgradeDetail.requiredTech);
                    ShowDependencyLinks(node, dependencies);
                }
            }

            foreach (var buildAction in config.techConfig.buildActions)
            {
                var node = data.techTreeData.GetNode(buildAction);
                var dependencies = GetDependencies(buildAction.techRequirements);
                ShowDependencyLinks(node, dependencies);
            }
            
            data.techTreeData.GetAllNodes()
                .Where(n => n.HasValidObject()).ToList()
                .ForEach(n => nodeRoot.Add(n.GetElement()));
        }

        private List<Node> GetDependencies(List<Technology> techRequirements)
        {
            var dependencies = new List<Node>();
            
            foreach (var buildAction in config.techConfig.buildActions)
            {
                if (techRequirements.Contains(buildAction.prefab.Technology))
                {
                    dependencies.Add(data.techTreeData.GetNode(buildAction));
                }
            }

            foreach (var upgradeAction in config.techConfig.upgrades)
            {
                foreach (var upgrade in upgradeAction.upgradeDetails)
                {
                    if (techRequirements.Contains(upgrade.tech))
                    {
                        dependencies.Add(data.techTreeData.GetNode(upgradeAction));
                    }
                }
            }

            return dependencies;
        }

        private void SetupData()
        {
            var game = Object.FindObjectOfType<GameManager>();
            config = InspectorUtil.GetPrivateValue<GameConfig, GameManager>("config", game);
            
            var cells = Object.FindObjectOfType<Cells>();

            var cellTypes = (Cells.CellTypes[])Enum.GetValues(typeof(Cells.CellTypes));
            foreach (var cellType in cellTypes)
            {
                if (cellType == Cells.CellTypes.None) continue;
                
                var cell = cells.GetPrefab(cellType);
                if (cell != null)
                {
                    data.techTreeData.GetNode(cell);
                }
            }
            
            foreach (var buildAction in config.techConfig.buildActions)
            {
                data.techTreeData.GetNode(buildAction);
            }
            
            foreach (var upgradeAction in config.techConfig.upgrades)
            {
                data.techTreeData.GetNode(upgradeAction);
            }
        }

        private void UpdateTree2()
        {
            var allNodes = data.techTreeData.GetAllNodes();
            foreach (var node in allNodes)
            {
                if (!node.HasValidObject()) continue;

                var obj = node.GetObject();
                if (obj is HexCell cell)
                {
                    ProcessCell(cell, node);
                }
                else if (obj is ActionDefinition action)
                {
                    ProcessAction(action, node);
                }
            }
            
            foreach (var node in data.techTreeData.GetAllNodes())
            {
                if (!node.HasValidObject()) continue;
                nodeRoot.Add(node.GetElement());
            }
        }

        private void ProcessCell(HexCell cell, Node node)
        {
            foreach (var action in cell.Actions)
            {
                if (action is UnitAction) continue;
                
                var actionNode = data.techTreeData.GetNode(action);
                var line = new ConnectionElement(node, actionNode, new Color(0.35f, 0.59f, 1f), 2f, true);
                nodeRoot.Add(line);
            }

            var buildAction = GetBuildActionForCell(cell);
            if (buildAction != null)
            {
                ProcessAction(buildAction, node);
            }
        }

        private BuildAction GetBuildActionForCell(HexCell cell)
        {
            return config.techConfig.buildActions.FirstOrDefault(a => a.prefab == cell);
        }

        private void ProcessAction(ActionDefinition action, Node node)
        {
            if (action is BuildAction buildAction)
            {
                var deps = GetDependencies(buildAction.techRequirements);
                ShowDependencyLinks(node, deps);
            }
            else if (action is UpgradeAction upgradeAction)
            {
                foreach (var upgrade in upgradeAction.upgradeDetails)
                {
                    var deps = GetDependencies(upgrade.requiredTech);
                    ShowDependencyLinks(node, deps);
                }
            }
        }
        
        private void ShowDependencyLinks(Node node, List<Node> dependencies)
        {
            dependencies.ForEach(d =>
            {
                var line = new ConnectionElement(d, node, new Color(1f, 0.86f, 0.61f), 1.5f, true);
                nodeRoot.Add(line);
            });
        }

    }
}