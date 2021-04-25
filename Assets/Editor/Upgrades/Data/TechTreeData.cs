using System;
using System.Collections.Generic;
using Aspekt.Hex.Actions;
using UnityEngine;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class TechTreeData
    {
        public int test = 1;
        [SerializeField] private List<Node> nodes;

        public List<Node> GetAllNodes() => nodes;
        
        public Node GetNode(ActionDefinition action)
        {
            var index = nodes.FindIndex(n => n.GetHash() == action.GetHashCode());
            if (index >= 0)
            {
                var node = nodes[index];
                node.Setup(action);
                return node;
            }
            var newNode = new Node(action);
            nodes.Add(newNode);
            return newNode;
        }
    }
}