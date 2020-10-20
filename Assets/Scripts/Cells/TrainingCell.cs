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
        
        public override string DisplayName { get; } = "Training";

        public override List<CellUIItem.Details> ItemDetails { get; protected set; }

        private void Awake()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
                new CellUIItem.Details { Sprite = t1UnitSprite, Callback = T1UnitClicked },
                new CellUIItem.Details { Sprite = t2UnitSprite, Callback = T2UnitClicked }
            };
        }

        private void T1UnitClicked()
        {
            Debug.Log("t1 unit");
        }

        private void T2UnitClicked()
        {
            Debug.Log("t1 unit");
        }
    }
}