using System;
using Aspekt.Hex.Actions;

namespace Aspekt.Hex.Commands
{
    public class ValidatedUpgrade
    {
        public Int16 ID { get; }

        private UpgradeAction action;
        private Technology tech;

        public ValidatedUpgrade(Int16 id, UpgradeAction action, Technology tech)
        {
            ID = id;
            this.action = action;
            this.tech = tech;
        }
        
        public void Validate()
        {
            
        }
    }
}