using System;
using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HomeCell : HexCell
    {
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.Training || cellType == Cells.CellTypes.Income;
        }

        public void ActionBuildTraining()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.Training, this);
            }
        }

        public void ActionBuildFarm()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.Income, this);
            }
        }
    }
}