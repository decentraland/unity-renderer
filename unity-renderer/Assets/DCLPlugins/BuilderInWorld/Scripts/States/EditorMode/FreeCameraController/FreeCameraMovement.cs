using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Configuration;
using DCL.SettingsCommon;
using UnityEngine;

namespace DCL.Camera
{
    public class FreeCameraMovement : CameraStateBase, IFreeCameraMovement
    {
        private const float CAMERA_ANGLE_THRESHOLD = 0.01f;
        private const float CAMERA_PAN_THRESHOLD = 0.001f;
        private const float CAMERA_MOVEMENT_THRESHOLD = 0.001f;

        private const float CAMERA_MOVEMENT_DEACTIVATE_INPUT_MAGNITUD = 0.001f;

        public float focusDistance = 5f;

        [Header("Camera Movement")]
        public float xPlaneSpeedPercentCompensantion = 0.85f;

        public float movementSpeed = 5f;
        public float lerpTime = 0.3F;

        public float lerpDeccelerationTime = 0.3F;
        public float initalAcceleration = 2f;
        public float speedClamp = 1f;

        [Header("Camera Look")]
        public float smoothLookAtSpeed = 5f;

        public float smoothCameraLookSpeed = 5f;
        public float lookSpeedH = 2f;
        public float lookSpeedV = 2f;

        [Header("Camera Pan")]
        public float smoothCameraPanAceleration = 5f;

        public float dragSpeed = 3f;

        [Header("Camera Zoom")]
        public float zoomSpeed = 2f;

        [Header("InputActions")]
        [SerializeField] internal InputAction_Hold advanceFowardInputAction;

        [SerializeField] internal InputAction_Hold advanceBackInputAction;
        [SerializeField] internal InputAction_Hold advanceLeftInputAction;
        [SerializeField] internal InputAction_Hold advanceRightInputAction;
        [SerializeField] internal InputAction_Hold advanceUpInputAction;
        [SerializeField] internal InputAction_Hold advanceDownInputAction;
        [SerializeField] internal InputAction_Hold cameraPanInputAction;
        [SerializeField] internal InputAction_Trigger zoomInFromKeyboardInputAction;
        [SerializeField] internal InputAction_Trigger zoomOutFromKeyboardInputAction;

        internal Vector3 direction = Vector3.zero;

        internal float yaw = 0f;
        internal float pitch = 0f;

        internal float panAxisX = 0f;
        internal float panAxisY = 0f;

        internal bool isCameraAbleToMove = true;

        internal bool isAdvancingForward = false;
        internal bool isAdvancingBackward = false;
        internal bool isAdvancingLeft = false;
        internal bool isAdvancingRight = false;
        internal bool isAdvancingUp = false;
        internal bool isAdvancingDown = false;

        internal bool isDetectingMovement = false;
        internal bool hasBeenMovement = false;

        internal bool isPanCameraActive = false;
        internal bool isMouseRightClickDown = false;

        internal Coroutine smoothLookAtCoroutine;
        internal Coroutine smoothFocusOnTargetCoroutine;
        internal Coroutine smoothScrollCoroutine;

        private InputAction_Hold.Started advanceForwardStartDelegate;
        private InputAction_Hold.Finished advanceForwardFinishedDelegate;

        private InputAction_Hold.Started advanceBackStartDelegate;
        private InputAction_Hold.Finished advanceBackFinishedDelegate;

        private InputAction_Hold.Started advanceLeftStartDelegate;
        private InputAction_Hold.Finished advanceLeftFinishedDelegate;

        private InputAction_Hold.Started advanceRightStartDelegate;
        private InputAction_Hold.Finished advanceRightFinishedDelegate;

        private InputAction_Hold.Started advanceDownStartDelegate;
        private InputAction_Hold.Finished advanceDownFinishedDelegate;

        private InputAction_Hold.Started advanceUpStartDelegate;
        private InputAction_Hold.Finished advanceUpFinishedDelegate;

        private InputAction_Hold.Started cameraPanStartDelegate;
        private InputAction_Hold.Finished cameraPanFinishedDelegate;

        private InputAction_Trigger.Triggered zoomInFromKeyboardDelegate;
        private InputAction_Trigger.Triggered zoomOutFromKeyboardDelegate;

