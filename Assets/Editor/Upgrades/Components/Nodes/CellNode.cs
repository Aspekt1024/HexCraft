using System;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class CellNode : Node
    {
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
            hash = GenerateHash(cell);
            element = GetElement();
        }

        public override VisualElement GetElement()
        {
            if (element != null) return element;
            
            element = new VisualElement();
            element.AddToClassList("node");

            element.Add(new Label(cell.DisplayName));
            element.AddToClassList(cell is UnitCell ? "node-unit" : "node-building");
            
            element.style.top = position.y;
            element.style.left = position.x;
            
            element.AddManipulator(this);
            
            return element;
        }
    }
}