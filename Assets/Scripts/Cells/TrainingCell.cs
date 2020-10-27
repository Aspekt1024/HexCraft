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

        protected override void OnInit()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
                new CellUIItem.Details(t1UnitSprite, T1UnitClicked, CellData.GetCost(Cells.CellTypes.UnitT1)),
                new CellUIItem.Details(t2UnitSprite, T2UnitClicked, CellData.GetCost(Cells.CellTypes.UnitT2)),
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