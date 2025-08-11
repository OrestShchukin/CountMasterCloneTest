using DG.Tweening;
using UnityEngine;

public class SpikesTrap : MonoBehaviour
{
    [SerializeField]
    Transform spikesParent;

    [SerializeField]
    float animationDuration = 0.5f;

    bool isActivated = false;


    public void DeactivateTrap()
    {
        if (isActivated) return;
        isActivated = true;

        int childCount = spikesParent.childCount;

        for (int i = 0; i < childCount; i++)
        {
            spikesParent.GetChild(i).GetChild(0).DOLocalMoveY(-1.55f, animationDuration);
        }
    }
}
