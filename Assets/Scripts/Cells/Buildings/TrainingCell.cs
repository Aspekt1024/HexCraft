using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class TrainingCell : HexCell
    {
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.UnitT1 || cellType == Cells.CellTypes.UnitT2;
        }

        public void ActionTrainUnitT1()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.UnitT1, this);
            }
        }

        public void ActionTrainUnitT2()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.UnitT2, this);
            }
        }
    }
}