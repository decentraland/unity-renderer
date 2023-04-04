using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIComponents.Scripts.Utils
{
    public class PointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public event Action<PointerEventData> OnClick;

        public void OnPointerClick(PointerEventData eventData) =>
            OnClick?.Invoke(eventData);
    }
}
