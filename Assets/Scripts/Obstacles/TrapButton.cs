using DG.Tweening;
using UnityEngine;
public enum TrapType { HorizontalTrap, SpikesTrap, Bridge }


public class TrapButton : MonoBehaviour
{
    [SerializeField]
    private TrapType trapType;
    [SerializeField] 
    private GameObject scriptParent;

    public void ActivateButton()
    {
        if (trapType == TrapType.HorizontalTrap)
        {
            HorizontalTrap horizontalTrap = scriptParent.GetComponent<HorizontalTrap>();
            horizontalTrap.DeactivateTrap();
        }
        else if (trapType == TrapType.SpikesTrap)
        {
            SpikesTrap spikesTrap = scriptParent.GetComponent<SpikesTrap>();
            spikesTrap.DeactivateTrap();
        }
        AnimateButton();
        if (trapType == TrapType.Bridge)
        {
            BridgeManager bridgeManager = scriptParent.GetComponent<BridgeManager>();
            bridgeManager.ActivateBridge();
        }
    }

    void AnimateButton()
    {
        transform.DOLocalMoveY(-0.2f, 0.2f).SetEase(Ease.OutQuad);
    }
}
