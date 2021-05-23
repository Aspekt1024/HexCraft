using System;
using System.Linq;
using Aspekt.Hex.Actions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class GamePlan
    {
        private readonly TurnCalculator turnCalculator;
        
        private struct StepRowDetails
        {
            public int TurnNumber;
            public string Label;
            public string StyleClass;
            public int InitialSupplies;
            public int InitialProduction;
            public int SupplyDifference;
            public int ProductionDifference;
        }
        
        public GamePlan()
        {
            turnCalculator = new TurnCalculator();
        }

        public void CalculateGamePlan(HexCell cell, Action onGamePlanCalculatedCallback)
        {
            turnCalculator.QueueCalculation(cell, onGamePlanCalculatedCallback);
        }
        
        public void CalculateGamePlan(UpgradeAction.UpgradeDetails upgradeDetails, Action onGamePlanCalculatedCallback)
        {
            turnCalculator.QueueCalculation(upgradeDetails, onGamePlanCalculatedCallback);
        }
        
        public void ShowGamePlan(VisualElement container)
        {
            var gamePlan = turnCalculator.GetCalculatedPlan();
            if (gamePlan == null || !gamePlan.Any()) return;
            
            var turnIndication = new VisualElement();
            var stepContainer = new VisualElement();
            var turnNumber = 1;
            while (gamePlan.Any())
            {
                var node = gamePlan.Dequeue();
                ShowNextTurnSteps(node, turnNumber, stepContainer);
                
                var numTurnsSinceLastAction = node.PlayerData.TurnNumber - turnNumber;
                var suppliesAtStartOfTurn = node.InitialSupplies + node.SuppliesPerTurn * numTurnsSinceLastAction;
                
                turnNumber = node.PlayerData.TurnNumber;

                var details = new StepRowDetails
                {
                    TurnNumber = turnNumber,
                    Label = node.Action,
                    InitialSupplies = suppliesAtStartOfTurn,
                    InitialProduction = node.InitialProduction,
                    SupplyDifference = node.PlayerData.CurrencyData.Supplies - suppliesAtStartOfTurn,
                    ProductionDifference = node.PlayerData.CurrencyData.AvailableProduction - node.InitialProduction,
                };
                var step = CreateStepRow(details);
                stepContainer.Add(step);
            }
            
            turnIndication.Add(new Label($"Optimal turns ({turnNumber}):"));
            turnIndication.Add(stepContainer);
            container.Add(turnIndication);
        }

        private void ShowNextTurnSteps(TurnCalculatorNode node, int turnNumber, VisualElement container)
        {
            var numTurnsSinceLastAction = node.PlayerData.TurnNumber - turnNumber;
            for (int i = 0; i < numTurnsSinceLastAction; i++)
            {
                var details = new StepRowDetails
                {
                    TurnNumber = turnNumber + i + 1,
                    Label = "Next turn",
                    StyleClass = "plan-step-nextturn",
                    InitialSupplies = node.InitialSupplies + node.SuppliesPerTurn * i,
                    InitialProduction = node.InitialProduction,
                    SupplyDifference = node.SuppliesPerTurn,
                    ProductionDifference = 0,
                };
                var step = CreateStepRow(details);
                
                container.Add(step);
            }
        }
        
        private static VisualElement CreateStepRow(StepRowDetails details)
        {
            var step = new VisualElement();
            step.AddToClassList("plan-step");
            if (!string.IsNullOrEmpty(details.StyleClass))
            {
                step.AddToClassList("plan-step-nextturn");
            }

            var turnLabel = new Label(details.TurnNumber.ToString());
            turnLabel.AddToClassList("plan-step-turn");
            step.Add(turnLabel);
            
            var stepLabel = new Label(details.Label);
            stepLabel.AddToClassList("plan-step-action");
            step.Add(stepLabel);

            step.Add(CreateStepCurrencyElement(details));
            
            return step;
        }

        private static VisualElement CreateStepCurrencyElement(StepRowDetails details)
        {
            var currencyContainer = new VisualElement();
            currencyContainer.AddToClassList("plan-step-currency-container");

            currencyContainer.Add(CreateCurrencyDiffElement(details.SupplyDifference, details.ProductionDifference));
            
            var currencyRunningLabel = new VisualElement();
            currencyRunningLabel.AddToClassList("plan-step-currency");
            currencyRunningLabel.Add(CreateCurrencyLabel(details.InitialSupplies + details.SupplyDifference, "s"));
            currencyRunningLabel.Add(CreateCurrencyLabel(details.InitialProduction + details.ProductionDifference, "p"));
            
            currencyContainer.Add(currencyRunningLabel);
            return currencyContainer;
        }

        private static VisualElement CreateCurrencyDiffElement(int supplyDiff, int prodDiff)
        {
            var diffElement = new VisualElement();
            diffElement.AddToClassList("plan-step-currency");
            diffElement.AddToClassList("plan-step-currency-diff");
            diffElement.Add(CreateCurrencyDiffLabel(supplyDiff, "s"));
            diffElement.Add(CreateCurrencyDiffLabel(prodDiff, "p"));
            return diffElement;
        }

        private static Label CreateCurrencyLabel(int value, string suffix)
        {
            var label = new Label($"{value}{suffix}");
            label.AddToClassList("plan-step-currency-item");
            return label;
        }

        private static Label CreateCurrencyDiffLabel(int diff, string suffix)
        {
            var prefix = diff > 0 ? "+" : "";
            var diffLabel = new Label($"{prefix}{diff}{suffix}");
            diffLabel.AddToClassList("plan-step-currency-item");
            if (Mathf.Abs(diff) > 0)
            {
                diffLabel.AddToClassList(diff > 0 ? "plan-step-currency-item-pos" : "plan-step-currency-item-neg");
            }
            return diffLabel;
        }
    }
}