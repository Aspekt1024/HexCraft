using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public abstract class UnitCell : HexCell
    {
#pragma warning disable 649
        [SerializeField] private Sprite moveImage;
        [SerializeField] private Sprite attackImage;
#pragma warning restore 649
        
        [Header("Unit Settings")]
        public int MoveRange;
        public int AttackRange;
        public int AttackDamage;
        
        public override List<CellUIItem.Details> ItemDetails { get; protected set; }

        protected override void OnInit()
        {
            SetupActionItems();
        }

        public override bool CanCreate(Cells.CellTypes cellType) => false;
        
        protected virtual void SetupActionItems()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
                new CellUIItem.Details(attackImage, ActionAttack, 0),
                new CellUIItem.Details(moveImage, ActionMove, 0),
            };
        }


        private void ActionAttack()
        {
            foreach (var observer in Observers)
            {
                observer.IndicateUnitAttack(this);
            }
        }

        private void ActionMove()
        {
            foreach (var observer in Observers)
            {
                observer.IndicateUnitMove(this);
            }
        }
        
    }
}