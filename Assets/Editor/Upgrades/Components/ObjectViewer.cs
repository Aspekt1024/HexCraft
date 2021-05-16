using System.Linq;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class ObjectViewer : Page
    {
        public override string Title => "ObjectViewer";

        private readonly VisualElement contents;
        
        private readonly TurnCalculator turnCalculator;
        
        private HexCell cell;
        
        public ObjectViewer(VisualElement root)
        {
            AddToRoot(root, "ObjectViewer");
            contents = Root.Q("ObjectContents");
            
            turnCalculator = new TurnCalculator();
            
            SetupUI();
        }

        public void ShowNodeDetails(HexCell cell)
        {
            this.cell = cell;
            UpdateContents();
        }
        
        public override void UpdateContents()
        {
            SetupUI();
        }

        private void SetupUI() 
        {
            contents.Clear();
            
            if (cell == null)
            {
                contents.Add(new Label("No cell selected"));
                return;
            }

            var header = new Label(cell.DisplayName);
            header.AddToClassList("object-header");
            contents.Add(header);
            if (cell is IProductionGenerator productionGenerator)
            {
                contents.Add(new Label("generates " + productionGenerator.GetProduction() + " production"));
            }

            var gamePlan = turnCalculator.GetCalculatedPlan();
            if (gamePlan != null)
            {
                var turnIndication = new VisualElement();
                var stepContainer = new VisualElement();
                var turnNumber = 1;
                while (gamePlan.Any())
                {
                    var node = gamePlan.Dequeue();
                    var numTurnsSinceLastAction = node.PlayerData.TurnNumber - turnNumber;
                    
                    for (int i = 0; i < numTurnsSinceLastAction; i++)
                    {
                        var nextTurnAction = new VisualElement();
                        nextTurnAction.AddToClassList("plan-step");
                        var nextTurnLabel = new Label("Next turn");
                        nextTurnLabel.AddToClassList("plan-step-action");
                        nextTurnLabel.AddToClassList("plan-step-nextturn");
                        nextTurnAction.Add(nextTurnLabel);

                        var runningSupplies = node.InitialSupplies + node.SuppliesPerTurn * (i + 1);
                        var currencyLabel = new Label($"{runningSupplies}s {node.InitialProduction}p");
                        currencyLabel.AddToClassList("plan-step-currency");
                        nextTurnAction.Add(currencyLabel);
                        
                        stepContainer.Add(nextTurnAction);
                    }
                    
                    turnNumber = node.PlayerData.TurnNumber;
                    
                    var planStep = new VisualElement();
                    planStep.AddToClassList("plan-step");
                    
                    var action = new Label(node.Action);
                    action.AddToClassList("plan-step-action");
                    planStep.Add(action);
                    var supplies = node.PlayerData.CurrencyData.Supplies;
                    var production = node.PlayerData.CurrencyData.AvailableProduction;
                    var currency = new Label($"{supplies}s {production}p");
                    currency.AddToClassList("plan-step-currency");
                    planStep.Add(currency);
                    
                    stepContainer.Add(planStep);
                }
                turnIndication.Add(new Label($"Optimal turns ({(turnNumber - 1).ToString()}):"));
                turnIndication.Add(stepContainer);
                contents.Add(turnIndication);
            }

            var btn = new Button {text = "Calculate Turns"};
            btn.AddToClassList("calc-button");

            btn.clicked += () => turnCalculator.QueueCalculation(cell, UpdateContents);
            contents.Add(btn);
                
        }
    }
}