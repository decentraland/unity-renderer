using DCL.Helpers;
using System;
using TMPro;
using UnityEngine.EventSystems;

namespace UIComponents.Scripts.Utils
{
    public static class TextMeshProExtensions
    {
        public static void SubscribeToClickEvents(this TMP_Text text, Action<PointerEventData> callback)
        {
            PointerClickHandler handler = text.gameObject.GetOrCreateComponent<PointerClickHandler>();
            handler.OnClick += callback;
        }

        public static void UnsubscribeToClickEvents(this TMP_Text text, Action<PointerEventData> callback)
        {
            PointerClickHandler handler = text.gameObject.GetOrCreateComponent<PointerClickHandler>();
            handler.OnClick -= callback;
        }
    }
}
