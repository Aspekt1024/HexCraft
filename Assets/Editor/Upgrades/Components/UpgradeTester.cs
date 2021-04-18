using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class UpgradeTester : Page
    {
        private readonly UpgradeEditor editor;

        private readonly List<BuildingCell> buildings = new List<BuildingCell>();
        
        private int supply;
        private int production;
        private List<Technology> tech = new List<Technology>();

        public override string Title => "Tester";
        public UpgradeTester(UpgradeEditor editor, VisualElement root)
        {
            this.editor = editor;
            AddToRoot(root, "UpgradeTester");
            SetupUI();
        }

        public override void UpdateContents()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            SetupData();
            UpdateActions();
        }

        private void UpdateActions()
        {
            Root.Clear();
            
            var validActions = GetValidBuildActions();
            var builds = new VisualElement();
            builds.Add(new Label("Build Actions"));
            foreach (var action in validActions)
            {
                var cellName = action.prefab.DisplayName;
                
                var btn = new Button { text = cellName };
                btn.clicked += () =>
                {
                    ProcessAction(action);
                };
                
                builds.Add(btn);
            }
            Root.Add(builds);
        }

        private void ProcessAction(BuildAction action)
        {
            buildings.Add((BuildingCell) action.prefab);
            tech.Add(action.prefab.Technology);
            UpdateActions();
        }
        
        private void SetupData()
        {
            var cells = Object.FindObjectOfType<Cells>();
            var homeCell = (BuildingCell)cells.GetPrefab(Cells.CellTypes.Base);
            buildings.Add(homeCell);
            tech.Add(homeCell.Technology);

            var game = Object.FindObjectOfType<GameManager>();
            // TODO get currency
            // TODO private field util
        }

        private List<BuildAction> GetValidBuildActions()
        {
            var uniqueBuildings = new List<BuildingCell>();
            foreach (var building in buildings)
            {
                if (uniqueBuildings.All(b => b.name != building.name))
                {
                    uniqueBuildings.Add(building);
                }
            }
            
            var availableBuildActions = uniqueBuildings
                .SelectMany(b => b.Actions)
                .OfType<BuildAction>()
                .Where(a => a.prefab is BuildingCell).ToList();

            return availableBuildActions
                .Where(b => b.techRequirements.All(t => tech.Contains(t)))
                .ToList();
        }
    }
}