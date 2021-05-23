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
        
        [SerializeField] private List<UpgradeSubNode> subNodes;
        [SerializeField] private bool isChildrenLinked;

        private bool isActivatingNewLink;
        private UpgradeSubNode lastSubNode;
        private UpgradeSubNode activatingSubNode;

        public bool IsGroupActivatingNewLink => isActivatingNewLink || subNodes.Any(s => s.IsActivatingNewLink);

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

            subNodes ??= new List<UpgradeSubNode>();

            for (int i = 0; i < action.upgradeDetails.Length; i++)
            {
                var subNode = GetSubNode(action.upgradeDetails[i]);
                
                subNode.Setup(this, action.upgradeDetails[i], i);
                subNode.OnEnter = OnEnteredSubNode;
                subNode.OnLeave = OnLeftSubNode;
            }

            hash = GenerateHash(action);
            Element = GetElement();
        }

        public override TreeElement GetElement()
        {
            if (Element.VisualElement != null) return Element;
            
            var element = new VisualElement();
            element.AddToClassList("node-group");

            var linkChildrenButton = new Button();
            linkChildrenButton.AddToClassList("node-group-link-trigger");
            SetupChildLinkVisuals(element, linkChildrenButton);
            linkChildrenButton.clicked += () =>
            {
                ToggleChildrenLinked();
                SetupChildLinkVisuals(element, linkChildrenButton);
            };

            element.Add(new Label(action.name));
            element.AddToClassList("node-upgrade");
            
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);
            element.Add(linkChildrenButton);

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
                subNode.SetMoveWithParent(isChildrenLinked);
                
                var ce = new ConnectionElement(this, subNode, new Color(1f, 0f, 1f, 0.29f), 5f, true);
                ce.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
                Element.Children.Add(new TreeElement() { Parent = element, SortOrder = 1, VisualElement = ce});
            }
            
            return Element;
        }

        private void SetupChildLinkVisuals(VisualElement element, Button button)
        {
            if (isChildrenLinked)
            {
                element.RemoveFromClassList("node-group-unlinked");
                button.text = "+";
                button.AddToClassList("node-group-link-trigger-active");
            }
            else
            {
                element.AddToClassList("node-group-unlinked");
                button.text = "-";
                button.RemoveFromClassList("node-group-link-trigger-active");
            }
        }
        
        private void ToggleChildrenLinked()
        {
            isChildrenLinked = !isChildrenLinked;
            foreach (var subNode in subNodes)
            {
                subNode.SetMoveWithParent(isChildrenLinked);
                subNode.OnParentMoved();
            }
        }

        public override void SetupOnClickCallbacks(Action<Node> onClickCallback)
        {
            OnClick = onClickCallback;
            foreach (var subNode in subNodes)
            {
                subNode.SetupOnClickCallbacks(onClickCallback);
            }
        }

        public UpgradeSubNode GetSubNode(UpgradeAction.UpgradeDetails upgradeDetails)
        {
            var subUpgradeHash = UpgradeSubNode.GenerateHash(upgradeDetails);
            var subNode = subNodes.FirstOrDefault(n => n.GetHash() == subUpgradeHash);
            if (subNode == null)
            {
                var index = action.upgradeDetails.ToList().FindIndex(u => u.tech == upgradeDetails.tech);
                subNode = new UpgradeSubNode(this, upgradeDetails, index);
                subNodes.Add(subNode);
            }

            return subNode;
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
            OnEnter?.Invoke(node);
            lastSubNode = (UpgradeSubNode) node;
            if (isActivatingNewLink && activatingSubNode == null)
            {
                lastSubNode.ActivatingLinkStart();
                Element.VisualElement.RemoveFromClassList(ActivatingLinkClass);
            }
        }

        private void OnLeftSubNode(Node node)
        {
            OnLeave?.Invoke(node);
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
            if (IsGroupActivatingNewLink) return;
            
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

        public override void ShowSelected()
        {
            base.ShowSelected();
            foreach (var subNode in subNodes)
            {
                subNode.ShowSelected();
            }

            foreach (var child in Element.Children)
            {
                if (child.VisualElement is ConnectionElement ce)
                {
                    ce.style.visibility = new StyleEnum<Visibility>(isChildrenLinked ? Visibility.Hidden : Visibility.Visible);
                }
            }
        }

        public override void ShowUnselected()
        {
            base.ShowUnselected();
            foreach (var subNode in subNodes)
            {
                subNode.ShowUnselected();
            }
            
            foreach (var child in Element.Children)
            {
                if (child.VisualElement is ConnectionElement ce)
                {
                    ce.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
                }
            }
        }

        protected override void OnMouseMove(MouseMoveEvent e)
        {
            base.OnMouseMove(e);
            if (IsDragged && isChildrenLinked)
            {
                foreach (var node in subNodes)
                {
                    node.OnParentMoved();
                }
            }
        }
    }
}