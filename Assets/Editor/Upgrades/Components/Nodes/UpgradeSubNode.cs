using System;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class UpgradeSubNode : Node
    {
        public override int SortOrder { get; } = 90;
        
        private UpgradeGroupNode group;
        private UpgradeAction.UpgradeDetails upgrade;

        private bool moveWithParent;
        private int index;
        private bool isActivatingNewLink;

        public UpgradeSubNode(UpgradeGroupNode group, UpgradeAction.UpgradeDetails upgrade, int index)
        {
            MoveDisabled = false;
            Setup(group, upgrade, index);
        }

        public static int GenerateHash(UpgradeAction.UpgradeDetails upgrade) => Hash128.Compute("SubUpgrade" + upgrade.title).GetHashCode();
        public override bool HasValidObject() => upgrade.tech != Technology.None;
        public override object GetObject() => upgrade;
        public UpgradeAction.UpgradeDetails GetUpgradeDetails() => upgrade;
        public override ActionDefinition GetAction(TechConfig techConfig) => group.GetAction(techConfig);
        public Technology GetTechnology() => upgrade.tech;
        public bool IsActivatingNewLink => isActivatingNewLink;
        
        public void SetMoveWithParent(bool value)
        {
            MoveDisabled = value;
            moveWithParent = value;
        }

        public void Setup(UpgradeGroupNode group, UpgradeAction.UpgradeDetails upgrade, int index)
        {
            this.group = group;
            this.upgrade = upgrade;
            this.index = index;
            
            hash = GenerateHash(upgrade);
            Element = GetElement();
        }

        public override TreeElement GetElement()
        {
            if (Element.VisualElement != null) return Element;

            var element = new VisualElement { name = upgrade.title };
            element.AddToClassList("node-group-subnode");
            element.Add(new Label(upgrade.title));

            Element.VisualElement = element;
            Element.SortOrder = SortOrder;
            
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);
            
            return Element;
        }

        public override void SetupOnClickCallbacks(Action<Node> onClickCallback)
        {
            OnClick = onClickCallback;
        }

        public override Vector2 GetConnectingPosition(Vector2 fromPos)
        {
            var e = Element.VisualElement;

            var pos = position;
            pos.y += e.layout.height / 2f;
            if (fromPos.x > pos.x)
            {
                pos.x += e.layout.width;
            }
            
            return pos;
        }

        public void OnParentMoved()
        {
            if (moveWithParent)
            {
                OnMove?.Invoke();
                position = group.GetPosition();
                var e = Element.VisualElement;

                position.y += (e.layout.height + 1f) * (index + 1);
                position.x += (group.GetElement().VisualElement.layout.width - e.layout.width) / 2f;
                
                e.style.top = position.y;
                e.style.left = position.x;
            }
        }

        public override void ActivatingLinkStart()
        {
            if (group.IsGroupActivatingNewLink) return;
            isActivatingNewLink = true;
            base.ActivatingLinkStart();
        }

        public override void ActivatingLinkEnd()
        {
            isActivatingNewLink = false;
            base.ActivatingLinkEnd();
        }
    }
}