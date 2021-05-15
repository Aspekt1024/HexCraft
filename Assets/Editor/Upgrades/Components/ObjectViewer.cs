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
                var turnNumber = 0;
                while (gamePlan.Any())
                {
                    var node = gamePlan.Dequeue();
                    turnNumber = node.PlayerData.TurnNumber;
                    
                    var planStep = new VisualElement();
                    planStep.AddToClassList("plan-step");
                    
                    var action = new Label(node.Action);
                    action.AddToClassList("plan-step-action");
                    if (node.Action == "Next Turn")
                    {
                        action.AddToClassList("plan-step-nextturn");
                    }
                    planStep.Add(action);
                    var supplies = node.PlayerData.CurrencyData.Supplies.ToString();
                    var production = node.PlayerData.CurrencyData.AvailableProduction.ToString();
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