        private Vector3 nextTranslation;
        internal Vector3 originalCameraPosition;
        internal Vector3 originalCameraPointToLookAt;
        private Vector3 cameraVelocity = Vector3.zero;

        private float lastMouseWheelTime;
        private float cameraPanAdvance;
        private float cameraLookAdvance;
        private bool isCameraDragging = false;

        public bool invertMouseY = false;

        private void Awake()
        {
            BIWInputWrapper.OnMouseDrag += MouseDrag;
            BIWInputWrapper.OnMouseDragRaw += MouseDragRaw;
            BIWInputWrapper.OnMouseWheel += MouseWheel;

            BIWInputWrapper.OnMouseDown += OnInputMouseDown;
            BIWInputWrapper.OnMouseUp += OnInputMouseUp;

            advanceForwardStartDelegate = (action) => isAdvancingForward = true;
            advanceForwardFinishedDelegate = (action) => isAdvancingForward = false;

            if (advanceFowardInputAction != null)
            {
                advanceFowardInputAction.OnStarted += advanceForwardStartDelegate;
                advanceFowardInputAction.OnFinished += advanceForwardFinishedDelegate;
            }

            advanceBackStartDelegate = (action) => isAdvancingBackward = true;
            advanceBackFinishedDelegate = (action) => isAdvancingBackward = false;

            if (advanceBackInputAction != null)
            {
                advanceBackInputAction.OnStarted += advanceBackStartDelegate;
                advanceBackInputAction.OnFinished += advanceBackFinishedDelegate;
            }

            advanceLeftStartDelegate = (action) => isAdvancingLeft = true;
            advanceLeftFinishedDelegate = (action) => isAdvancingLeft = false;

            if (advanceLeftInputAction != null)
            {
                advanceLeftInputAction.OnStarted += advanceLeftStartDelegate;
                advanceLeftInputAction.OnFinished += advanceLeftFinishedDelegate;
            }

            advanceRightStartDelegate = (action) => isAdvancingRight = true;
            advanceRightFinishedDelegate = (action) => isAdvancingRight = false;

            if (advanceRightInputAction != null)
            {
                advanceRightInputAction.OnStarted += advanceRightStartDelegate;
                advanceRightInputAction.OnFinished += advanceRightFinishedDelegate;
            }

            advanceUpStartDelegate = (action) => isAdvancingUp = true;
            advanceUpFinishedDelegate = (action) => isAdvancingUp = false;

            if (advanceUpInputAction != null)
            {
                advanceUpInputAction.OnStarted += advanceUpStartDelegate;
                advanceUpInputAction.OnFinished += advanceUpFinishedDelegate;
            }

            advanceDownStartDelegate = (action) => isAdvancingDown = true;
            advanceDownFinishedDelegate = (action) => isAdvancingDown = false;

            if (advanceDownInputAction != null)
            {
                advanceDownInputAction.OnStarted += advanceDownStartDelegate;
                advanceDownInputAction.OnFinished += advanceDownFinishedDelegate;
            }

            cameraPanStartDelegate = (action) => isPanCameraActive = true;
            cameraPanFinishedDelegate = (action) =>
            {
                isPanCameraActive = false;
                isCameraDragging = false;
            };

            if (cameraPanInputAction != null)
            {
                cameraPanInputAction.OnStarted += cameraPanStartDelegate;
                cameraPanInputAction.OnFinished += cameraPanFinishedDelegate;
            }

            zoomInFromKeyboardDelegate = (action) => MouseWheel(1f);

            if (zoomInFromKeyboardInputAction != null)
                zoomInFromKeyboardInputAction.OnTriggered += zoomInFromKeyboardDelegate;

            zoomOutFromKeyboardDelegate = (action) => MouseWheel(-1f);

            if (zoomOutFromKeyboardInputAction != null)
                zoomOutFromKeyboardInputAction.OnTriggered += zoomOutFromKeyboardDelegate;

            invertMouseY = Settings.i.generalSettings.Data.invertYAxis;

            Settings.i.generalSettings.OnChanged += UpdateLocalGeneralSettings;
        }

        private void UpdateLocalGeneralSettings(GeneralSettings obj)
        {
            invertMouseY = obj.invertYAxis;
        }

        public void StartDetectingMovement()
        {
            isDetectingMovement = true;
            hasBeenMovement = false;
        }

