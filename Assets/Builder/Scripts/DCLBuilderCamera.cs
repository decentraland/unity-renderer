using UnityEngine;
using DCL.Controllers;
using DCL.Models;

namespace Builder
{
    public class DCLBuilderCamera : MonoBehaviour
    {
        [Header("References")]
        public Transform pitchPivot;
        public Transform yawPivot;
        public Transform rootPivot;
        public Camera builderCamera;

        [Space()]
        [Header("Input Settings")]
        public float rotationSpeed = 5f;
        [Header("Pitch")]
        public float pitchAmount = 10f;
        public float pitchAngleMin = 0f;
        public float pitchAngleMax = 89f;
        [Header("Yaw")]
        public float yawAmount = 10f;
        [Header("Pan")]
        public float panSpeed = 5f;
        public float panAmount = 0.2f;
        [Header("Zoom")]
        public float zoomMin = 1f;
        public float zoomMax = 20f;
        public float zoomSpeed = 5f;
        public float zoomAmount = 0.5f;

        private float pitchCurrent = 0;
        private float pitchTarget = 0;
        private float yawCurrent = 0;
        private float yawTarget = 0;
        private Vector3 panCurrent = Vector3.zero;
        private Vector3 panTarget = Vector3.zero;
        private float zoomCurrent = 0;
        private float zoomTarget = 0;

        private float pitchDefault = 0;
        private float yawDefault = 0;
        private float zoomDefault = 0;

        private bool isObjectBeingDrag = false;

        private Vector2 sceneMinSize;
        private Vector2 sceneMaxSize;

        private bool isGameObjectActive = false;

        private void Awake()
        {
            zoomDefault = zoomCurrent = zoomTarget = Mathf.Clamp(builderCamera.transform.position.z, -zoomMax, -zoomMin);
            pitchDefault = pitchCurrent = pitchTarget = pitchPivot.localEulerAngles.x;
            yawDefault = yawCurrent = yawTarget = yawPivot.localEulerAngles.y;

            SetSceneSize(GameObject.FindObjectOfType<ParcelScene>());
            CenterCamera();

            DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
        }

        private void OnDestroy()
        {
            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;

        }

        private void Update()
        {
            yawCurrent += (yawTarget - yawCurrent) * Time.deltaTime * rotationSpeed;
            yawPivot.localRotation = Quaternion.Euler(0, yawCurrent, 0);

            pitchCurrent += (pitchTarget - pitchCurrent) * Time.deltaTime * rotationSpeed;
            pitchPivot.localRotation = Quaternion.Euler(pitchCurrent, 0, 0);

            zoomCurrent += (zoomTarget - zoomCurrent) * Time.deltaTime * zoomSpeed;
            builderCamera.transform.localPosition = new Vector3(builderCamera.transform.localPosition.x, builderCamera.transform.localPosition.y, zoomCurrent);

            Vector3 panOffset = panTarget - panCurrent;
            float sqDist = panOffset.magnitude;
            if (sqDist >= 0.01f)
            {
                panCurrent = panCurrent + panOffset.normalized * sqDist * panSpeed * Time.deltaTime;
                rootPivot.localPosition = panCurrent;
            }
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderInput.OnMouseDrag += OnMouseDrag;
                DCLBuilderInput.OnMouseWheel += OnMouseWheel;
                DCLBuilderInput.OnKeyboardButtonHold += OnKeyboardButtonHold;
                DCLBuilderInput.OnKeyboardButtonDown += OnKeyboardButtonDown;
                DCLBuilderBridge.OnResetCamera += OnResetCamera;
                DCLBuilderBridge.OnZoomFromUI += OnZoomFormUI;
                DCLBuilderObjectSelector.OnDraggingObjectStart += OnDragObjectStart;
                DCLBuilderObjectSelector.OnDraggingObjectEnd += OnDragObjectEnd;
                DCLBuilderObjectSelector.OnGizmoTransformObjectStart += OnGizmoTransformObjectStart;
                DCLBuilderObjectSelector.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnd;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseWheel -= OnMouseWheel;
            DCLBuilderInput.OnKeyboardButtonHold -= OnKeyboardButtonHold;
            DCLBuilderInput.OnKeyboardButtonDown -= OnKeyboardButtonDown;
            DCLBuilderBridge.OnResetCamera -= OnResetCamera;
            DCLBuilderBridge.OnZoomFromUI -= OnZoomFormUI;
            DCLBuilderObjectSelector.OnDraggingObjectStart -= OnDragObjectStart;
            DCLBuilderObjectSelector.OnDraggingObjectEnd -= OnDragObjectEnd;
            DCLBuilderObjectSelector.OnGizmoTransformObjectStart -= OnGizmoTransformObjectStart;
            DCLBuilderObjectSelector.OnGizmoTransformObjectEnd -= OnGizmoTransformObjectEnd;
        }

