using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapZoom : MonoBehaviour
    {
        private const int MAX_ZOOM = 4;
        private const int MIN_ZOOM = 1;
        private const float MOUSE_WHEEL_THRESHOLD = 0.04f;
        private const float SCALE_DURATION = 0.2f;

        [SerializeField] private InputAction_Measurable mouseWheelAction;
        [SerializeField] private RectTransform containerRectTransform;
        [SerializeField] private AnimationCurve zoomCurve;

        [SerializeField] private ZoomInput zoomIn;
        [SerializeField] private ZoomInput zoomOut;

        private int currentZoomLevel;
        private bool isScaling;

        public float Scale { get; private set; } = 1;
        private BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;

        public void ResetToDefault()
        {
            currentZoomLevel = MAX_ZOOM / 2;
            Scale = zoomCurve.Evaluate(currentZoomLevel);
            containerRectTransform.localScale = new Vector3(Scale, Scale, Scale);

            SetUiButtonsInteractability();
        }
        
        private void SetUiButtonsInteractability()
        {
            zoomIn.SetUiInteractable(isInteractable: currentZoomLevel < MAX_ZOOM);
            zoomOut.SetUiInteractable(isInteractable: currentZoomLevel > MIN_ZOOM);
        }
        
        private void Start() => ResetToDefault();

        private void OnEnable()
        {
            mouseWheelAction.OnValueChanged += OnMouseWheelValueChanged;

            zoomIn.InputAction.OnStarted += Zoom;
            zoomOut.InputAction.OnStarted += Zoom;

            zoomIn.Button.onClick.AddListener( () => Zoom(DCLAction_Hold.ZoomIn));
            zoomOut.Button.onClick.AddListener( () => Zoom(DCLAction_Hold.ZoomOut));
        }

        private void OnDisable()
        {
            mouseWheelAction.OnValueChanged -= OnMouseWheelValueChanged;

            zoomIn.InputAction.OnStarted -= Zoom;
            zoomOut.InputAction.OnStarted -= Zoom;
        }

        private void OnMouseWheelValueChanged(DCLAction_Measurable action, float value)
        {
            if (value == 0 || Mathf.Abs(value) < MOUSE_WHEEL_THRESHOLD)
                return;

            var zoomAction = value > 0 ? DCLAction_Hold.ZoomIn : DCLAction_Hold.ZoomOut;

            Zoom(zoomAction);
        }

        private void Zoom(DCLAction_Hold action)
        {
            if (!navmapVisible.Get() || isScaling)
                return;
            
            switch (action)
            {
                case DCLAction_Hold.ZoomIn when currentZoomLevel == MAX_ZOOM:
                case DCLAction_Hold.ZoomOut when currentZoomLevel == MIN_ZOOM:
                    return;
            }

            EventSystem.current.SetSelectedGameObject(null);

            float startScale = Scale;
            UpdateScale(zoomDirection: action == DCLAction_Hold.ZoomIn ? 1 : -1);

            StopAllCoroutines();
            StartCoroutine(ScaleOverTime(from: startScale, to: Scale));

            SetUiButtonsInteractability();
        }
        
        private void UpdateScale(int zoomDirection)
        {
            currentZoomLevel += zoomDirection;
            Scale = zoomCurve.Evaluate(currentZoomLevel);
            MapRenderer.i.scaleFactor = Scale;
        }

        private IEnumerator ScaleOverTime(float from, float to)
        {
            isScaling = true;

            Vector3 startScale = new Vector3(from, from, from);
            Vector3 targetScale = new Vector3(to, to, to);

            for (float timer = 0; timer < SCALE_DURATION; timer += Time.deltaTime)
            {
                containerRectTransform.localScale =
                    Vector3.Lerp(startScale, targetScale, timer / SCALE_DURATION);

                yield return null;
            }

            isScaling = false;
        }

        [Serializable]
        private class ZoomInput
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