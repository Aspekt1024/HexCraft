using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class UpgradeGroupNode : Node
    {
        private UpgradeAction action;
        
        private List<UpgradeSubNode> subNodes;

        private bool isActivatingNewLink;
        private UpgradeSubNode lastSubNode;
        private UpgradeSubNode activatingSubNode;
        
        public UpgradeGroupNode(UpgradeAction action)
        {
            Setup(action);
        } 
        
        public static int GenerateHash(UpgradeAction action) => Hash128.Compute("Upgrade" + action.name).GetHashCode();
        public override bool HasValidObject() => action != null;
        public override object GetObject() => action;
        public override ActionDefinition GetAction(TechConfig techConfig) => action;
        public UpgradeAction GetUpgradeAction() => action;

        public void Setup(UpgradeAction action)
        {
            this.action = action;

            subNodes = new List<UpgradeSubNode>();
            foreach (var upgrade in action.upgradeDetails)
            {
                var subNode = new UpgradeSubNode(this, upgrade);
                subNodes.Add(subNode);
                subNode.OnEnter = OnEnteredSubNode;
                subNode.OnLeave = OnLeftSubNode;
            }
            
            hash = GenerateHash(action);
            element = GetElement();
        }

        public override VisualElement GetElement()
        {
            if (element != null) return element;
            
            element = new VisualElement();
            element.AddToClassList("node-group");

            element.Add(new Label(action.name));
            element.AddToClassList("node-upgrade");
            
            foreach (var subNode in subNodes)
            {
                element.Add(subNode.GetElement());
            }
            
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);
            
            return element;
        }

        public UpgradeSubNode GetSubNode(UpgradeAction.UpgradeDetails upgradeDetails)
        {
            return subNodes.FirstOrDefault(n => n.GetTechnology() == upgradeDetails.tech);
        }

        public Technology GetSelectedTech() => activatingSubNode?.GetTechnology() ?? Technology.None;
        public UpgradeSubNode GetDependentSubNode() => lastSubNode;

        private void OnEnteredSubNode(Node node)
        {
            lastSubNode = (UpgradeSubNode) node;
            if (isActivatingNewLink)
            {
                lastSubNode.ActivatingLinkStart();
                element.RemoveFromClassList(ActivatingLinkClass);
            }
        }

        private void OnLeftSubNode(Node node)
        {
            if (lastSubNode == node)
            {
                lastSubNode = null;
            }

            if (isActivatingNewLink)
            {
                node.ActivatingLinkEnd();
                element.AddToClassList(ActivatingLinkClass);
            }
        }

        public override void ActivatingLinkStart()
        {
            if (isActivatingNewLink) return;
            
            isActivatingNewLink = true;
            if (lastSubNode != null)
            {
                activatingSubNode = lastSubNode;
                activatingSubNode.ActivatingLinkStart();
            }
            else
            {
                base.ActivatingLinkStart();
            }
        }

        public override void ActivatingLinkEnd()
        {
            isActivatingNewLink = false;
            
            lastSubNode?.ActivatingLinkEnd();
            if (activatingSubNode != null)
            {
                activatingSubNode.ActivatingLinkEnd();
            }
            else
            {
                base.ActivatingLinkEnd();
            }
        }

        protected override void OnMouseMove(MouseMoveEvent e)
        {
            base.OnMouseMove(e);
            if (IsDragged)
            {
                foreach (var node in subNodes)
                {
                    node.OnParentMoved();
                }
            }
        }
    }
}