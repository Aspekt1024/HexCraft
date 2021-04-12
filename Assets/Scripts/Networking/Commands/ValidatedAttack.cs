using System;
using UnityEngine;

namespace Aspekt.Hex.Commands
{
    public class ValidatedAttack
    {
        public Int16 ID { get; }

        private bool isValidated;
        private bool isAnimationComplete;
        private bool isActioned;
        private Action onCompletionCallback;

        private readonly UnitCell attacker;
        private readonly HexCell target;
        
        private int damage;
            
        public ValidatedAttack(Int16 id, UnitCell attacker, HexCell target, int damage)
        {
            ID = id;
            this.attacker = attacker;
            this.target = target;
            this.damage = damage;
        }
        
        public static int GetDamage(UnitCell attacker, HexCell target)
        {
            var attackerStats = attacker.GetStats();
            
            var damageMultiplier = Mathf.Max(1f - target.GetDamageMitigation(), 0f);
            return Mathf.RoundToInt(attackerStats.Attack * damageMultiplier);
        }

        public void OnAttackLanded()
        {
            isAnimationComplete = true;
            target.ShowDamage(attacker, damage);
            CompleteAttackIfSynced();
        }

        public void Validate(UnitCell attacker, HexCell target, int damage, Action onCompletionCallback)
        {
            if (this.attacker != attacker)
            {
                Debug.LogError("Invalid attacker!");
                return;
            }
            
            if (this.target != target)
            {
                Debug.LogError("Invalid target!");
                isActioned = true;
                attacker.AttackComplete();
                return;
            }

            if (this.damage != damage)
            {
                Debug.LogError($"Damage mismatch. Expected {this.damage} got {damage}");
                this.damage = damage;
            }

            this.damage = damage;
            this.onCompletionCallback = onCompletionCallback;
            
            isValidated = true;
            
            CompleteAttackIfSynced();
        }

        private void CompleteAttackIfSynced()
        {
            if (isActioned) return;
            
            if (isAnimationComplete && isValidated)
            {
                isActioned = true;
                target.RemoveHealth(damage);
                onCompletionCallback?.Invoke();
                attacker.AttackComplete();
            }
        }
    }
}