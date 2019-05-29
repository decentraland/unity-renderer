using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIReferencesContainer : MonoBehaviour
    {
        [System.NonSerialized] public UIShape owner;

        [Header("Basic Fields")]
        [Tooltip("This needs to always have the root RectTransform.")]
        public RectTransform rectTransform;

        public CanvasGroup canvasGroup;

        public HorizontalLayoutGroup layoutGroup;
        public LayoutElement layoutElement;
        public RectTransform layoutElementRT;

        [Tooltip("Children of this UI object will reparent to this rectTransform.")]
        public RectTransform childHookRectTransform;

#if UNITY_EDITOR
        [Header("Debug")]
        public bool forceRefresh;

        public void LateUpdate()
        {
            if (forceRefresh)
            {
                owner.RefreshDCLLayoutRecursively();
                forceRefresh = false;
            }
        }
#endif
    }
}