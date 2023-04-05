using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapZoom : MonoBehaviour
    {
        [SerializeField] private InputAction_Measurable mouseWheelAction;
        [SerializeField, Obsolete] private RectTransform containerRectTransform;
        [SerializeField, Obsolete] private AnimationCurve zoomCurve;

        [SerializeField] private ZoomInput zoomIn;
        [SerializeField] private ZoomInput zoomOut;

        [field: SerializeField] internal AnimationCurve normalizedZoomCurve { get; private set; }
        [field: SerializeField] internal Vector2Int zoomVerticalRange { get; private set; } = new (28, 50);
        [field: SerializeField] internal float scaleDuration { get; private set; } = 0.2f;

        internal ZoomInput ZoomIn => zoomIn;
        internal ZoomInput ZoomOut => zoomOut;
        internal InputAction_Measurable MouseWheelAction => mouseWheelAction;

        [Serializable]
        internal class ZoomInput
        {
            private static Color normalColor = new Color(0f, 0f, 0f, 1f);
            private static Color disabledColor = new Color(0f, 0f, 0f, 0.5f);

            public InputAction_Hold InputAction;
            public Button Button;

            [SerializeField] private Image Image;

            public void SetUiInteractable(bool isInteractable)
            {
                Button.interactable = isInteractable;
                Image.color = isInteractable ? normalColor : disabledColor;
            }
        }
    }
}
