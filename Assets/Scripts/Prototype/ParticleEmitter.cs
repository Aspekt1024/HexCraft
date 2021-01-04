using UnityEngine;

namespace Aspekt.Prototype
{
    public class ParticleEmitter : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private ParticleSystem projectile;
        [SerializeField] private float speed = 15f;
#pragma warning restore 649

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var newProj = Instantiate(projectile);
                newProj.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
                var body = newProj.GetComponent<Rigidbody>();
                body.velocity = Vector3.right * speed;
                
                Destroy(newProj, 5f);
            }
        }
    }
}