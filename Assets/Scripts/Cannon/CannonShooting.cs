using UnityEngine;

public class CannonShooting : MonoBehaviour
{
    [Header("Cannon preferences")] public GameObject characterBullet;
    public Transform barrel;
    public float force;


    public int currentAmmo = 50;
    [SerializeField] int ammoPerShot = 1;
    [SerializeField] float interval = 0.15f;


    private CannonMovement cannonMovementScript;

    void Start()
    {
        StartAutoFire();
        cannonMovementScript = GetComponent<CannonMovement>();
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
        Destroy(bullet, 4f);
    }
    
    
    public void ShootTheCoin()
    {
        cannonMovementScript.AimAtCoin();
        Invoke(nameof(StartAutoFire), 2f);
    }
}