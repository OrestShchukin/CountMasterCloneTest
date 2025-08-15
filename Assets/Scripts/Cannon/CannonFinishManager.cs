using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CannonFinishManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject cannon;
    [SerializeField] private Transform castlePartsParent;
    [SerializeField] private GameObject coin;
    
    [Header("Additional")]
    [SerializeField] private Explosion explosion;
    
    
    
    private List<Transform> castleParts = new ();
    private CannonShooting cannonShootingScript;
    private CannonMovement cannonMovementScript;
    private CoinManager coinManagerScript;

    
    private bool play = true;
    
    public int destroyedCounter = 0;

    void Start()
    {
        cannonShootingScript = cannon.GetComponent<CannonShooting>();
        cannonMovementScript = cannon.GetComponent<CannonMovement>();
        coinManagerScript = coin.GetComponent<CoinManager>();
        
        for (int i = 0; i < castlePartsParent.childCount; i++)
        {
            castleParts.Add(castlePartsParent.GetChild(i));
        }
    }

    private void Update()
    {
        if (!play) return;
        
        if (destroyedCounter >= 15)
        {
            play = false;
            StartCoroutine(switchToShootingTheCoin());
            if (UIManager.UIManagerInstance)
                UIManager.UIManagerInstance.OpenWinScreen();
            return;
        }
        for (int i = castleParts.Count - 1; i >= 0; i--)
        {
            if (castleParts[i].position.y < -10f)
            {
                destroyedCounter++;
                GameObject toDestroy = castleParts[i].gameObject;
                castleParts.RemoveAt(i);
                Destroy(toDestroy);
            }
        }
    }

    IEnumerator switchToShootingTheCoin()
    {
        cannonShootingScript.StopAutoFire();
        cannonMovementScript.AimAtCoin();
        yield return new WaitForSeconds(0.4f);
        explosion.Explode();
        yield return new WaitForSeconds(0.3f);
        ActivateCoin();
        yield return new WaitForSeconds(1f);
        cannonShootingScript.ShootTheCoin();
        coinManagerScript.duration = cannonShootingScript.interval;
        yield return null;
    }

    private void ActivateCoin()
    {
        coin.SetActive(true);
        Transform coinTransform = coin.transform;
        coinTransform.DOScale(new Vector3(2.31f, 2.31f, 2.31f), 2f).SetEase(Ease.InOutBounce);
    }
}
