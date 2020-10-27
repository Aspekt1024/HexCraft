using System;
using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class HomeCell : HexCell
    {
#pragma warning disable 649
        [SerializeField] private Sprite incomeImage;
        [SerializeField] private Sprite trainingImage;
#pragma warning restore 649

        public override List<CellUIItem.Details> ItemDetails { get; protected set; }

        protected override void OnInit()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
                new CellUIItem.Details(incomeImage, IncomeItemClicked, CellData.GetCost(Cells.CellTypes.Income)),
                new CellUIItem.Details(trainingImage, TrainingItemClicked, CellData.GetCost(Cells.CellTypes.Training)),
            };
        }
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.Training || cellType == Cells.CellTypes.Income;
        }

        private void TrainingItemClicked()
        {
            foreach (var observer in Observers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.Training, this);
            }
        }

        private void IncomeItemClicked()
        {
            foreach (var observer in Observers)
            {
                observer.IndicateBuildCell(Cells.CellTypes.Income, this);
            }
        }
    }
}