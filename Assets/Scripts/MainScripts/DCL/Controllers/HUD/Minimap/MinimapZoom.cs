using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapZoom : MonoBehaviour, IScrollHandler
{
    public float sensitivity = 1f;

    public event System.Action<float> OnZoom;

    public void OnScroll(PointerEventData eventData)
    {
        OnZoom?.Invoke(eventData.scrollDelta.y * sensitivity * Time.deltaTime);
    }
}