using System;
using TMPro;
using UnityEngine;
using UnityEngine.AdaptivePerformance.VisualScripting;

public class CannonShooting : MonoBehaviour
{
    [Header("Cannon preferences")] 
    public GameObject characterBullet;
    public Transform barrel;
    public float force;
    
    [Header("UI Counter")]
    [SerializeField] TextMeshProUGUI ammoCounter;

    [Header("Scripts")]
    [SerializeField] CannonFinishManager cannonFinishManager;
    
    public int currentAmmo = 0;
    public  int ammoPerShot = 1;
    public float interval = 0.15f;

    private bool castleDestroyed = false;
    private bool shootingTheCoinLastShot = false;

    private CannonMovement cannonMovementScript;


    void Start()
    {
        cannonMovementScript = GetComponent<CannonMovement>();
    }
    public void StartShooting() // Use after the cannon was fully loaded with stickmans
    {
        StartAutoFire();
        UpdateAmmoCounter();
    }

    public bool Fire()
    {
        if (currentAmmo < ammoPerShot)
        {
            if (shootingTheCoinLastShot)
            {
                currentAmmo = ammoPerShot;
                shootingTheCoinLastShot = false;
            }
            else
            {
                return false;
            }
        }
            
        SpawnBullet();
        currentAmmo -= ammoPerShot;
        UpdateAmmoCounter();
        if (currentAmmo < ammoPerShot)
        {
            CancelInvoke(nameof(TickFire));
            if (castleDestroyed)
            {
                // Open Win Screen for castle Destroyed (Modify later)
                cannonFinishManager.OpenWinScreen();
            }
            else
            {
                // Open Win Screen for castle not Destroyed (Modify Later)
                cannonFinishManager.OpenWinScreen();
            }
        }
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
        castleDestroyed = true;
        shootingTheCoinLastShot = true;
        interval *= 0.8f;
        ammoPerShot = 3;
        cannonMovementScript.AimAtCoin();
        Invoke(nameof(StartAutoFire), 2f);
    }

    private void UpdateAmmoCounter()
    {
        ammoCounter.text = currentAmmo.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("blue"))
        {
            currentAmmo++;
            UpdateAmmoCounter();
            other.gameObject.SetActive(false);
        }
    }
}