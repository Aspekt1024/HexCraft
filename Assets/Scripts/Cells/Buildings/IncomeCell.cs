using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class IncomeCell : HexCell
    {
        [Header("Income Settings")]
        public int CreditsPerRound = 1;
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return false;
        }
    }
}