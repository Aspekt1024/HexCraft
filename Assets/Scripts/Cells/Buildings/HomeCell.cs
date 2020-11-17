using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HomeCell : BuildingCell, ISuppliesGenerator, IProductionGenerator
    {
        public override Technology Technology { get; } = Technology.None;
        public int production = 1;
        public int suppliesPerRound = 2;
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.Training
                   || cellType == Cells.CellTypes.Income
                   || cellType == Cells.CellTypes.Blacksmith;
        }

        public int GetSupplies()
        {
            return suppliesPerRound;
        }

        public int GetProduction()
        {
            return production;
        }
    }
}