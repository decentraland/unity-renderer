using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapZoomView : MonoBehaviour
    {
        [SerializeField] private ZoomInput zoomIn;
        [SerializeField] private ZoomInput zoomOut;
        [SerializeField] private InputAction_Measurable mouseWheelAction;

        [field: SerializeField] internal AnimationCurve normalizedZoomCurve { get; private set; }
        [field: SerializeField] internal Vector2Int zoomVerticalRange { get; private set; } = new (28, 50);
        [field: SerializeField] internal float scaleDuration { get; private set; } = 0.2f;

        internal ZoomInput ZoomIn => zoomIn;
        internal ZoomInput ZoomOut => zoomOut;
        internal InputAction_Measurable MouseWheelAction => mouseWheelAction;

        private void Awake()
        {
            if (DataStore.i.featureFlags.flags.Get().IsInitialized)
            {
                if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("map_focus_home_or_user"))
                    zoomVerticalRange = new Vector2Int(zoomVerticalRange.x, 165);
            }
            else
                DataStore.i.featureFlags.flags.OnChange += OnFeatureFlagsChanged;
        }

        private void OnFeatureFlagsChanged(FeatureFlag current, FeatureFlag previous)
        {
            DataStore.i.featureFlags.flags.OnChange -= OnFeatureFlagsChanged;
            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("map_focus_home_or_user"))
                zoomVerticalRange = new Vector2Int(zoomVerticalRange.x, 165);
        }

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
