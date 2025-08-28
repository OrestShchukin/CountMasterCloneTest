using UnityEngine;
using DG.Tweening;
public class HorizontalTrap : MonoBehaviour
{
    [SerializeField]
    Transform leftArm, rightArm;
    [SerializeField]
    float rotationDuration = 0.5f;
    

    bool isActivated = false;


    public void DeactivateTrap()
    {
        if (isActivated) return;
        isActivated = true;

        leftArm.DOLocalRotate(new Vector3(0, 0, -90), rotationDuration);
        // leftArm.DOLocalMoveX(Mathf.Abs(-0.5f), rotationDuration);
        rightArm.DOLocalRotate(new Vector3(0, 0, 90), rotationDuration);
        // leftArm.DOLocalMoveX(Mathf.Abs(0.2f), rotationDuration);
    }

}
