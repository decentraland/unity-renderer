using UnityEngine;
using UnityEngine.EventSystems;

public class SizeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform targetRectTransform;

    public float hoverScale;

    public float normalScale;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetRectTransform != null)
            targetRectTransform.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetRectTransform != null)
            targetRectTransform.localScale = Vector3.one * normalScale;
    }
}
