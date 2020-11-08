using System;
using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HomeCell : BuildingCell
    {
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.Training
                   || cellType == Cells.CellTypes.Income
                   || cellType == Cells.CellTypes.Blacksmith;
        }

        public void ActionBuildTraining()
        {
            EventObservers.ForEach(o => o.IndicateBuildCell(Cells.CellTypes.Training, this));
        }

        public void ActionBuildFarm()
        {
            EventObservers.ForEach(o => o.IndicateBuildCell(Cells.CellTypes.Income, this));
        }

        public void ActionBuildBlacksmith()
        {
            EventObservers.ForEach(o => o.IndicateBuildCell(Cells.CellTypes.Blacksmith, this));
        }
    }
}