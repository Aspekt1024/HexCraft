using System;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class CellDetailView
    {
        private readonly GamePlan gamePlan;

        public CellDetailView(GamePlan gamePlan)
        {
            this.gamePlan = gamePlan;
        }

        public void Show(VisualElement container, HexCell cell, Action updateContentsCallback)
        {
            var header = new Label(cell.DisplayName);
            header.AddToClassList("object-header");
            container.Add(header);
            if (cell is IProductionGenerator productionGenerator)
            {
                container.Add(new Label("generates " + productionGenerator.GetProduction() + " production"));
            }
            
            gamePlan.ShowGamePlan(container);

            var btn = new Button {text = "Turns"};
            btn.AddToClassList("calc-button");

            btn.clicked += () => gamePlan.CalculateGamePlan(cell, updateContentsCallback);
            container.Add(btn);
        }
    }
}