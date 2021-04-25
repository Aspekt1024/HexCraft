using System.Collections.Generic;
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

        private readonly List<Node> nodes = new List<Node>();
        
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
            nodes.Clear();
            SetupData();
            UpdateTree();
        }

        private void UpdateTree()
        {
            foreach (var upgrade in config.techConfig.upgrades)
            {
                var node = data.techTreeData.GetNode(upgrade);
                nodeRoot.Add(node.GetElement());
            }

            foreach (var buildAction in config.techConfig.buildActions)
            {
                var node = data.techTreeData.GetNode(buildAction);
                nodeRoot.Add(node.GetElement());
            }

            
            var node = 
            var line = new VisualElement
            {
                generateVisualContent = (ctx) => DrawLine(Vector2)
            };
            nodeRoot.Add(line); 
            
        }
        
        private void OnGenerateVisualContent( MeshGenerationContext cxt )
        {
            var mesh = cxt.Allocate( 3, 3 );
            
            var vertices = new Vertex[3];
            vertices[0].position = new Vector3(0, 0, Vertex.nearZ);
            vertices[1].position = new Vector3(100, 200, Vertex.nearZ);
            vertices[2].position = new Vector3(0, 200, Vertex.nearZ);
            vertices[0].tint = Color.red;
            vertices[1].tint = Color.green;
            vertices[2].tint = Color.blue;
            
            mesh.SetAllVertices( vertices );
            mesh.SetAllIndices( new ushort[]{ 0, 1, 2 }  );
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