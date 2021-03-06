using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Util;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspekt.Hex.Upgrades
{
    public class UpgradeTester : Page
    {
        private readonly UpgradeEditor editor;

        private readonly List<BuildingCell> buildings = new List<BuildingCell>();

        private Label currencyLabel;
        private Label turnLabel;

        private PlayerData playerData;
        
        private VisualElement buildActionRoot;
        private VisualElement upgradeActionRoot;
        private VisualElement unitActionRoot;

        public override string Title => "Tester";
        public UpgradeTester(UpgradeEditor editor, VisualElement root)
        {
            this.editor = editor;
            AddToRoot(root, "UpgradeTester");
            SetupUI();
        }

        public override void UpdateContents()
        {
            Reset();
        }

        private void SetupUI()
        {
            currencyLabel = Root.Q<Label>("Currency");
            turnLabel = Root.Q<Label>("TurnIndicator");
            
            var turnButton = Root.Q<Button>("NextTurnButton");
            turnButton.Clear();
            turnButton.clicked += NextTurn;
            
            var resetButton = Root.Q<Button>("ResetButton");
            resetButton.Clear();
            resetButton.clicked += Reset;
            
            buildActionRoot = Root.Q("BuildActions");
            upgradeActionRoot = Root.Q("UpgradeActions");
            unitActionRoot = Root.Q("UnitActions");

            SetupData();
            UpdateActions();
        }

        private void Reset()
        {
            buildings.Clear();
            SetupData();
            UpdateActions();
        }

        private void NextTurn()
        {
            playerData.TurnNumber += 1;
            turnLabel.text = $"Turn {playerData.TurnNumber}";
            
            var suppliers = Cells.GetSuppliers(buildings.Select(b => b as HexCell).ToList());
            var generatedSupplies = suppliers.Sum(c => c.CalculateSupplies(playerData));
            playerData.CurrencyData.Supplies += generatedSupplies;
            
            DisplayCurrency();
            UpdateActions();
        }

        private void UpdateActions()
        {
            UpdateBuildActions();
        }

        private void UpdateBuildActions()
        {
            buildActionRoot.Clear();
            upgradeActionRoot.Clear();
            unitActionRoot.Clear();

            var uniqueBuildings = GetUniqueBuildings();
            foreach (var building in uniqueBuildings)
            {
                var buildingActions = new List<VisualElement>();
                var unitActions = new List<VisualElement>();
                var upgradeActions = new List<VisualElement>();
                
                foreach (var action in building.Actions)
                {
                    if (action is BuildAction buildAction)
                    {
                        if (buildAction.prefab is BuildingCell)
                        {
                            buildingActions.Add(CreateActionIndicator(buildAction));
                        }
                        else
                        {
                            unitActions.Add(CreateActionIndicator(buildAction));
                        }
                    }
                    else if (action is UpgradeAction upgradeAction)
                    {
                        upgradeActions.Add(CreateActionIndicator(upgradeAction));
                    }
                }
                
                AddToActionGroups(building, buildingActions, unitActions, upgradeActions);
            }
        }

        private void AddToActionGroups(BuildingCell building, List<VisualElement> buildingActions, List<VisualElement> unitActions, List<VisualElement> upgradeActions)
        {
            if (buildingActions.Any())
            {
                var buildingGroup = new VisualElement();
                buildingGroup.AddToClassList("cell-group");
                var header = new Label(building.DisplayName);
                header.AddToClassList("cell-group-header");
                buildingGroup.Add(header);
                buildingActions.ForEach(a => buildingGroup.Add(a));
                        
                buildActionRoot.Add(buildingGroup);
            }

            if (unitActions.Any())
            {
                var unitGroup = new VisualElement();
                unitGroup.AddToClassList("cell-group");
                var header = new Label(building.DisplayName);
                header.AddToClassList("cell-group-header");
                unitGroup.Add(header);
                unitActions.ForEach(a => unitGroup.Add(a));
                        
                unitActionRoot.Add(unitGroup);
            }

            if (upgradeActions.Any())
            {
                var upgradeGroup = new VisualElement();
                upgradeGroup.AddToClassList("cell-group");
                var header = new Label(building.DisplayName);
                header.AddToClassList("cell-group-header");
                upgradeGroup.Add(header);
                upgradeActions.ForEach(a => upgradeGroup.Add(a));
                        
                upgradeActionRoot.Add(upgradeGroup);
            }
        }

        private VisualElement CreateActionIndicator(BuildAction action)
        {
            var cellName = action.prefab.DisplayName;
            if (playerData.TechnologyData.HasTechnologies(action.techRequirements))
            {
                return CreateButton(
                    cellName,
                    action.prefab.Cost,
                    () => ProcessBuildAction(action),
                    newCost => UpdateActionCost(action, newCost),
                    playerData.TechnologyData.HasTechnology(action.prefab.Technology)
                );
            }
            else
            {
                var label = new Label(cellName);
                label.AddToClassList("action-unavailable");
                return label;
            }
        }

        private void UpdateActionCost(BuildAction action, Currency cost)
        {
            editor.RecordUndo(action.prefab, "Update build action cost");
            action.prefab.Cost = cost;
            EditorUtility.SetDirty(action.prefab);
            UpdateActions();
        }

        private void UpdateActionCost(UpgradeAction action, UpgradeAction.UpgradeDetails upgrade, Currency cost)
        {
            for (int i = 0; i < action.upgradeDetails.Length; i++)
            {
                if (action.upgradeDetails[i].tech == upgrade.tech && action.upgradeDetails[i].title == upgrade.title)
                {
                    editor.RecordUndo(action, "Upgrade action cost");
                    upgrade.cost = cost;
                    var allDetails = action.upgradeDetails;
                    allDetails[i] = upgrade;
                    action.upgradeDetails = allDetails;
                    EditorUtility.SetDirty(action);
                    UpdateActions();
                }
            }
        }

        private VisualElement CreateActionIndicator(UpgradeAction action)
        {
            UpgradeAction.UpgradeDetails nextUpgrade;
            try
            {
                nextUpgrade = action.upgradeDetails.First(u => !playerData.TechnologyData.HasTechnology(u.tech));
            }
            catch
            {
                var label = new Label(action.name.Replace("Upgrade", ""));
                label.AddToClassList("action-button");
                label.AddToClassList("action-button-researched");
                var costDisplay = new Label("(purchased)");
                costDisplay.AddToClassList("action-cost");
                label.Add(costDisplay);
                return label;
            }

            return CreateButton(
                nextUpgrade.title.Replace("Upgrade", ""),
                nextUpgrade.cost,
                () => ProcessUpgradeAction(nextUpgrade),
                newCost => UpdateActionCost(action, nextUpgrade, newCost),
                false
            );
        }

        private Button CreateButton(string label, Currency cost, Action onClick, Action<Currency> costUpdateAction, bool isResearched)
        {
            var btn = new Button();
            btn.AddToClassList("action-button");
            btn.Add(new Label(label));
            
            var canAfford = playerData.CurrencyData.CanAfford(cost);
            var costDisplay = NodeUtil.CreateCostField(cost, costUpdateAction, isResearched, canAfford);
            btn.Add(costDisplay);
            if (!isResearched && canAfford)
            {
                btn.clicked += onClick;
            }
            return btn;
        }

        private void ProcessBuildAction(BuildAction action)
        {
            if (action.prefab is BuildingCell building)
            {
                buildings.Add(building);
                var currencyBonus = building.GetCurrencyBonus();
                playerData.CurrencyData.Production.maximum += currencyBonus.production;
                playerData.CurrencyData.Population.maximum += currencyBonus.population;
            }
            playerData.TechnologyData.AddTechnology(action.prefab.Technology);
            ProcessCost(action.prefab.Cost);
            UpdateActions();
        }

        private void ProcessUpgradeAction(UpgradeAction.UpgradeDetails upgrade)
        {
            ProcessCost(upgrade.cost);
            playerData.TechnologyData.AddTechnology(upgrade.tech);
            UpdateActions();
        }

        private void ProcessCost(Currency cost)
        {
            playerData.CurrencyData.Supplies -= cost.supplies;
            playerData.CurrencyData.Production.utilised += cost.production;
            playerData.CurrencyData.Population.utilised += cost.population;
            DisplayCurrency();
        }

        private void DisplayCurrency()
        {
            var supplies = playerData.CurrencyData.Supplies;
            var production = playerData.CurrencyData.Production.Available;
            var population = playerData.CurrencyData.Population.Available;
            currencyLabel.text = $"{supplies}s | {production}pd | {population}pp";
        }
        
        private void SetupData()
        {
            var game = Object.FindObjectOfType<GameManager>();
            var config = InspectorUtil.GetPrivateValue<GameConfig, GameManager>("config", game);
            var cells = Object.FindObjectOfType<Cells>();
            
            config.Init(cells);
            playerData = new PlayerData(null);
            
            playerData.Init(config);
            
            var homeCell = (BuildingCell)cells.GetPrefab(Cells.CellTypes.Home);
            buildings.Add(homeCell);
            
            playerData.TurnNumber = 1;
            playerData.TechnologyData.AddTechnology(homeCell.Technology);
            playerData.CurrencyData.Supplies = config.startingSupply;
            playerData.CurrencyData.Production.maximum = homeCell.GetCurrencyBonus().production;
            playerData.CurrencyData.Population.maximum = homeCell.GetCurrencyBonus().population;

            DisplayCurrency();
            turnLabel.text = $"Turn {playerData.TurnNumber}";
        }

        private List<BuildingCell> GetUniqueBuildings()
        {
            var uniqueBuildings = new List<BuildingCell>();
            foreach (var building in buildings)
            {
                if (uniqueBuildings.All(b => b.name != building.name))
                {
                    uniqueBuildings.Add(building);
                }
            }
            return uniqueBuildings;
        }
    }
}