using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float explosionForce, radius;
    
    
    public void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, radius);
            }
        }
        Destroy(gameObject);
    }
}
