using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.SettingsPanelHUD.Controls
{
    public class PointerClickEventInterceptor : MonoBehaviour, IPointerClickHandler
    {
        public event Action<PointerEventData> PointerClicked;

        public void OnPointerClick(PointerEventData eventData) =>
            PointerClicked?.Invoke(eventData);
    }
}
