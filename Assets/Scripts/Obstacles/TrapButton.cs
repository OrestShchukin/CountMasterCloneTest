using DG.Tweening;
using UnityEngine;
public enum TrapType { HorizontalTrap, SpikesTrap, Bridge }


public class TrapButton : MonoBehaviour
{
    [SerializeField]
    TrapType trapType;

    public void ActivateButton()
    {
        if (trapType == TrapType.HorizontalTrap)
        {
            HorizontalTrap horizontalTrap = transform.parent.parent.GetComponent<HorizontalTrap>();
            horizontalTrap.DeactivateTrap();
        }
        else if (trapType == TrapType.SpikesTrap)
        {
            SpikesTrap spikesTrap = transform.parent.parent.GetComponent<SpikesTrap>();
            spikesTrap.DeactivateTrap();
        }
        AnimateButton();
        if (trapType == TrapType.Bridge)
        {
            BridgeManager bridgeManager = transform.parent.parent.GetComponent<BridgeManager>();
            bridgeManager.ActivateBridge();
        }
    }

    void AnimateButton()
    {
        transform.DOLocalMoveY(-0.2f, 0.2f).SetEase(Ease.OutQuad);
    }
}
