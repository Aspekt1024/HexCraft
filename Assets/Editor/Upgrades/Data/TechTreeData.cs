using System;
using System.Collections.Generic;
using Aspekt.Hex.Actions;
using UnityEngine;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class TechTreeData
    {
        [SerializeField] private List<CellNode> cellNodes;
        [SerializeField] private List<UpgradeGroupNode> upgradeGroupNodes;

        private List<Node> allNodes = new List<Node>();

        public List<Node> GetAllNodes() => allNodes;

        public void Init()
        {
            allNodes.AddRange(cellNodes);
            allNodes.AddRange(upgradeGroupNodes);
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
                allNodes.Add(node);
            }
            else
            {
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
                allNodes.Add(node);
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
            allNodes.Clear();
        }

        public void Clean()
        {
            for (int i = allNodes.Count - 1; i >= 0; i--)
            {
                if (!allNodes[i].HasValidObject() || allNodes[i].GetObject() is UnitAction)
                {
                    if (allNodes[i] is CellNode cellNode) cellNodes.Remove(cellNode);
                    if (allNodes[i] is UpgradeGroupNode upgradeNode) upgradeGroupNodes.Remove(upgradeNode);
                    allNodes.RemoveAt(i);
                }
            }
        }

        private T GetNodeFromHash<T>(int hash) where T : Node
        {
            var index = allNodes.FindIndex(n => n.GetHash() == hash);
            return (T)(index >= 0 ? allNodes[index] : null);
        }
    }
}