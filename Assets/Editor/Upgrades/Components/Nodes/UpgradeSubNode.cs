using System;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class UpgradeSubNode : Node
    {
        public override int SortOrder { get; } = 90;
        
        private readonly int index;
        
        private UpgradeGroupNode group;
        private UpgradeAction.UpgradeDetails upgrade;

        public UpgradeSubNode(UpgradeGroupNode group, UpgradeAction.UpgradeDetails upgrade, int index)
        {
            this.index = index;
            MoveDisabled = true;
            Setup(group, upgrade);
        }

        public static int GenerateHash(UpgradeAction.UpgradeDetails upgrade) => Hash128.Compute("SubUpgrade" + upgrade.title).GetHashCode();
        public override bool HasValidObject() => upgrade.tech != Technology.None;
        public override object GetObject() => upgrade;
        public UpgradeAction.UpgradeDetails GetUpgradeDetails() => upgrade;
        public override ActionDefinition GetAction(TechConfig techConfig) => group.GetAction(techConfig);
        public Technology GetTechnology() => upgrade.tech;
        
        public void Setup(UpgradeGroupNode group, UpgradeAction.UpgradeDetails upgrade)
        {
            this.group = group;
            this.upgrade = upgrade;
            
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
            Element.SortOrder = SortOrder + index;
            
            element.AddManipulator(this);
            
            UpdatePosition();
            
            return Element;
        }

        public override void SetupOnClickCallbacks(Action<Node> onClickCallback)
        {
            OnClick = onClickCallback;
        }

        public override Vector2 GetConnectingPosition(Vector2 fromPos)
        {
            var e = Element.VisualElement;
            var pos = group.GetPosition();
            pos.x += e.layout.x;
            pos.y += e.layout.y + e.layout.height / 2f;
            
            if (fromPos.x > pos.x)
            {
                pos.x += e.layout.width;
            }
            
            return pos;
        }

        public void OnParentMoved()
        {
            OnMove?.Invoke();
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            position = group.GetPosition() + Element.VisualElement.layout.position;
        }
        
        private VisualElement GetElementInParent()
        {
            var groupElement = group.GetElement().VisualElement;
            var e = groupElement.Children().FirstOrDefault(v => v.name == upgrade.title);
            return e;
        }
    }
}