using System.Collections.Generic;
using UnityEngine;

public class CastleDemolishionCounter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject cannon;
    [SerializeField] private Transform castelPartsParent;
    
    private List<Transform> castleParts = new ();
    private CannonShooting cannonShootingScript;

    private bool play = true;
    
    public int destroyedCounter = 0;

    void Start()
    {
        cannonShootingScript = cannon.GetComponent<CannonShooting>();
        for (int i = 0; i < castelPartsParent.childCount; i++)
        {
            castleParts.Add(castelPartsParent.GetChild(i));
        }
    }

    private void FixedUpdate()
    {
        if (!play) return;
        for (int i = 0; i < castleParts.Count; i++)
        {
            if (destroyedCounter >= 15)
            {
                play = false;
                cannonShootingScript.StopAutoFire();
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
