using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class BlacksmithCell : BuildingCell
    {
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return false;
        }
        
        public void ActionUpgrade(ActionDefinition action)
        {
            if (action is UpgradeAction upgradeAction)
            {
                var tech = upgradeAction.GetNextTech();
                EventObservers.ForEach(o => o.TryPurchaseTech(tech));
            }
            else
            {
                Debug.LogError("unexpected blacksmith action: " + action.name);
            }
        }
    }
}