using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class UIReferencesContainer : MonoBehaviour
    {
        [System.NonSerialized] public UIShape owner;

        [Header("Basic Fields")]
        [Tooltip("This needs to always have the root RectTransform.")]
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public HorizontalLayoutGroup alignmentLayoutGroup;
        [Tooltip("Children of this UI object will reparent to this rectTransform.")]
        public RectTransform childHookRectTransform;
    }
}
