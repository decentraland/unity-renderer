using Builder.Gizmos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraMovement : CameraStateBase
{
    private const int SCENE_SNAPSHOT_WIDTH_RES = 854;
    private const int SCENE_SNAPSHOT_HEIGHT_RES = 480;

    public float focusDistance = 5f;

    [Header("Constants")]
    public float AccelerationMod;
    public float XAxisSensitivity;
    public float YAxisSensitivity;
    public float DecelerationMod;

    [Space]
    public float MaximumMovementSpeed = 1f;

    private Vector3 _moveSpeed = Vector3.zero;

    [Header("Manual Camera Movement")]
    public float smoothLookAtSpeed = 5f;
    public float smoothCameraLookSpeed = 5f;
    public float smoothCameraMovementSpeed = 5f;
    public float smoothCameraDeceleration = 5f;
    public float keyboardMovementSpeed = 5f;

    public float lookSpeedH = 2f;

    public float lookSpeedV = 2f;

    public float zoomSpeed = 2f;

    public float dragSpeed = 3f;

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

    private float yaw = 0f;
    private float pitch = 0f;

    private bool isCameraAbleToMove = true;

    private bool isAdvancingForward = false;
    private bool isAdvancingBackward = false;
    private bool isAdvancingLeft = false;
    private bool isAdvancingRight = false;
    private bool isAdvancingUp = false;
    private bool isAdvancingDown = false;

    private bool isDetectingMovement = false;
    private bool hasBeenMovement = false;

    private bool isPanCameraActive = false;
    private bool isMouseRightClickDown = false;

    private bool isCameraRotating = false;

    private Coroutine smoothCameraLookCor;
    private Coroutine smoothLookAtCor;
    private Coroutine smoothFocusOnTargetCor;
    private Coroutine smoothScrollCor;

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

    private Vector3 originalCameraPosition;
    private Vector3 cameraDestinationPosition;
    private Transform originalCameraLookAt;

    private float lastMouseWheelTime = 0;
    public delegate void OnSnapshotsReady(Texture2D sceneSnapshot);

    private void Awake()
    {
        BuilderInWorldInputWrapper.OnMouseDrag += MouseDrag;
        BuilderInWorldInputWrapper.OnMouseDragRaw += MouseDragRaw;
        BuilderInWorldInputWrapper.OnMouseWheel += MouseWheel;

        BuilderInWorldInputWrapper.OnMouseDown += OnInputMouseDown;
        BuilderInWorldInputWrapper.OnMouseUp += OnInputMouseUp;

        DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmoTransformObjectStart;
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnd;

        advanceForwardStartDelegate = (action) => isAdvancingForward = true;
        advanceForwardFinishedDelegate = (action) => isAdvancingForward = false;

        advanceFowardInputAction.OnStarted += advanceForwardStartDelegate;
        advanceFowardInputAction.OnFinished += advanceForwardFinishedDelegate;

        advanceBackStartDelegate = (action) => isAdvancingBackward = true;
        advanceBackFinishedDelegate = (action) => isAdvancingBackward = false;

        advanceBackInputAction.OnStarted += advanceBackStartDelegate;
        advanceBackInputAction.OnFinished += advanceBackFinishedDelegate;

        advanceLeftStartDelegate = (action) => isAdvancingLeft = true;
        advanceLeftFinishedDelegate = (action) => isAdvancingLeft = false;

        advanceLeftInputAction.OnStarted += advanceLeftStartDelegate;
        advanceLeftInputAction.OnFinished += advanceLeftFinishedDelegate;

        advanceRightStartDelegate = (action) => isAdvancingRight = true;
        advanceRightFinishedDelegate = (action) => isAdvancingRight = false;

        advanceRightInputAction.OnStarted += advanceRightStartDelegate;
        advanceRightInputAction.OnFinished += advanceRightFinishedDelegate;

        advanceUpStartDelegate = (action) => isAdvancingUp = true;
        advanceUpFinishedDelegate = (action) => isAdvancingUp = false;

        advanceUpInputAction.OnStarted += advanceUpStartDelegate;
        advanceUpInputAction.OnFinished += advanceUpFinishedDelegate;

        advanceDownStartDelegate = (action) => isAdvancingDown = true;
        advanceDownFinishedDelegate = (action) => isAdvancingDown = false;

        advanceDownInputAction.OnStarted += advanceDownStartDelegate;
        advanceDownInputAction.OnFinished += advanceDownFinishedDelegate;

        cameraPanStartDelegate = (action) => isPanCameraActive = true;
        cameraPanFinishedDelegate = (action) => isPanCameraActive = false;

        cameraPanInputAction.OnStarted += cameraPanStartDelegate;
        cameraPanInputAction.OnFinished += cameraPanFinishedDelegate;

        zoomInFromKeyboardDelegate = (action) => MouseWheel(1f);
        zoomInFromKeyboardInputAction.OnTriggered += zoomInFromKeyboardDelegate;

        zoomOutFromKeyboardDelegate = (action) => MouseWheel(-1f);
        zoomOutFromKeyboardInputAction.OnTriggered += zoomOutFromKeyboardDelegate;
    }

    public void StartDectectingMovement()
    {
        isDetectingMovement = true;
        hasBeenMovement = false;
    }

    public bool HasBeenMovement => hasBeenMovement;

    public void StopDetectingMovement() { isDetectingMovement = false; }

    private void OnInputMouseUp(int buttonId, Vector3 mousePosition)
    {
        if (buttonId != 1)
            return;

        isMouseRightClickDown = false;
    }

    private void OnInputMouseDown(int buttonId, Vector3 mousePosition)
    {
        if (buttonId != 1)
            return;

        isMouseRightClickDown = true;
        _moveSpeed = Vector3.zero;
    }

    private void OnDestroy()
    {
        BuilderInWorldInputWrapper.OnMouseDrag -= MouseDrag;
        BuilderInWorldInputWrapper.OnMouseDragRaw -= MouseDragRaw;
        BuilderInWorldInputWrapper.OnMouseWheel -= MouseWheel;

        BuilderInWorldInputWrapper.OnMouseDown -= OnInputMouseDown;
        BuilderInWorldInputWrapper.OnMouseUp -= OnInputMouseUp;

        advanceFowardInputAction.OnStarted -= advanceForwardStartDelegate;
        advanceFowardInputAction.OnFinished -= advanceForwardFinishedDelegate;

        advanceBackInputAction.OnStarted -= advanceBackStartDelegate;
        advanceBackInputAction.OnFinished -= advanceBackFinishedDelegate;

        advanceLeftInputAction.OnStarted -= advanceLeftStartDelegate;
        advanceLeftInputAction.OnFinished -= advanceLeftFinishedDelegate;

        advanceRightInputAction.OnStarted -= advanceRightStartDelegate;
        advanceRightInputAction.OnFinished -= advanceRightFinishedDelegate;

        advanceDownInputAction.OnStarted -= advanceDownStartDelegate;
        advanceDownInputAction.OnFinished -= advanceDownFinishedDelegate;

        advanceUpInputAction.OnStarted -= advanceUpStartDelegate;
        advanceUpInputAction.OnFinished -= advanceUpFinishedDelegate;

        cameraPanInputAction.OnStarted -= cameraPanStartDelegate;
        cameraPanInputAction.OnFinished -= cameraPanFinishedDelegate;

        zoomInFromKeyboardInputAction.OnTriggered -= zoomInFromKeyboardDelegate;
        zoomOutFromKeyboardInputAction.OnTriggered -= zoomOutFromKeyboardDelegate;
    }

    private void Update()
    {


        var acceleration = HandleKeyInput();
        _moveSpeed += acceleration;
        HandleDeceleration(acceleration);

        // clamp the move speed
        if (_moveSpeed.magnitude > MaximumMovementSpeed)
        {
            _moveSpeed = _moveSpeed.normalized * MaximumMovementSpeed;
        }

        transform.Translate(_moveSpeed);

        // Vector3 cameraVelocity = Vector3.zero; 
        //
        // if (isAdvancingForward)
        //     cameraVelocity += GetTotalVelocity( transform.forward);
        //
        // if (isAdvancingBackward)
        //     cameraVelocity += GetTotalVelocity( -transform.forward);
        //
        // if (isAdvancingRight)
        //     cameraVelocity += GetTotalVelocity( transform.right);
        //
        // if (isAdvancingLeft)
        //     cameraVelocity += GetTotalVelocity( -transform.right);
        //
        // if (isAdvancingUp)
        //     cameraVelocity += GetTotalVelocity( Vector3.up);
        //
        // if (isAdvancingDown)
        //     cameraVelocity += GetTotalVelocity( -Vector3.up);
        //
        // cameraDestinationPosition = transform.position + cameraVelocity ;
    }
    private void HandleDeceleration(Vector3 acceleration)
    {
        //deceleration functionality
        if (Mathf.Approximately(Mathf.Abs(acceleration.x), 0))
        {
            if (Mathf.Abs(_moveSpeed.x) < DecelerationMod)
            {
                _moveSpeed.x = 0;
            }
            else
            {
                _moveSpeed.x -= DecelerationMod * Mathf.Sign(_moveSpeed.x);
            }
        }

        if (Mathf.Approximately(Mathf.Abs(acceleration.y), 0))
        {
            if (Mathf.Abs(_moveSpeed.y) < DecelerationMod)
            {
                _moveSpeed.y = 0;
            }
            else
            {
                _moveSpeed.y -= DecelerationMod * Mathf.Sign(_moveSpeed.y);
            }
        }

        if (Mathf.Approximately(Mathf.Abs(acceleration.z), 0))
        {
            if (Mathf.Abs(_moveSpeed.z) < DecelerationMod)
            {
                _moveSpeed.z = 0;
            }
            else
            {
                _moveSpeed.z -= DecelerationMod * Mathf.Sign(_moveSpeed.z);
            }
        }
    }
    private Vector3 HandleKeyInput()
    {
        var acceleration = Vector3.zero;

        if (isMouseRightClickDown)
        {
            if (isAdvancingForward)
                acceleration.z += 1;

            if (isAdvancingBackward)
                acceleration.z -= 1;

            if (isAdvancingLeft)
                acceleration.x -= 1;

            if (isAdvancingRight)
                acceleration.x += 1;

            if (isAdvancingUp)
                acceleration.y += 1;

            if (isAdvancingDown)
                acceleration.y -= 1;

        }

        return acceleration.normalized * AccelerationMod;
    }

    public Vector3 GetTotalVelocity(Vector3 direction)
    {
        if (isDetectingMovement)
            hasBeenMovement = true;
        return direction * (keyboardMovementSpeed * Time.deltaTime);
    }

    public void SetCameraCanMove(bool canMove) { isCameraAbleToMove = canMove; }

    private void OnGizmoTransformObjectEnd(string gizmoType) { isCameraAbleToMove = true; }

    private void OnGizmoTransformObjectStart(string gizmoType) { isCameraAbleToMove = false; }

    private void MouseWheel(float axis)
    {
        if (!isCameraAbleToMove)
            return;

        if (smoothScrollCor != null)
            CoroutineStarter.Stop(smoothScrollCor);

        float delta = Time.time - lastMouseWheelTime;
        float scrollValue = axis * Mathf.Clamp01(delta);
        lastMouseWheelTime = Time.time;

        smoothScrollCor = CoroutineStarter.Start(SmoothScroll(scrollValue));
    }

    private void MouseDragRaw(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (buttonId == 1 && !isPanCameraActive)
            CameraLook(axisX, axisY);
    }

    private void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (buttonId == 2 || buttonId == 1 && isPanCameraActive)
            CameraDrag(axisX, axisY);
    }

    public void CameraDrag(float axisX, float axisY)
    {
        if (isCameraAbleToMove)
            transform.Translate(-axisX * Time.deltaTime * dragSpeed, -axisY * Time.deltaTime * dragSpeed, 0);
    }

    public void CameraLook(float axisX, float axisY)
    {
        if (isCameraAbleToMove)
        {
            yaw += lookSpeedH * axisX;
            pitch -= lookSpeedV * axisY;

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            // if (smoothCameraLookCor != null)
            //     CoroutineStarter.Stop(smoothCameraLookCor);
            // smoothCameraLookCor = CoroutineStarter.Start(SmoothCameraLook(axisX, axisY));
        }
    }

    public override Vector3 OnGetRotation() { return transform.eulerAngles; }

    public void FocusOnEntities(List<DCLBuilderInWorldEntity> entitiesToFocus)
    {
        if (entitiesToFocus.Count > 0)
        {
            Vector3 middlePoint = FindMidPoint(entitiesToFocus);
            if (smoothFocusOnTargetCor != null)
                CoroutineStarter.Stop(smoothFocusOnTargetCor);
            smoothFocusOnTargetCor = CoroutineStarter.Start(SmoothFocusOnTarget(middlePoint));
            SmoothLookAt(middlePoint);
        }
    }

    public void SetPosition(Vector3 position) { transform.position = position; }

    public void LookAt(Transform transformToLookAt)
    {
        transform.LookAt(transformToLookAt);
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    public void SmoothLookAt(Transform transform) { SmoothLookAt(transform.position); }

    public void SmoothLookAt(Vector3 position)
    {
        if (smoothLookAtCor != null)
            CoroutineStarter.Stop(smoothLookAtCor);
        smoothLookAtCor = CoroutineStarter.Start(SmoothLookAtCorutine(position));
    }

    Vector3 FindMidPoint(List<DCLBuilderInWorldEntity> entitiesToLook)
    {
        Vector3 finalPosition = Vector3.zero;
        int totalPoints = 0;
        foreach (DCLBuilderInWorldEntity entity in entitiesToLook)
        {
            if (entity.rootEntity.meshRootGameObject && entity.rootEntity.meshesInfo.renderers.Length > 0)
            {
                Vector3 midPointFromEntity = Vector3.zero;
                foreach (Renderer render in entity.rootEntity.renderers)
                {
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

    IEnumerator SmoothCameraLook(float axisX, float axisY)
    {
        yaw += lookSpeedH * axisX;
        pitch -= lookSpeedV * axisY;

        Vector3 destination = new Vector3(pitch, yaw, 0f);

        float advance = 0;
        while (advance <= 1)
        {
            advance += smoothCameraLookSpeed * Time.deltaTime;

            Vector3 result = Vector3.Lerp(transform.eulerAngles, destination, advance);
            transform.eulerAngles  = result;
            yield return null;
        }
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

    public void SetResetConfiguration(Vector3 position, Transform lookAt)
    {
        originalCameraPosition = position;
        originalCameraLookAt = lookAt;
    }

    public void ResetCameraPosition()
    {
        SetPosition(originalCameraPosition);
        LookAt(originalCameraLookAt);
        _moveSpeed = Vector3.zero;
    }

    public void TakeSceneScreenshot(OnSnapshotsReady onSuccess) { StartCoroutine(TakeSceneScreenshotCoroutine(onSuccess)); }

    private IEnumerator TakeSceneScreenshotCoroutine(OnSnapshotsReady callback)
    {
        var current = camera.targetTexture;
        camera.targetTexture = null;

        yield return null;

        Texture2D sceneScreenshot = ScreenshotFromCamera(SCENE_SNAPSHOT_WIDTH_RES, SCENE_SNAPSHOT_HEIGHT_RES);

        yield return null;

        camera.targetTexture = current;
        callback?.Invoke(sceneScreenshot);
    }

    private Texture2D ScreenshotFromCamera(int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 32);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        screenShot.Apply();

        return screenShot;
    }
}