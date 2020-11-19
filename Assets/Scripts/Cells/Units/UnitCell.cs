using System.Collections;
using System.Collections.Generic;
using Aspekt.Hex.Actions;
using UnityEngine;

namespace Aspekt.Hex
{
    public abstract class UnitCell : HexCell
    {
        protected Transform Model;
        
        [Header("Unit Settings")]
        public int MoveRange;
        public int AttackRange;
        public int AttackDamage;

        public struct UnitStats
        {
            public int Attack;
            public int Defense;
            public int Speed;
            public int Shield;
        }

        protected UnitStats Stats;

        public bool HasMoved { get; private set; }
        public bool HasAttacked { get; private set; }
        
        private static readonly int AnimMoveSpeed = Animator.StringToHash("moveSpeed");
        private static readonly int AnimAttackTrigger = Animator.StringToHash("attack");
        private static readonly int AnimDamagedTrigger = Animator.StringToHash("damaged");
        private static readonly int AnimDeathTrigger = Animator.StringToHash("dead");

        private readonly List<IUnitActionObserver> unitActionObservers = new List<IUnitActionObserver>();
        
        private void Start()
        {
            Model.LookAt(transform.position + Vector3.back);
            Stats = new UnitStats();
        }
        
        protected Animator Anim;

        protected override void OnInit()
        {
            // Must be done here instead of Start
            Anim = GetComponentInChildren<Animator>();
        }

        public UnitStats GetStats() => Stats;

        public override bool CanCreate(Cells.CellTypes cellType) => false;

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
        
        public void ShowAttack(HexCell target, int damage)
        {
            HasAttacked = true;
            StartCoroutine(AttackRoutine(target, damage));
        }

        public override void Remove()
        {
            StartCoroutine(DeathRoutine());
        }
        
        public override void TakeDamage(UnitCell attacker, int damage)
        {
            base.TakeDamage(attacker, damage);
            
            if (CurrentHP > 0)
            {
                Model.LookAt(attacker.transform);
                Anim.SetTrigger(AnimDamagedTrigger);
            }
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
            
            foreach (var observer in unitActionObservers)
            {
                observer.OnFinishedMove(this);
            }
        }

        private Vector3 GetMoveVelocity(Vector3 velocity, Vector3 currentPos, Vector3 targetPos, bool isLastPoint)
        {
            var currentSpeed = velocity.magnitude;
            float maxSpeed = 3f * Mathf.Sqrt(MoveRange);
            
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

        private IEnumerator AttackRoutine(HexCell target, int damage)
        {
            Model.LookAt(target.transform);
            Anim.SetTrigger(AnimAttackTrigger);
            yield return new WaitForSeconds(0.3f);
            target.TakeDamage(this, damage);

            foreach (var observer in unitActionObservers)
            {
                observer.OnFinishedAttack(this);
            }
        }
        
        private IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(0.3f);
            Anim.SetTrigger(AnimDeathTrigger);
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