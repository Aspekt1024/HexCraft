using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Util;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspekt.Hex.Upgrades
{
    public class TechTree : Page
    {
        private readonly UpgradeEditor editor;
        private readonly UpgradeEditorData data;

        public override string Title => "Tech Tree";

        private VisualElement nodeRoot;
        private GameConfig config;

        public TechTree(UpgradeEditor editor, VisualElement root, UpgradeEditorData data)
        {
            this.editor = editor;
            this.data = data;
            AddToRoot(root, "TechTree");
            SetupUI();
        }

        public override void UpdateContents()
        {
            Reset();
        }
        
        private void SetupUI()
        {
            nodeRoot = Root.Q("Nodes");

            SetupData();
            UpdateTree();
        }

        private void Reset()
        {
            SetupData();
            UpdateTree();
        }

        private void UpdateTree()
        {
            foreach (var upgrade in config.techConfig.upgrades)
            {
                //var node = data.techTreeData.GetNode(upgrade);
                //nodeRoot.Add(node.GetElement());
            }

            foreach (var buildAction in config.techConfig.buildActions)
            {
                var node = data.techTreeData.GetNode(buildAction);
                
                var dependencies = GetDependencies(buildAction);
                dependencies.ForEach(d =>
                {
                    var line = new ConnectionElement(d, node);
                    nodeRoot.Add(line);
                });
            }
            
            data.techTreeData.GetAllNodes().Where(n => n.IsValid()).ToList().ForEach(n => nodeRoot.Add(n.GetElement()));
        }

        private List<Node> GetDependencies(BuildAction action)
        {
            var techRequirements = new List<Technology>(action.techRequirements);
            var dependencies = new List<Node>();
            
            // TODO find the node that builds it
            foreach (var buildAction in config.techConfig.buildActions)
            {
                if (techRequirements.Contains(buildAction.prefab.Technology))
                {
                    dependencies.Add(data.techTreeData.GetNode(buildAction));
                }
            }

            return dependencies;
        }

        private void SetupData()
        {
            var game = Object.FindObjectOfType<GameManager>();
            config = InspectorUtil.GetPrivateValue<GameConfig, GameManager>("config", game);
            
            var cells = Object.FindObjectOfType<Cells>();
            var homeCell = (BuildingCell)cells.GetPrefab(Cells.CellTypes.Base);
            AddNode(homeCell);
        }

        private void AddNode(HexCell cell)
        {
        }

        private void AddTechRequirement(BuildAction action, Technology tech)
        {
            action.techRequirements.Add(tech);
        }

        private void RemoveTechRequirement(BuildAction action, Technology tech)
        {
            action.techRequirements.Remove(tech);
        }

    }
}