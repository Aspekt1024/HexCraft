using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex.Commands
{
    public class CommandValidator
    {
        private readonly List<ValidatedAttack> attackActions = new List<ValidatedAttack>();

        public void RegisterAttack(ValidatedAttack attack) => attackActions.Add(attack);

        public void OnAttackReceived(Int16 id, UnitCell attacker, HexCell target, int damage)
        {
            var attackIndex = attackActions.FindIndex(a => a.ID == id);
            if (attackIndex < 0)
            {
                Debug.LogError($"Received invalid attack action with id {id}");
                return;
            }

            var attack = attackActions[attackIndex];
            attackActions.RemoveAt(attackIndex);
            
            attack.Validate(attacker, target, damage);
        }
    }
}