using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class IncomeCell : HexCell
    {
#pragma warning disable 649
        [SerializeField] private Transform orb;
        [SerializeField] private float orbSpeed = 160f;
#pragma warning restore 649

        [Header("Income Settings")]
        public int CreditsPerRound = 1;
        
        public override List<CellUIItem.Details> ItemDetails { get; protected set; }

        protected override void OnInit()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
            };
        }
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return false;
        }
        
        private void Update()
        {
            orb.RotateAround(transform.position, Vector3.up, orbSpeed * Time.deltaTime);
        }
    }
}