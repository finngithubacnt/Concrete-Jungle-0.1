using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickDebugger : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"★ CLICK detected on {gameObject.name} at {eventData.position}");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"★ POINTER DOWN on {gameObject.name} at {eventData.position}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"★ POINTER UP on {gameObject.name} at {eventData.position}");
    }
}
