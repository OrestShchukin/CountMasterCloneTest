using DG.Tweening;
using UnityEngine;

public class BridgeManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Bridge Settings")]
    [SerializeField] Transform firstPivot;
    [SerializeField] Transform secondPivot;
    [SerializeField] GameObject fallZone;
    [SerializeField] float duration = 0.5f;
    

    public void ActivateBridge()
    {
        firstPivot.DORotate(new Vector3(0, 0, 0), duration);
        secondPivot.DORotate(new Vector3(0, 180, 0), duration).OnComplete(DisableFallZone);

        void DisableFallZone()
        {
            fallZone.gameObject.SetActive(false);
        }
    }
    
}
