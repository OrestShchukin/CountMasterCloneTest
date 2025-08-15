using System.Collections.Generic;
using UnityEngine;

public class CastleDemolishionCounter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject cannon;
    [SerializeField] private Transform castlePartsParent;
    
    [Header("Additional")]
    [SerializeField] private Explosion explosion;
    
    
    
    private List<Transform> castleParts = new ();
    private CannonShooting cannonShootingScript;

    
    private bool play = true;
    
    public int destroyedCounter = 0;

    void Start()
    {
        cannonShootingScript = cannon.GetComponent<CannonShooting>();
        for (int i = 0; i < castlePartsParent.childCount; i++)
        {
            castleParts.Add(castlePartsParent.GetChild(i));
        }
    }

    private void Update()
    {
        if (!play) return;
        for (int i = 0; i < castleParts.Count; i++)
        {
            if (destroyedCounter >= 15)
            {
                play = false;
                cannonShootingScript.StopAutoFire();
                explosion.Explode();
                cannonShootingScript.ShootTheCoin();
                if (UIManager.UIManagerInstance)
                    UIManager.UIManagerInstance.OpenWinScreen();
                this.gameObject.SetActive(false);
            }
            
            if (castleParts[i].position.y < -10f)
            {
                destroyedCounter++;
                GameObject toDestroy = castleParts[i].gameObject;
                castleParts.Remove(castleParts[i]);
                Destroy(toDestroy);
            }
        }
    }
}
