using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HomeCell : BuildingCell
    {
        public override Technology Technology { get; } = Technology.None;
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.Training
                   || cellType == Cells.CellTypes.Income
                   || cellType == Cells.CellTypes.Blacksmith;
        }

        public override void OnTechAdded(Technology tech)
        {
            base.OnTechAdded(tech);
        }
    }
}