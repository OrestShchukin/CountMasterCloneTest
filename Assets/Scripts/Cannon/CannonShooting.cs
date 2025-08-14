using Unity.VisualScripting;
using UnityEngine;

public class CannonShooting : MonoBehaviour
{
    public GameObject characterBullet;
    public Transform barrel;

    public float force;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject bullet = Instantiate(characterBullet, barrel.position, barrel.rotation);
            bullet.GetComponent<Rigidbody>().linearVelocity = barrel.forward * (force);  
        }
    }
}
