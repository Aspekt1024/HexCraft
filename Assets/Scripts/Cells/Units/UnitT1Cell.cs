using System;
using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex
{
    public class UnitT1Cell : UnitCell
    {
        
        public override string DisplayName { get; } = "Unit (T1)";


        public override int MoveRange { get; protected set; }
        public override int AttackRange { get; protected set; }
        public override int AttackDamage { get; protected set; }

        private void Start()
        {
            MoveRange = 1;
            AttackRange = 1;
            AttackDamage = 1;
        }
    }
}