using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SizeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform targetRectTransform;

    public float hoverScale;

    public float normalScale;

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetRectTransform.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetRectTransform.localScale = Vector3.one * normalScale;
    }
}