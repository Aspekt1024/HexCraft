using System;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class CellNode : Node
    {
        public override int SortOrder => 100;

        private HexCell cell;
        
        public CellNode(HexCell cell)
        {
            Setup(cell);
        }
        
        public static int GenerateHash(HexCell c) => Hash128.Compute("Cell" + c.name).GetHashCode();
        
        public override ActionDefinition GetAction(TechConfig techConfig)
        {
            foreach (var buildAction in techConfig.buildActions)
            {
                if (buildAction.prefab.cellType == cell.cellType)
                {
                    return buildAction;
                }
            }
            return null;
        }

        public override bool HasValidObject() => cell != null;
        public override object GetObject() => cell;
        public HexCell GetCell() => cell;

        public void Setup(HexCell cell)
        {
            this.cell = cell;
            if (cell is BuildingCell buildingCell)
            {
                buildingCell.SetUpgradeLevel(Technology.None);
            }
            hash = GenerateHash(cell);
            Element = GetElement();
        }

        public override TreeElement GetElement()
        {
            if (Element.VisualElement != null) return Element;
            
            var element = new VisualElement();
            element.AddToClassList("node");

            element.Add(new Label(cell.DisplayName));
            var costField = NodeUtil.CreateCostField(cell.Cost, OnCostUpdated);
            element.Add(costField);
            
            element.AddToClassList(cell is UnitCell ? "node-unit" : "node-building");
            
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);

            Element.VisualElement = element;
            Element.SortOrder = SortOrder;
            
            return Element;
        }

        public override void SetupOnClickCallbacks(Action<Node> onClickCallback)
        {
            OnClick = onClickCallback;
        }

        private void OnCostUpdated(Currency newCost)
        {
            Undo.RecordObject(cell, "Upgrade cell cost");
            cell.Cost = newCost;
            EditorUtility.SetDirty(cell);
        }
    }
}