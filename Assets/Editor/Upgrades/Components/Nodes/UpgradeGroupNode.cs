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
        public override int SortOrder => 0;
        
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

            if (subNodes == null)
            {
                subNodes = new List<UpgradeSubNode>();
                for (int i = 0; i < action.upgradeDetails.Length; i++)
                {
                    var subNode = new UpgradeSubNode(this, action.upgradeDetails[i], i);
                    subNodes.Add(subNode);
                    subNode.OnEnter = OnEnteredSubNode;
                    subNode.OnLeave = OnLeftSubNode;
                }
            }
            
            hash = GenerateHash(action);
            Element = GetElement();
        }

        public override TreeElement GetElement()
        {
            if (Element.VisualElement != null) return Element;
            
            var element = new VisualElement();
            element.AddToClassList("node-group");

            element.Add(new Label(action.name));
            element.AddToClassList("node-upgrade");
            
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);

            Element.VisualElement = element;
            Element.SortOrder = SortOrder;
            
            Element.Children = new List<TreeElement>();
            foreach (var subNode in subNodes)
            {
                var subElement = subNode.GetElement();
                Element.Children.Add(new TreeElement()
                {
                    Parent = element,
                    VisualElement = subElement.VisualElement,
                    SortOrder = subElement.SortOrder,
                });
            }
            
            return Element;
        }

        public override void SetupOnClickCallbacks(Action<Node> onClickCallback)
        {
            OnClick = onClickCallback;
            foreach (var subNode in subNodes)
            {
                subNode.SetupOnClickCallbacks(onClickCallback);;
            }
        }

        public UpgradeSubNode GetSubNode(UpgradeAction.UpgradeDetails upgradeDetails)
        {
            return subNodes.FirstOrDefault(n => n.GetTechnology() == upgradeDetails.tech);
        }

        public Technology GetSelectedTech() => activatingSubNode?.GetTechnology() ?? Technology.None;
        public UpgradeSubNode GetDependentSubNode() => lastSubNode;

        public override Vector2 GetConnectingPosition(Vector2 fromPos)
        {
            var e = Element.VisualElement;
            var pos = position;

            var dist = pos - fromPos;
            if (Mathf.Abs(dist.y) > Mathf.Abs(dist.x))
            {
                pos.x += e.layout.width / 2f;

                if (fromPos.y > pos.y)
                {
                    pos.y += e.layout.height;
                }
            }
            else
            {
                pos.y += 10f;
            
                if (fromPos.x > pos.x)
                {
                    pos.x += e.layout.width;
                }
            }

            return pos;
        }

        private void OnEnteredSubNode(Node node)
        {
            lastSubNode = (UpgradeSubNode) node;
            if (isActivatingNewLink && activatingSubNode == null)
            {
                lastSubNode.ActivatingLinkStart();
                Element.VisualElement.RemoveFromClassList(ActivatingLinkClass);
            }
        }

        private void OnLeftSubNode(Node node)
        {
            if (lastSubNode == node)
            {
                lastSubNode = null;
            }

            if (isActivatingNewLink && activatingSubNode == null)
            {
                node.ActivatingLinkEnd();
                Element.VisualElement.AddToClassList(ActivatingLinkClass);
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
                activatingSubNode = null;
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