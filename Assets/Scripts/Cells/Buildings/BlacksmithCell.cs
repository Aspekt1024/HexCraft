using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class BlacksmithCell : BuildingCell
    {
        public override Technology Technology { get; } = Technology.Blacksmith;

        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return false;
        }

        public override void OnTechAdded(Technology tech)
        {
            
        }
    }
}