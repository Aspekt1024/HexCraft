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
    public class TechTree : Page, TechTreeContainer.IObserver
    {
        private readonly UpgradeEditor editor;
        private readonly UpgradeEditorData data;
        private TechTreeContainer container;
        private ObjectViewer objectViewer;

        public override string Title => "Tech Tree";

        private VisualElement nodeRoot;
        private GameConfig config;
        
        public Action<Vector2> OnDrag;
        private Node lastNode;
        private Node startNode;
        private ConnectionElement depLine;

        private readonly List<ConnectionElement> dependencyLinks = new List<ConnectionElement>();

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
            var scrollView = Root.Q("Nodes");
            container = new TechTreeContainer();
            nodeRoot = container.GetElement();
            container.RegisterSingleObserver(this);
            scrollView.Add(nodeRoot);
            
            objectViewer = new ObjectViewer(scrollView);

            SetupData();
            UpdateTree();
        }

        private void Reset()
        {
            nodeRoot.Clear();
            dependencyLinks.Clear();
            data.techTreeData.ClearVisualData();
            objectViewer.UpdateContents();
            SetupData();
            UpdateTree();
        }

        private void UpdateTree()
        {
            ProcessAllNodeLogic();
            ProcessAllNodeVisuals();
        }

        private void ProcessAllNodeLogic()
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
        }

        private void ProcessAllNodeVisuals()
        {
            var allElements = new List<TreeElement>();

            var allNodes = data.techTreeData.GetAllNodes();
            foreach (var node in allNodes)
            {
                if (!node.HasValidObject()) continue;
                
                node.OnEnter = NodeEntered;
                node.OnLeave = NodeLeft;
                node.SetupOnClickCallbacks(NodeClicked);

                AddElementsToList(node.GetElement(), allElements);
            }

            foreach (var link in dependencyLinks)
            {
                allElements.Add(new TreeElement()
                {
                    VisualElement = link,
                    SortOrder = link.SortOrder
                });
            }
            
            foreach (var element in allElements.OrderBy(e => e.SortOrder))
            {
                if (element.Parent == null)
                {
                    nodeRoot.Add(element.VisualElement);
                }
                else
                {
                    element.Parent.Add(element.VisualElement);
                }
            }
        }

        private static void AddElementsToList(TreeElement element, List<TreeElement> allElements)
        {
            allElements.Add(element);
            
            if (element.Children == null) return;
            
            foreach (var child in element.Children)
            {
                AddElementsToList(child, allElements);
            }
        }

        private List<Node> GetDependencies(List<Technology> techRequirements)
        {
            var dependencies = new List<Node>();
            
            foreach (var cellNode in data.techTreeData.GetCellNodes())
            {
                var cell = cellNode.GetCell();
                if (cell == null) continue;
                if (techRequirements.Contains(cell.Technology))
                {
                    dependencies.Add(data.techTreeData.GetNode(cell));
                }
            }

            foreach (var upgradeAction in config.techConfig.upgrades)
            {
                foreach (var upgrade in upgradeAction.upgradeDetails)
                {
                    if (techRequirements.Contains(upgrade.tech))
                    {
                        var upgradeGroupNode = data.techTreeData.GetNode(upgradeAction) as UpgradeGroupNode;
                        if (upgradeGroupNode == null)
                        {
                            Debug.LogError($"upgrade action {upgradeAction} did not produce {nameof(UpgradeGroupNode)}");
                            continue;
                        }
                        var upgradeNode = upgradeGroupNode.GetSubNode(upgrade);
                        dependencies.Add(upgradeNode);
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
                CreateDependencyLinks(node, deps);
            }
            else if (action is UpgradeAction upgradeAction)
            {
                foreach (var upgrade in upgradeAction.upgradeDetails)
                {
                    var subNode = ((UpgradeGroupNode) node).GetSubNode(upgrade);
                    var deps = GetDependencies(upgrade.requiredTech);
                    CreateDependencyLinks(subNode, deps);
                }
            }
        }
        
        private void CreateDependencyLinks(Node node, List<Node> dependencies)
        {
            dependencies.ForEach(d =>
            {
                var line = new ConnectionElement(d, node, new Color(1f, 0.86f, 0.61f), 1.5f, true);
                dependencyLinks.Add(line);
            });
        }

        public void OnStartDependency(Vector2 mousePos)
        {
            if (lastNode != null)
            {
                startNode = lastNode;
                startNode.ActivatingLinkStart();
                depLine = new ConnectionElement(this, mousePos, Color.yellow, 2f, true);
                nodeRoot.Add(depLine);
            }
        }

        public void OnEndDependency(UpgradeDependencyMode mode)
        {
            if (startNode != null)
            {
                if (lastNode != null)
                {
                    var success = NodeDependencies.CreateDependency(startNode, lastNode, config.techConfig, mode);
                    if (success)
                    {
                        UpdateContents();
                    }
                    lastNode.ActivatingLinkEnd();
                }
                startNode.ActivatingLinkEnd();
                startNode = null;
            }
            
            if (depLine != null)
            {
                if (nodeRoot.Contains(depLine)) nodeRoot.Remove(depLine);
                depLine = null;
            }
        }

        public void OnDependencyDrag(Vector2 mousePos) => OnDrag?.Invoke(mousePos);
        private void NodeEntered(Node node)
        {
            if (startNode != null)
            {
                node.ActivatingLinkStart();
            }
            lastNode = node;
        }

        private void NodeLeft(Node node)
        {
            if (startNode != null && startNode != node)
            {
                node.ActivatingLinkEnd();
            }
            if (lastNode == node)
            {
                lastNode = null;
            }
        }

        private void NodeClicked(Node node)
        {
            if (node is CellNode cellNode)
            {
                objectViewer.ShowNodeDetails(cellNode.GetCell());
            }
            else if (node is UpgradeGroupNode upgradeGroupNode)
            {
                objectViewer.ShowNodeDetails(upgradeGroupNode.GetAction(config.techConfig));
            }
            else if (node is UpgradeSubNode upgradeSubNode)
            {
                objectViewer.ShowNodeDetails(upgradeSubNode.GetUpgradeDetails());
            }
        }
    }
}