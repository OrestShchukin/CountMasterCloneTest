using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CannonShooting : MonoBehaviour
{
    [Header("Cannon preferences")] public GameObject characterBullet;
    public Transform barrel;
    public float force;
    private List<GameObject> usedBullets = new();


    public int currentAmmo = 50;
    [SerializeField] int ammoPerShot = 1;
    [SerializeField] float interval = 0.15f;
    

    void Start()
    {
        StartAutoFire();
    }

    public bool Fire()
    {
        if (currentAmmo < ammoPerShot) return false;
        SpawnBullet();
        currentAmmo -= ammoPerShot;
        if (currentAmmo < ammoPerShot) CancelInvoke(nameof(TickFire));
        return true;
    }

    void TickFire()
    {
        if (!Fire()) CancelInvoke(nameof(TickFire));
    }

    public void StartAutoFire()
    {
        CancelInvoke(nameof(TickFire)); // на всяк випадок
        InvokeRepeating(nameof(TickFire), 0f, interval);
    }

    public void StopAutoFire()
    {
        CancelInvoke(nameof(TickFire));
    }

    private void SpawnBullet()
    {
        GameObject bullet = Instantiate(characterBullet, barrel.position, barrel.rotation);
        bullet.transform.GetChild(0).GetComponent<Rigidbody>().linearVelocity = barrel.forward * (force);
        usedBullets.Add(bullet);

        if (usedBullets.Count > 10)
        {
            GameObject bulletToDestroy = usedBullets[0];
            usedBullets.RemoveAt(0);
            Destroy(bulletToDestroy);
        }
    }
}