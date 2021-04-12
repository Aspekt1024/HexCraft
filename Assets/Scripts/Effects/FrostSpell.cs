using System;
using UnityEngine;

namespace Aspekt.Hex.Effects
{
    public class FrostSpell : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private ParticleSystem spell;
        [SerializeField] private ParticleSystem explosion;
#pragma warning restore 649

        private Rigidbody body;
        private Transform tf;

        private float hitTime;
        private Vector3 endPos;
        private Action onHitCallback;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            tf = transform;
        }

        private void Update()
        {
            if (Time.time >= hitTime)
            {
                var e = Instantiate(explosion, endPos, Quaternion.identity);
                Destroy(e.gameObject, 1f);
                Destroy(gameObject);
                onHitCallback?.Invoke();
            }
        }

        public void Cast(Vector3 startPos, Vector3 endPos, float speed, Action onHitCallback)
        {
            this.endPos = endPos;
            this.onHitCallback = onHitCallback;
            
            tf.position = startPos;
            var distance = endPos - startPos;
            tf.LookAt(endPos);
            body.velocity = distance.normalized * speed;
            hitTime = Time.time + distance.magnitude / speed;
        }

    }
}