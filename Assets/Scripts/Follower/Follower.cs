using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Follower : MonoBehaviour
{
    public Transform groupCenter;
    public float attractionForce = 10f;
    public float maxSpeed = 1f;
    public float distanceFromCenter = 0.5f;
    public float cohesionStrength = 2f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
