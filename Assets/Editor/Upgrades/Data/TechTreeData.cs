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
            var hash = Node.GenerateHash(action);
            var node = GetNodeFromHash(hash);
            
            if (node == null)
            {
                node = new Node(action);
                nodes.Add(node);
            }
            else
            {
                node.Setup(action);
            }
            
            return node;
        }

        public Node GetNode(HexCell cell)
        {
            var hash = Node.GenerateHash(cell);
            var node = GetNodeFromHash(hash);

            if (node == null)
            {
                node = new Node(cell);
                nodes.Add(node);
            }
            else
            {
                node.Setup(cell);
            }

            return node;
        }

        public void Purge()
        {
            nodes.Clear();
        }

        public void Clean()
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (!nodes[i].HasValidObject() || nodes[i].GetObject() is UnitAction)
                {
                    nodes.RemoveAt(i);
                }
            }
        }

        private Node GetNodeFromHash(int hash)
        {
            var index = nodes.FindIndex(n => n.GetHash() == hash);
            return index >= 0 ? nodes[index] : null;
        }
    }
}