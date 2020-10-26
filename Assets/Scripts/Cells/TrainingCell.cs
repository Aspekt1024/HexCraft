using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class TrainingCell : HexCell
    {
#pragma warning disable 649
        [SerializeField] private Sprite t1UnitSprite;
        [SerializeField] private Sprite t2UnitSprite;
#pragma warning restore 649

        public override List<CellUIItem.Details> ItemDetails { get; protected set; }

        private void Awake()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
                new CellUIItem.Details { Sprite = t1UnitSprite, Callback = T1UnitClicked },
                new CellUIItem.Details { Sprite = t2UnitSprite, Callback = T2UnitClicked }
            };
        }
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.UnitT1 || cellType == Cells.CellTypes.UnitT2;
        }

        private void T1UnitClicked()
        {
            foreach (var observer in Observers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.UnitT1, this);
            }
        }

        private void T2UnitClicked()
        {
            foreach (var observer in Observers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.UnitT2, this);
            }
        }
    }
}