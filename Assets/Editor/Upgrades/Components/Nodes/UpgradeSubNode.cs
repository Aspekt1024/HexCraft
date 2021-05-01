using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class UpgradeSubNode : Node
    {
        private UpgradeGroupNode group;
        private UpgradeAction.UpgradeDetails upgrade;

        public UpgradeSubNode(UpgradeGroupNode group, UpgradeAction.UpgradeDetails upgrade)
        {
            MoveDisabled = true;
            Setup(group, upgrade);
        }

        public static int GenerateHash(UpgradeAction.UpgradeDetails upgrade) => Hash128.Compute("SubUpgrade" + upgrade.title).GetHashCode();
        public override bool HasValidObject() => upgrade.tech != Technology.None;
        public override object GetObject() => upgrade;
        public override ActionDefinition GetAction(TechConfig techConfig) => group.GetAction(techConfig);
        public Technology GetTechnology() => upgrade.tech;
        
        public void Setup(UpgradeGroupNode group, UpgradeAction.UpgradeDetails upgrade)
        {
            this.group = group;
            this.upgrade = upgrade;
            hash = GenerateHash(upgrade);
            element = GetElement();
        }

        public override VisualElement GetElement()
        {
            if (element != null) return element;

            element = new VisualElement { name = upgrade.title };
            element.AddToClassList("node-group-subnode");
            element.Add(new Label(upgrade.title));

            element.AddManipulator(this);

            return element;
        }


        public override Vector2 GetInputPosition() => GetCenterPosition();
        public override Vector2 GetOutputPosition() => GetCenterPosition();

        private Vector2 GetCenterPosition()
        {
            var groupElement = group.GetElement();
            var left = groupElement.style.left.value.value;
            var top = groupElement.style.top.value.value;
            
            var e = groupElement.Children().FirstOrDefault(v => v.name == upgrade.title);
            if (e != null)
            {
                left += e.layout.x + e.layout.width / 2f;
                top += e.layout.y + e.layout.height / 2f;
            }

            return new Vector2(left, top);
        }

        public void OnParentMoved()
        {
            OnMove?.Invoke();
        }
    }
}