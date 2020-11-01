using System;
using System.Collections;
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
        
        [Header("Unit Settings")]
        public int MoveRange;
        public int AttackRange;
        public int AttackDamage;
        
        public override List<CellUIItem.Details> ItemDetails { get; protected set; }

        private Animator Anim;
        
        private static readonly int AnimMoveSpeed = Animator.StringToHash("moveSpeed");
        private static readonly int AnimAttackTrigger = Animator.StringToHash("attack");
        private static readonly int AnimDamagedTrigger = Animator.StringToHash("damaged");
        private static readonly int AnimDeathTrigger = Animator.StringToHash("dead");

        private void Start()
        {
            transform.LookAt(transform.position + Vector3.back);
        }

        protected override void OnInit()
        {
            SetupActionItems();
            Anim = GetComponentInChildren<Animator>();
        }

        public override bool CanCreate(Cells.CellTypes cellType) => false;
        
        protected virtual void SetupActionItems()
        {
            ItemDetails = new List<CellUIItem.Details>
            {
                new CellUIItem.Details(attackImage, ActionAttack, 0),
                new CellUIItem.Details(moveImage, ActionMove, 0),
            };
        }
        
        public override void MoveTo(HexCoordinates coords)
        {
            var path = CellData.GetPath(this, coords);
            Coordinates = coords;
            StartCoroutine(MoveRoutine(path));
        }
        
        public void ShowAttack(HexCell target, int damage)
        {
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
                transform.LookAt(attacker.transform);
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
                transform.LookAt(path[i]);

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
            transform.LookAt(target.transform);
            Anim.SetTrigger(AnimAttackTrigger);
            yield return new WaitForSeconds(0.3f);
            target.TakeDamage(this, damage);
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

        private void ActionAttack()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateUnitAttack(this);
            }
        }

        private void ActionMove()
        {
            foreach (var observer in EventObservers)
            {
                observer.IndicateUnitMove(this);
            }
        }
        
    }
}