        public bool HasBeenMovement() { return hasBeenMovement; }

        public void StopDetectingMovement() { isDetectingMovement = false; }

        internal void OnInputMouseUp(int buttonId, Vector3 mousePosition)
        {
            if (buttonId == 1)
                isMouseRightClickDown = false;
            else if (buttonId == 2)
                isCameraDragging = false;
        }

        internal void OnInputMouseDown(int buttonId, Vector3 mousePosition)
        {
            if (buttonId != 1)
                return;

            isMouseRightClickDown = true;
            direction = Vector3.zero;
        }

        private void OnDestroy()
        {
            BIWInputWrapper.OnMouseDrag -= MouseDrag;
            BIWInputWrapper.OnMouseDragRaw -= MouseDragRaw;
            BIWInputWrapper.OnMouseWheel -= MouseWheel;

            BIWInputWrapper.OnMouseDown -= OnInputMouseDown;
            BIWInputWrapper.OnMouseUp -= OnInputMouseUp;

            if (advanceFowardInputAction != null)
            {
                advanceFowardInputAction.OnStarted -= advanceForwardStartDelegate;
                advanceFowardInputAction.OnFinished -= advanceForwardFinishedDelegate;
            }

            if (advanceBackInputAction != null)
            {
                advanceBackInputAction.OnStarted -= advanceBackStartDelegate;
                advanceBackInputAction.OnFinished -= advanceBackFinishedDelegate;
            }

            if (advanceLeftInputAction != null)
            {
                advanceLeftInputAction.OnStarted -= advanceLeftStartDelegate;
                advanceLeftInputAction.OnFinished -= advanceLeftFinishedDelegate;
            }

            if (advanceRightInputAction != null)
            {
                advanceRightInputAction.OnStarted -= advanceRightStartDelegate;
                advanceRightInputAction.OnFinished -= advanceRightFinishedDelegate;
            }

            if (advanceDownInputAction != null)
            {
                advanceDownInputAction.OnStarted -= advanceDownStartDelegate;
                advanceDownInputAction.OnFinished -= advanceDownFinishedDelegate;
            }

            if (advanceUpInputAction != null)
            {
                advanceUpInputAction.OnStarted -= advanceUpStartDelegate;
                advanceUpInputAction.OnFinished -= advanceUpFinishedDelegate;
            }

            if (cameraPanInputAction != null)
            {
                cameraPanInputAction.OnStarted -= cameraPanStartDelegate;
                cameraPanInputAction.OnFinished -= cameraPanFinishedDelegate;
            }

            if (zoomInFromKeyboardInputAction != null)
                zoomInFromKeyboardInputAction.OnTriggered -= zoomInFromKeyboardDelegate;

            if (zoomOutFromKeyboardInputAction != null)
                zoomOutFromKeyboardInputAction.OnTriggered -= zoomOutFromKeyboardDelegate;

            if (smoothScrollCoroutine != null)
                CoroutineStarter.Stop(smoothScrollCoroutine);

            if (smoothLookAtCoroutine != null)
                CoroutineStarter.Stop(smoothLookAtCoroutine);

            if (smoothFocusOnTargetCoroutine != null)
                CoroutineStarter.Stop(smoothFocusOnTargetCoroutine);
            
            Settings.i.generalSettings.OnChanged -= UpdateLocalGeneralSettings;
        }

        private void Update()
        {
            HandleCameraLook();
            HandleCameraPan();
            HandleCameraMovement();
        }

        #region CameraTransformChanges

        private void HandleCameraMovement()
        {
            HandleCameraMovementInput();
            if (direction.magnitude >= CAMERA_MOVEMENT_DEACTIVATE_INPUT_MAGNITUD)
                nextTranslation = direction;
            nextTranslation = Vector3.Lerp(nextTranslation, Vector3.zero, lerpTime * Time.deltaTime);

            if (nextTranslation.magnitude >= CAMERA_MOVEMENT_THRESHOLD)
                transform.Translate(nextTranslation * (movementSpeed * Time.deltaTime), Space.Self);
        }

        private void HandleCameraLook()
        {
            Quaternion nextIteration =  Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(pitch, yaw, 0f)), cameraLookAdvance);

