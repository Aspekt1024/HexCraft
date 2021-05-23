using System.Linq;
using Aspekt.Hex.Actions;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class ObjectViewer : Page
    {
        public override string Title => "ObjectViewer";

        private readonly VisualElement contents;

        private readonly CellDetailView cellDetailView;
        private readonly UpgradeDetailView upgradeDetailView;
        private readonly UpgradeGroupView upgradeGroupDetailView;
        
        private HexCell cell;
        private UpgradeAction upgrade;
        private UpgradeAction.UpgradeDetails upgradeDetails;
        
        public ObjectViewer(VisualElement root)
        {
            var gamePlan = new GamePlan();
            cellDetailView = new CellDetailView(gamePlan);
            upgradeDetailView = new UpgradeDetailView(gamePlan);
            upgradeGroupDetailView = new UpgradeGroupView();
            
            AddToRoot(root, "ObjectViewer");
            contents = Root.Q("ObjectContents");
            
            SetupUI();
        }

        public void ShowNodeDetails(HexCell cell)
        {
            if (this.cell == cell) return;
            
            this.cell = cell;
            upgrade = null;
            upgradeDetails.tech = Technology.None;
            UpdateContents();
        }

        public void ShowNodeDetails(ActionDefinition action)
        {
            if (upgrade == action) return;
            if (!(action is UpgradeAction upgradeAction)) return;
            
            upgrade = upgradeAction;
            cell = null;
            upgradeDetails.tech = Technology.None;
            UpdateContents();
        }

        public void ShowNodeDetails(UpgradeAction.UpgradeDetails upgradeDetails)
        {
            if (this.upgradeDetails.tech == upgradeDetails.tech) return;
            
            this.upgradeDetails = upgradeDetails;
            cell = null;
            upgrade = null;
            UpdateContents();
        }
        
        public override void UpdateContents()
        {
            SetupUI();
        }

        private void SetupUI() 
        {
            contents.Clear();
            
            if (cell == null && upgradeDetails.tech == Technology.None && upgrade == null)
            {
                contents.Add(new Label("Click on a node to show its details"));
                return;
            }

            if (cell != null)
            {
                cellDetailView.Show(contents, cell, UpdateContents);
            }
            else if (upgradeDetails.tech != Technology.None)
            {
                upgradeDetailView.Show(contents, upgradeDetails, UpdateContents);
            }
            else if (upgrade != null)
            {
                upgradeGroupDetailView.Show(contents, upgrade, UpdateContents);
            }
        }
    }
}