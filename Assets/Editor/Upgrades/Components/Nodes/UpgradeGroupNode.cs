using System;
using System.Collections.Generic;
using Aspekt.Hex.Actions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class UpgradeGroupNode : Node
    {
        private UpgradeAction action;
        
        // TODO list of upgrade nodes
        
        public UpgradeGroupNode(UpgradeAction action)
        {
            Setup(action);
        }
        
        public static int GenerateHash(UpgradeAction action) => Hash128.Compute("Upgrade" + action.name).GetHashCode();
        public override bool HasValidObject() => action != null;
        public override object GetObject() => action;

        public void Setup(UpgradeAction action)
        {
            this.action = action;
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

            foreach (var upgrade in action.upgradeDetails)
            {
                var e = new VisualElement();
                e.AddToClassList("node-group-subnode");
                e.Add(new Label(upgrade.title));

                element.Add(e);
            }
            
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);
            
            return element;
        }
        
    }
}