            if (Mathf.Abs(nextIteration.eulerAngles.magnitude - transform.rotation.eulerAngles.magnitude) >= CAMERA_ANGLE_THRESHOLD)
                transform.rotation = nextIteration;
        }

        private void HandleCameraPan()
        {
            panAxisX = Mathf.Lerp(panAxisX, 0, cameraPanAdvance * Time.deltaTime);
            panAxisY = Mathf.Lerp(panAxisY, 0, cameraPanAdvance * Time.deltaTime);

            if (Mathf.Abs(panAxisX) >= CAMERA_PAN_THRESHOLD || Mathf.Abs(panAxisY) >= CAMERA_PAN_THRESHOLD)
                transform.Translate(panAxisX, panAxisY, 0);
        }

        #endregion

        internal void HandleCameraMovementInput()
        {
            int velocityChangedCount = 0;
            if (isAdvancingForward)
            {
                cameraVelocity += GetTotalVelocity(Vector3.forward);
                velocityChangedCount++;
            }

            if (isAdvancingBackward)
            {
                cameraVelocity += GetTotalVelocity(Vector3.back);
                velocityChangedCount++;
            }

            if (!isAdvancingBackward && !isAdvancingForward)
                cameraVelocity.z = Mathf.Lerp(cameraVelocity.z, 0, lerpDeccelerationTime);

            if (isAdvancingRight)
            {
                cameraVelocity += GetTotalVelocity(Vector3.right) * xPlaneSpeedPercentCompensantion;
                velocityChangedCount++;
            }

            if (isAdvancingLeft)
            {
                cameraVelocity += GetTotalVelocity(Vector3.left) * xPlaneSpeedPercentCompensantion;
                velocityChangedCount++;
            }

            if (!isAdvancingRight && !isAdvancingLeft)
                cameraVelocity.x = Mathf.Lerp(cameraVelocity.x, 0, lerpDeccelerationTime);

            if (isAdvancingUp)
            {
                cameraVelocity += GetTotalVelocity(Vector3.up);
                velocityChangedCount++;
            }

            if (isAdvancingDown)
            {
                cameraVelocity += GetTotalVelocity(Vector3.down);
                velocityChangedCount++;
            }

            if (!isAdvancingUp && !isAdvancingDown)
                cameraVelocity.y = Mathf.Lerp(cameraVelocity.y, 0, lerpDeccelerationTime);

            if (velocityChangedCount != 0)
                cameraVelocity = Vector3.ClampMagnitude(cameraVelocity, speedClamp);

            direction = cameraVelocity;
        }

        private Vector3 GetTotalVelocity(Vector3 velocityToAdd)
        {
            if (!isMouseRightClickDown)
                return  Vector3.zero;

            if (isDetectingMovement)
                hasBeenMovement = true;
            return velocityToAdd * (initalAcceleration * Time.deltaTime);
        }

        public void SetCameraCanMove(bool canMove) { isCameraAbleToMove = canMove; }

        internal void MouseWheel(float axis)
        {
            if (!isCameraAbleToMove || isCameraDragging)
                return;

            if (smoothScrollCoroutine != null)
                CoroutineStarter.Stop(smoothScrollCoroutine);

            float delta = Time.time - lastMouseWheelTime;
            float scrollValue = axis * Mathf.Clamp01(delta);
            lastMouseWheelTime = Time.time;

            smoothScrollCoroutine = CoroutineStarter.Start(SmoothScroll(scrollValue));
        }

        internal void MouseDragRaw(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 1 && !isPanCameraActive)
                CameraLook(axisX, axisY);
        }

        internal void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 2 || buttonId == 1 && isPanCameraActive)
                CameraDrag(axisX, axisY);
        }

        private void CameraDrag(float axisX, float axisY)
        {
            if (!isCameraAbleToMove)
                return;

            panAxisX += -axisX * Time.deltaTime * dragSpeed;
            panAxisY += -axisY * Time.deltaTime * dragSpeed;
            cameraPanAdvance = smoothCameraPanAceleration;
            isCameraDragging = true;
        }

        private void CameraLook(float axisX, float axisY)
        {
            if (!isCameraAbleToMove || !isMouseRightClickDown)
                return;

            yaw += lookSpeedH * axisX;
            if (invertMouseY)
            {
                pitch -= lookSpeedV * -axisY;
            }
            else {
                pitch -= lookSpeedV * axisY;
            }
            cameraLookAdvance = smoothCameraLookSpeed * Time.deltaTime;
        }

        public override Vector3 OnGetRotation() { return transform.eulerAngles; }

        public Vector3 GetCameraPosition => defaultVirtualCamera.transform.position;
        public Vector3 GetCameraFoward => defaultVirtualCamera.transform.forward;
        
        public void FocusOnEntities(List<BIWEntity> entitiesToFocus)
        {
            if (entitiesToFocus.Count <= 0)
                return;

            Vector3 middlePoint = FindMidPoint(entitiesToFocus);
            if (Vector3.positiveInfinity == middlePoint ||
                Vector3.negativeInfinity == middlePoint ||
                float.IsNaN(middlePoint.x) ||
                float.IsNaN(middlePoint.y) ||
                float.IsNaN(middlePoint.z))
                return;

            if (smoothFocusOnTargetCoroutine != null)
                CoroutineStarter.Stop(smoothFocusOnTargetCoroutine);
            smoothFocusOnTargetCoroutine = CoroutineStarter.Start(SmoothFocusOnTarget(middlePoint));
            SmoothLookAt(middlePoint);
        }

        public void SetPosition(Vector3 position) { transform.position = position; }

        public void LookAt(Transform transformToLookAt) { LookAt(transformToLookAt.position); }

        public void LookAt(Vector3 pointToLookAt)
        {
            transform.LookAt(pointToLookAt);
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
        }

        public void SmoothLookAt(Transform transformToLookAt) { SmoothLookAt(transformToLookAt.position); }

        public void SmoothLookAt(Vector3 position)
        {
            if (smoothLookAtCoroutine != null)
                CoroutineStarter.Stop(smoothLookAtCoroutine);
            smoothLookAtCoroutine = CoroutineStarter.Start(SmoothLookAtCorutine(position));
        }

        Vector3 FindMidPoint(List<BIWEntity> entitiesToLook)
        {
            Vector3 finalPosition = Vector3.zero;
            int totalPoints = 0;
            foreach (BIWEntity entity in entitiesToLook)
            {
                if (entity.rootEntity.meshRootGameObject && entity.rootEntity.meshesInfo.renderers.Length > 0)
                {
                    Vector3 midPointFromEntity = Vector3.zero;
                    foreach (Renderer render in entity.rootEntity.renderers)
                    {
                        if (render == null)
                            continue;
                        midPointFromEntity += render.bounds.center;
                    }

                    midPointFromEntity /= entity.rootEntity.renderers.Length;
                    finalPosition += midPointFromEntity;
                    totalPoints++;
                }
            }

            finalPosition /= totalPoints;
            return finalPosition;
        }

        IEnumerator SmoothScroll(float axis)
        {
            float scrollMovementDestination = axis * zoomSpeed;

            Vector3 targetPosition = transform.position + transform.TransformDirection(Vector3.forward * scrollMovementDestination);

            float advance = 0;
            while (advance <= 1)
            {
                advance += smoothLookAtSpeed * Time.deltaTime;
                Vector3 result = Vector3.Lerp(transform.position, targetPosition, advance);
                transform.position = result;
                yield return null;
            }
        }

        IEnumerator SmoothFocusOnTarget(Vector3 targetPosition)
        {
            float advance = 0;
            while (advance <= 1)
            {
                advance += smoothLookAtSpeed * Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPosition, advance);
                if (Vector3.Distance(transform.position, targetPosition) <= focusDistance)
                    yield break;
                yield return null;
            }
        }

        IEnumerator SmoothLookAtCorutine(Vector3 targetPosition)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            float advance = 0;
            while (advance <= 1)
            {
                advance += smoothLookAtSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, advance);
                yield return null;
            }

            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
        }

        public void SetResetConfiguration(Vector3 position, Transform lookAt) { SetResetConfiguration(position, lookAt.position); }

        public void SetResetConfiguration(Vector3 position, Vector3 pointToLook)
        {
            originalCameraPosition = position;
            originalCameraPointToLookAt = pointToLook;
        }

        public void ResetCameraPosition()
        {
            SetPosition(originalCameraPosition);
            LookAt(originalCameraPointToLookAt);
            direction = Vector3.zero;
        }
    }
}