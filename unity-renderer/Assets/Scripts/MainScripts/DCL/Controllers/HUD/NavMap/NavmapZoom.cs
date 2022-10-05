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

        private readonly Color normalColor = new Color(0f, 0f, 0f, 1f);
        private readonly Color disabledColor = new Color(0f, 0f, 0f, 0.5f);
        
        [SerializeField] internal InputAction_Measurable mouseWheelAction;
        [SerializeField] internal RectTransform containerRectTransform;
        [SerializeField] internal AnimationCurve zoomCurve;
        
        [SerializeField] internal InputAction_Hold zoomIn;
        [SerializeField] internal Button zoomInButton;
        [SerializeField] internal Image zoomInPlus;
        
        [SerializeField] internal Button zoomOutButton;
        [SerializeField] internal InputAction_Hold zoomOut;
        [SerializeField] internal Image zoomOutMinus;
        
        private int currentZoomLevel;
        private Vector3 previousScaleSize;
        private bool isScaling;
        
        public float Scale { get; private set; } = 1;
        private BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
        
        public void ResetCameraZoom()
        {
            currentZoomLevel = Mathf.FloorToInt(MAP_ZOOM_LEVELS / 2);
            Scale = zoomCurve.Evaluate(currentZoomLevel);
            containerRectTransform.localScale = new Vector3(Scale, Scale, Scale);
            HandleZoomButtonsAspect();
        }
        
        private void Start()
        {
            ResetCameraZoom();
        }

        private void OnEnable()
        {
            zoomIn.OnStarted += OnZoomPlusMinus;
            zoomOut.OnStarted += OnZoomPlusMinus;
            mouseWheelAction.OnValueChanged += OnMouseWheelChangeValue;

            zoomInButton.onClick.AddListener(() => { OnZoomPlusMinus(DCLAction_Hold.ZoomIn); });
            zoomOutButton.onClick.AddListener(() => { OnZoomPlusMinus(DCLAction_Hold.ZoomOut); });
        }

        private void OnDisable()
        {
            zoomIn.OnStarted -= OnZoomPlusMinus;
            zoomOut.OnStarted -= OnZoomPlusMinus;
            mouseWheelAction.OnValueChanged -= OnMouseWheelChangeValue;
        }

        private void OnZoomPlusMinus(DCLAction_Hold action)
        {
            if (!navmapVisible.Get())
                return;

            if (action.Equals(DCLAction_Hold.ZoomIn))
            {
                CalculateZoomLevelAndDirection(1);
            }
            else if (action.Equals(DCLAction_Hold.ZoomOut))
            {
                CalculateZoomLevelAndDirection(-1);
            }

            EventSystem.current.SetSelectedGameObject(null);
        }

        private void CalculateZoomLevelAndDirection(float value)
        {
            if (!navmapVisible.Get())
                return;
            if (isScaling)
                return;
            previousScaleSize = new Vector3(Scale, Scale, Scale);
            if (value > 0 && currentZoomLevel < MAP_ZOOM_LEVELS)
            {
                currentZoomLevel++;
                StartCoroutine(ScaleOverTime(previousScaleSize));
            }
            if (value < 0 && currentZoomLevel >= 1)
            {
                currentZoomLevel--;
                StartCoroutine(ScaleOverTime(previousScaleSize));
            }
            HandleZoomButtonsAspect();
        }

        private void HandleZoomButtonsAspect()
        {
            SetZoomButtonInteractability(zoomInButton, zoomInPlus, canZoomMore: currentZoomLevel < MAP_ZOOM_LEVELS);
            SetZoomButtonInteractability(zoomOutButton, zoomOutMinus, canZoomMore: currentZoomLevel >= 1);
        }

        private void SetZoomButtonInteractability(Button button, Image buttonIcon, bool canZoomMore)
        {
            button.interactable = canZoomMore;
            buttonIcon.color = canZoomMore ? normalColor : disabledColor;
        }

        private IEnumerator ScaleOverTime(Vector3 startScaleSize)
        {
            isScaling = true;
            
            Scale = zoomCurve.Evaluate(currentZoomLevel);
            MapRenderer.i.scaleFactor = Scale;
            Vector3 targetScale = new Vector3(Scale, Scale, Scale);

            float counter = 0;
            while (counter < SCALE_DURATION)
            {
                counter += Time.deltaTime;
                containerRectTransform.localScale = Vector3.Lerp(startScaleSize, targetScale, counter / SCALE_DURATION);
                yield return null;
            }

            isScaling = false;
        }

        private void OnMouseWheelChangeValue(DCLAction_Measurable action, float value)
        {
            if (value > -MOUSE_WHEEL_THRESHOLD && value < MOUSE_WHEEL_THRESHOLD)
                return;
            
            CalculateZoomLevelAndDirection(value);
        }
    }
}