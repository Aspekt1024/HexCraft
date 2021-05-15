using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class TechTreeData
    {
        [SerializeField] private List<CellNode> cellNodes;
        [SerializeField] private List<UpgradeGroupNode> upgradeGroupNodes;

        public List<CellNode> GetCellNodes() => cellNodes.ToList();
        
        public List<Node> GetAllNodes()
        {
            var nodes = cellNodes.Select(n => n as Node).ToList();
            nodes.AddRange(upgradeGroupNodes);
            return nodes;
        }

        public Node GetNode(ActionDefinition action)
        {
            if (action is BuildAction buildAction)
            {
                return GetNode(buildAction.prefab);
            } 
            
            if (!(action is UpgradeAction upgradeAction))
            {
                Debug.LogError($"No node type defined for action {action.GetType()}");
                return null;
            }
            
            var hash = UpgradeGroupNode.GenerateHash(upgradeAction);
            var node = GetNodeFromHash<UpgradeGroupNode>(hash);
            
            if (node == null)
            {
                node = new UpgradeGroupNode(upgradeAction);
                upgradeGroupNodes.Add(node);
            }
            else
            {
                // TODO this calls a second Setup on the node
                // TODO which replaces the upgrade group sub nodes
                node.Setup(upgradeAction);
            }
            
            return node;
        }

        public Node GetNode(HexCell cell)
        {
            var hash = CellNode.GenerateHash(cell);
            var node = GetNodeFromHash<CellNode>(hash);

            if (node == null)
            {
                node = new CellNode(cell);
                cellNodes.Add(node);
            }
            else
            {
                node.Setup(cell);
            }

            return node;
        }

        public void Purge()
        {
            cellNodes.Clear();
            upgradeGroupNodes.Clear();
        }

        public void Clean()
        {
            for (int i = cellNodes.Count - 1; i >= 0; i--)
            {
                if (!cellNodes[i].HasValidObject() || cellNodes[i].GetObject() is UnitAction)
                {
                    cellNodes.RemoveAt(i);
                }
            }

            for (int i = upgradeGroupNodes.Count - 1; i >= 0; i--)
            {
                if (!upgradeGroupNodes[i].HasValidObject())
                {
                    upgradeGroupNodes.RemoveAt(i);
                }
            }
        }

        public void ClearVisualData()
        {
            cellNodes.ForEach(n => n.ClearElement());
            upgradeGroupNodes.ForEach(n => n.ClearElement());
        }

        private T GetNodeFromHash<T>(int hash) where T : Node
        {
            var allNodes = GetAllNodes();
            var index = allNodes.FindIndex(n => n.GetHash() == hash);
            return (T)(index >= 0 ? allNodes[index] : null);
        }
    }
}