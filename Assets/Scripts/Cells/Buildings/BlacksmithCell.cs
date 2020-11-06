using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class BlacksmithCell : BuildingCell
    {
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            // TODO check can create in networking
            return cellType == Cells.CellTypes.MeleeUnit || cellType == Cells.CellTypes.UnitT2;
        }

        public void ActionUpgradeArmor()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.MeleeUnit, this);
            }
        }
        
        public void ActionUpgradeWeapons()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.MeleeUnit, this);
            }
        }
        
        public void ActionUpgradeShields()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.MeleeUnit, this);
            }
        }
    }
}