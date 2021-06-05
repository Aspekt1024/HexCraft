using System;
using System.Collections.Generic;
using Aspekt.Hex.Util;
using UnityEditor;
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

            if (cell is BuildingCell buildingCell)
            {
                var currencyBonus = buildingCell.GetCurrencyBonus(Technology.None);
                if (currencyBonus.production > 0) container.Add(new Label($"Generates {currencyBonus.production} production"));
                if (currencyBonus.supplies > 0) container.Add(new Label($"Generates {currencyBonus.supplies} supplies"));
                if (currencyBonus.population > 0) container.Add(new Label($"Generates {currencyBonus.population} population"));
            }

            var upgradeDetails = GetUpgradeDetails(cell);
            container.Add(upgradeDetails);
            
            gamePlan.ShowGamePlan(container);

            var btn = new Button {text = "Turns"};
            btn.AddToClassList("calc-button");

            btn.clicked += () => gamePlan.CalculateGamePlan(cell, updateContentsCallback);
            container.Add(btn);
        }

        private VisualElement GetUpgradeDetails(HexCell cell)
        {
            if (!(cell is BuildingCell buildingCell)) return null;
            var upgradeDetails = InspectorUtil.GetPrivateValue<List<BuildingCell.UpgradeDetail>, BuildingCell>("upgradeDetails", buildingCell);

            var element = new VisualElement();
            for (int i = 0; i < upgradeDetails.Count; i++)
            {
                var index = i;
                var detail = upgradeDetails[index];
                var detailElement = new VisualElement();
                var currency = NodeUtil.CreateCostField(detail.currencyBonus, newCurrency => UpdateUpgradeBonus(buildingCell, index, newCurrency));
                detailElement.Add(new Label(detail.tech.ToString()));
                detailElement.Add(currency);
                element.Add(detailElement);
            }

            return element;
        }

        private void UpdateUpgradeBonus(BuildingCell cell, int upgradeIndex, Currency currency)
        {
            var upgradeDetailsField = InspectorUtil.GetPrivateField<BuildingCell>("upgradeDetails");
            var details = (List<BuildingCell.UpgradeDetail>) upgradeDetailsField.GetValue(cell);
            var detail = details[upgradeIndex];
            
            Undo.RecordObject(cell, "Update upgrade bonus");
            detail.currencyBonus = currency;
            details[upgradeIndex] = detail;
            
            upgradeDetailsField.SetValue(cell, details);
        }
    }
}