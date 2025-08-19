using DG.Tweening;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager coinManagerInstance;
    public float duration = 0.2f;

    void Awake()
    {
        coinManagerInstance = this;
    }

    public void Pump()
    {
        transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), duration, 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishCannonBullet"))
        {
            Pump();
            Destroy(other.gameObject);
        }
    }
}
