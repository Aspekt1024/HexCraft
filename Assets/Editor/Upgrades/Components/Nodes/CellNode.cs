using System;
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
        public override bool HasValidObject() => cell != null;
        public override object GetObject() => cell;

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