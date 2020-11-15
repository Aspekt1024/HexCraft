using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class TrainingCell : BuildingCell
    {
        public override Technology Technology { get; } = Technology.Barracks;
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.MeleeUnit || cellType == Cells.CellTypes.UnitT2;
        }

        public void ActionTrainMeleeUnit()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.MeleeUnit, this);
            }
        }
    }
}