using System;
using Aspekt.Hex.Actions;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class UpgradeDetailView
    {
        private readonly GamePlan gamePlan;

        public UpgradeDetailView(GamePlan gamePlan)
        {
            this.gamePlan = gamePlan;
        }

        public void Show(VisualElement container, UpgradeAction.UpgradeDetails upgrade, Action updateContentsCallback)
        {
            var header = new Label(upgrade.title);
            header.AddToClassList("object-header");
            container.Add(header);
            
            container.Add(new Label($"Cost: {upgrade.cost.supplies} supplies, {upgrade.cost.production} production"));
            
            gamePlan.ShowGamePlan(container);

            var btn = new Button {text = "Turns"};
            btn.AddToClassList("calc-button");

            btn.clicked += () => gamePlan.CalculateGamePlan(upgrade, updateContentsCallback);
            container.Add(btn);
        }
    }
}