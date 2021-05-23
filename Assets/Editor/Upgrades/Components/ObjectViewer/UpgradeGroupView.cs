using System;
using Aspekt.Hex.Actions;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class UpgradeGroupView
    {
        public void Show(VisualElement container, UpgradeAction upgradeAction, Action updateContentsCallback)
        {
            var header = new Label(upgradeAction.group.ToString());
            header.AddToClassList("object-header");
            container.Add(header);
        }
    }
}