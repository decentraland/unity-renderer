using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapZoom : MonoBehaviour
    {
        private const float MAP_ZOOM_LEVELS = 4;
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

        public void ResetCameraZoom()
        {
            currentZoomLevel = Mathf.FloorToInt(MAP_ZOOM_LEVELS / 2);
            Scale = zoomCurve.Evaluate(currentZoomLevel);
            containerRectTransform.localScale = new Vector3(Scale, Scale, Scale);

            zoomIn.SetUiInteractable(isInteractable: currentZoomLevel < MAP_ZOOM_LEVELS);
            zoomOut.SetUiInteractable(isInteractable: currentZoomLevel >= 1);
        }

        private void Start() => ResetCameraZoom();

        private void OnEnable()
        {
            mouseWheelAction.OnValueChanged += OnMouseWheelValueChanged;

            zoomIn.InputAction.OnStarted += ZoomInRequest;
            zoomOut.InputAction.OnStarted += ZoomOutRequest;

            zoomIn.Button.onClick.AddListener( () => ZoomInRequest(DCLAction_Hold.ZoomIn));
            zoomOut.Button.onClick.AddListener( () => ZoomOutRequest(DCLAction_Hold.ZoomOut));
        }

        private void OnDisable()
        {
            mouseWheelAction.OnValueChanged -= OnMouseWheelValueChanged;

            zoomIn.InputAction.OnStarted -= ZoomInRequest;
            zoomOut.InputAction.OnStarted -= ZoomOutRequest;
        }

        enum ZoomDir
        {
            In,
            Out
        }

        private void OnMouseWheelValueChanged(DCLAction_Measurable action, float value)
        {
            if (Mathf.Abs(value) < MOUSE_WHEEL_THRESHOLD)
                return;

            if (value > 0)
                ZoomIn();
            else if (value < 0)
                ZoomOut();
        }

        private void ZoomInRequest(DCLAction_Hold _)
        {
            if (!navmapVisible.Get())
                return;

            EventSystem.current.SetSelectedGameObject(null);

            ZoomIn();
        }
        
        private void ZoomOutRequest(DCLAction_Hold _)
        {
            if (!navmapVisible.Get())
                return;

            EventSystem.current.SetSelectedGameObject(null);

            ZoomOut();
        }

        private void ZoomIn()
        {
            if (!navmapVisible.Get() || isScaling)
                return;

            if (currentZoomLevel < MAP_ZOOM_LEVELS)
            {
                currentZoomLevel++;
                StartCoroutine(ScaleOverTime(Scale));
            }

            zoomIn.SetUiInteractable(isInteractable: currentZoomLevel < MAP_ZOOM_LEVELS);
            zoomOut.SetUiInteractable(isInteractable: currentZoomLevel >= 1);
        }

        private void ZoomOut()
        {
            if (!navmapVisible.Get() || isScaling)
                return;

            if (currentZoomLevel >= 1)
            {
                currentZoomLevel--;
                StartCoroutine(ScaleOverTime(Scale));
            }

            zoomIn.SetUiInteractable(isInteractable: currentZoomLevel < MAP_ZOOM_LEVELS);
            zoomOut.SetUiInteractable(isInteractable: currentZoomLevel >= 1);
        }

        private IEnumerator ScaleOverTime(float startScale)
        {
            isScaling = true;

            Scale = zoomCurve.Evaluate(currentZoomLevel);
            MapRenderer.i.scaleFactor = Scale;
            
            Vector3 startScaleSize = new Vector3(startScale, startScale, startScale);
            Vector3 targetScale = new Vector3(Scale, Scale, Scale);
            
            for (float timer = 0; timer < SCALE_DURATION; timer += Time.deltaTime)
            {
                containerRectTransform.localScale = 
                    Vector3.Lerp(startScaleSize, targetScale, timer / SCALE_DURATION);
                
                yield return null;
            }
            
            isScaling = false;
        }

        [Serializable]
        private struct ZoomInput
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