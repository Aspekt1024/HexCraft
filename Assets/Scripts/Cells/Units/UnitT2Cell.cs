using System.Collections.Generic;
using Aspekt.Hex.UI;

namespace Aspekt.Hex
{
    public class UnitT2Cell : UnitCell
    {
        
        public override string DisplayName { get; } = "Unit (T2)";


        public override int MoveRange { get; protected set; }
        public override int AttackRange { get; protected set; }
        public override int AttackDamage { get; protected set; }

        private void Start()
        {
            MoveRange = 2;
            AttackRange = 2;
            AttackDamage = 1;
        }
    }
}