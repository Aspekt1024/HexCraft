using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public static class NodeUtil
    {
        public static VisualElement CreateCostField(Currency cost, Action<Currency> costUpdateAction, bool isResearched = true, bool canAfford = true)
        {
            var costDisplay = new VisualElement();
            costDisplay.AddToClassList("action-cost");
            var suppliesCost = new LongField(3) {value = cost.supplies};
            suppliesCost.RegisterValueChangedCallback(
                newSupplies =>
                {
                    cost.supplies = (int) newSupplies.newValue;
                    costUpdateAction.Invoke(cost);
                });
            suppliesCost.AddToClassList("cost-field");
            
            var productionCost = new LongField(3) {value = cost.production};
            productionCost.RegisterValueChangedCallback(
                newProduction =>
                {
                    cost.production = (int) newProduction.newValue;
                    costUpdateAction.Invoke(cost);
                });
            productionCost.AddToClassList("cost-field");
            
            costDisplay.Add(suppliesCost);
            costDisplay.Add(new Label("s "));
            costDisplay.Add(productionCost);
            costDisplay.Add(new Label("p"));
            
            if (isResearched)
            {
                costDisplay.AddToClassList("action-cost-researched");
            }
            else if (!canAfford)
            {
                costDisplay.AddToClassList("action-cost-unaffordable");
            }
            else
            {
                costDisplay.AddToClassList("action-cost-purchasable");
            }

            return costDisplay;
        }
    }
}