        private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 0)
            {
                if (CanOrbit())
                {
                    yawTarget += axisX * yawAmount;
                    pitchTarget = Mathf.Clamp(pitchTarget - axisY * pitchAmount, pitchAngleMin, pitchAngleMax);
                }
            }
            else if (buttonId == 1)
            {
                Pan(0, -axisX * panAmount, -axisY * panAmount);
            }
        }

        private void OnMouseWheel(float axisValue)
        {
            Zoom(axisValue * zoomAmount);
        }

        private void OnKeyboardButtonHold(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.UpArrow:
                    Pan(panAmount, 0, 0);
                    break;
                case KeyCode.DownArrow:
                    Pan(-panAmount, 0, 0);
                    break;
                case KeyCode.LeftArrow:
                    Pan(0, -panAmount, 0);
                    break;
                case KeyCode.RightArrow:
                    Pan(0, panAmount, 0);
                    break;
            }
        }
        private void OnKeyboardButtonDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Space:
                    OnResetCamera();
                    break;
            }
        }

        private void OnResetCamera()
        {
            zoomCurrent = zoomTarget = zoomDefault;
            pitchCurrent = pitchTarget = pitchDefault;
            yawCurrent = yawTarget = yawDefault;

            //TODO: check if the number of parcels has changed
            CenterCamera();

            builderCamera.transform.position.Set(0, 0, zoomCurrent);
            pitchPivot.localRotation = Quaternion.Euler(yawCurrent, 0, 0);
            yawPivot.localRotation = Quaternion.Euler(0, yawCurrent, 0);
            rootPivot.localPosition = panCurrent;
        }

        private void OnZoomFormUI(float delta)
        {
            Zoom(-delta);
        }

        private void Pan(float forward, float right, float up)
        {
            Vector3 panOffset = ((yawPivot.right * right) + (pitchPivot.up * up) + (rootPivot.forward * forward));
            panTarget += panOffset;
        }

        private void Zoom(float amount)
        {
            zoomTarget = Mathf.Clamp(zoomTarget + amount, -zoomMax, -zoomMin);
        }

        private void CenterCamera()
        {
            panCurrent = new Vector3(
                sceneMinSize.x + ((sceneMaxSize.x + DCL.Configuration.ParcelSettings.PARCEL_SIZE) - sceneMinSize.x) * 0.5f,
                0,
                sceneMinSize.y + ((sceneMaxSize.y + DCL.Configuration.ParcelSettings.PARCEL_SIZE) - sceneMinSize.y) * 0.5f
            );

            rootPivot.transform.localPosition = panCurrent;
            panTarget = panCurrent;
        }

        private void OnDragObjectStart(DecentralandEntity entity, Vector3 objectPosition)
        {
            isObjectBeingDrag = true;
        }

        private void OnDragObjectEnd(DecentralandEntity entity, Vector3 objectPosition)
        {
            isObjectBeingDrag = false;
            pitchTarget = pitchCurrent;
            yawTarget = yawCurrent;
        }

        private void OnGizmoTransformObjectStart(DecentralandEntity entity, Vector3 objectPosition, string gizmoType)
        {
            OnDragObjectStart(entity, objectPosition);
        }

        private void OnGizmoTransformObjectEnd(DecentralandEntity entity, Vector3 objectPosition, string gizmoType)
        {
            OnDragObjectEnd(entity, objectPosition);
        }

        private bool CanOrbit()
        {
            return !isObjectBeingDrag;
        }

        private void SetSceneSize(ParcelScene scene)
        {
            sceneMinSize = Vector2.zero;
            sceneMaxSize = Vector2.zero;

            if (scene != null && scene.sceneData != null)
            {
                SetSceneSize(scene.sceneData);
            }
        }

        private void SetSceneSize(LoadParcelScenesMessage.UnityParcelScene scene)
        {
            sceneMinSize = Vector2.zero;
            sceneMaxSize = Vector2.zero;

            if (scene != null)
            {
                if (scene.parcels.Length > 0)
                {
                    foreach (Vector2Int parcel in scene.parcels)
                    {
                        Vector3 parcelPosition = new Vector3(parcel.x, 0, parcel.y);
                        sceneMinSize.x = Mathf.Min(sceneMinSize.x, parcelPosition.x);
                        sceneMinSize.y = Mathf.Min(sceneMinSize.y, parcelPosition.z);
                        sceneMaxSize.x = Mathf.Max(sceneMaxSize.x, parcelPosition.x);
                        sceneMaxSize.y = Mathf.Max(sceneMaxSize.y, parcelPosition.z);
                    }
                }
            }
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            gameObject.SetActive(!isPreview);
        }
    }
}