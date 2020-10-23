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
        
        public override List<CellUIItem.Details> ItemDetails { get; protected set; }

        public abstract int MoveRange { get; protected set; }
        public abstract int AttackRange { get; protected set; }
        public abstract int AttackDamage { get; protected set; }
        
        private void Awake()
        {
            SetupActionItems();
        }

        public override bool CanCreate(Cells.CellTypes cellType) => false;
        
        protected virtual void SetupActionItems()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
                new CellUIItem.Details { Callback = ActionAttack, Sprite = attackImage},
                new CellUIItem.Details { Callback = ActionMove, Sprite = moveImage },
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