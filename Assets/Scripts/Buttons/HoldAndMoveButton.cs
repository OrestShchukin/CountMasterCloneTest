using UnityEngine;
using UnityEngine.EventSystems;

public class HoldAndMoveButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        UIManager.UIManagerInstance.StartGame();
    }
}
