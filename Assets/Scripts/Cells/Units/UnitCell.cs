using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using UnityEngine;

namespace Aspekt.Hex
{
    public abstract class UnitCell : HexCell
    {
#pragma warning disable 649
        [SerializeField] protected UpgradeStats[] upgradeStats;
#pragma warning disable 649
        
        protected Transform Model;

        public struct UnitStats
        {
            public int Attack;
            public int Defense;
            public int Speed;
            public int Shield;
            public int Range;
        }

        protected UnitStats Stats = new UnitStats();
        protected Animator Anim;
        
        [Serializable]
        public struct UpgradeStats
        {
            [Serializable]
            public struct Upgrade
            {
                public Technology tech;
                public int level;
                public int value;    
            }
            
            public TechGroups techGroup;
            public Upgrade[] upgrades;

            public Upgrade GetUpgradeForLevel(int level)
            {
                return upgrades.FirstOrDefault(u => u.level == level);
            }
            
            public bool IsTechGroup(Technology tech)
            {
                return upgrades.Any(u => u.tech == tech);
            }
        }

        public bool HasMoved { get; private set; }
        public bool HasAttacked { get; private set; }
        
        protected static readonly int AnimAttackTrigger = Animator.StringToHash("attack");
        protected static readonly int AnimCastTrigger = Animator.StringToHash("cast");
        protected static readonly int AnimRangedAttackTrigger = Animator.StringToHash("rangedAttack");
        
        private static readonly int AnimMoveSpeed = Animator.StringToHash("moveSpeed");
        private static readonly int AnimDamagedTrigger = Animator.StringToHash("damaged");
        private static readonly int AnimDeathTrigger = Animator.StringToHash("dead");

        private readonly List<IUnitActionObserver> unitActionObservers = new List<IUnitActionObserver>();

        public override string GetDisplayName(Technology techLevel) => DisplayName;

        private void Start()
        {
            Model.LookAt(transform.position + Vector3.back);
        }

        protected override void OnInit()
        {
            // Must be done here instead of Start
            Anim = GetComponentInChildren<Animator>();
        }

        public UnitStats GetStats() => Stats;

        public override void OnActionClicked(ActionDefinition actionDefinition)
        {
            if (actionDefinition is UnitAction unitAction)
            {
                EventObservers.ForEach(o => o.IndicateUnitAction(this, unitAction));
            }
        }

        public void RegisterActionObserver(IUnitActionObserver observer)
        {
            unitActionObservers.Add(observer);
        }

        public void UnregisterActionObserver(IUnitActionObserver observer)
        {
            unitActionObservers.Remove(observer);
        }
        
        /// <summary>
        /// Called at the start of a new turn
        /// </summary>
        public void TurnReset()
        {
            HasMoved = false;
            HasAttacked = false;
        }
        
        public override void MoveTo(HexCoordinates coords)
        {
            HasMoved = true;
            var path = CellData.GetPath(this, coords);
            Coordinates = coords;
            StartCoroutine(MoveRoutine(path));
        }

        public abstract void ShowAttack(HexCell target, Action attackHitCallback);

        public override void Remove()
        {
            // TODO if not already dead, show death routine
            unitActionObservers.ForEach(o => o.OnUnitRemoved(this));
            StartCoroutine(RemoveRoutine());
        }
        
        public override float ShowDamage(UnitCell attacker, int damage)
        {
            var newHealthPercent = base.ShowDamage(attacker, damage);
            if (newHealthPercent > 0)
            {
                Model.LookAt(attacker.transform);
                Anim.SetTrigger(AnimDamagedTrigger);
            }
            else
            {
                Anim.SetTrigger(AnimDeathTrigger);
            }

            return newHealthPercent;
        }

        public override float GetDamageMitigation()
        {
            return Stats.Shield / 100f;
        }
        
        protected UpgradeStats.Upgrade GetUpgradeForLevel(TechGroups group, int level)
        {
            var stats = upgradeStats.FirstOrDefault(s => s.techGroup == group);
            if (stats.upgrades.Length <= level) return new UpgradeStats.Upgrade();
            return stats.upgrades[level];
        }

        private IEnumerator MoveRoutine(List<Vector3> path)
        {
            var tf = transform;
            var pos = tf.position;

            var velocity = Vector3.zero;
            var target = path[path.Count - 1];
            
            for (int i = 0; i < path.Count; i++)
            {
                // TODO smooth rotation
                Model.LookAt(path[i]);

                while (Vector3.Distance(pos, path[i]) > 0.05f)
                {
                    velocity = GetMoveVelocity(velocity, pos, path[i], i == path.Count - 1);
                    Anim.SetFloat(AnimMoveSpeed, velocity.magnitude);
                    pos += Time.deltaTime * velocity; 
                    tf.position = pos;
                    yield return null;
                }
            }
            
            transform.position = target;
            Anim.SetFloat(AnimMoveSpeed, 0f);
            
            foreach (var observer in unitActionObservers)
            {
                observer.OnFinishedMove(this);
            }
        }

        private Vector3 GetMoveVelocity(Vector3 velocity, Vector3 currentPos, Vector3 targetPos, bool isLastPoint)
        {
            var currentSpeed = velocity.magnitude;
            float maxSpeed = 3f * Mathf.Sqrt(Stats.Speed);
            
            var distVector = (targetPos - currentPos).normalized;

            if (isLastPoint)
            {
                const float slowThreshold = 0.5f;
                var dist = Vector3.Distance(targetPos, currentPos);
                if (dist < slowThreshold)
                {
                    currentSpeed = Mathf.Lerp(maxSpeed, .3f, 1f - dist / slowThreshold);
                    return distVector * currentSpeed;
                }
            }
            
            const float acceleration = 3f;
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * acceleration);

            return distVector * currentSpeed;
        }

        public void AttackComplete()
        {
            HasAttacked = true;
            unitActionObservers.ForEach(o => o.OnFinishedAttack(this));
        }
        
        private IEnumerator RemoveRoutine()
        {
            yield return new WaitForSeconds(2f);
            
            const float duration = 2f;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                transform.position += Vector3.down * (Time.deltaTime * 0.3f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}