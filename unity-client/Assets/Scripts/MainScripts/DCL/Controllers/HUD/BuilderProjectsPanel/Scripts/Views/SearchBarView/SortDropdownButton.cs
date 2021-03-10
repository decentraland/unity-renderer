using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

internal class SortDropdownButton : MonoBehaviour, IPointerDownHandler
{
    public event Action<string> OnSelected;

    [SerializeField] internal TextMeshProUGUI label;

    public void SetText(string text)
    {
        label.text = text;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        OnSelected?.Invoke(label.text);
    }
}
