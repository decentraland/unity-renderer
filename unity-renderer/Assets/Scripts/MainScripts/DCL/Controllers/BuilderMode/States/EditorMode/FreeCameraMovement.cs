using Builder.Gizmos;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraMovement : CameraStateBase
{
    public BuilderInWorldInputWrapper builderInputWrapper;

    public float smoothLookAtSpeed = 5f;
    public float focusDistance = 5f;
    public float focusSpeed = 5f;

    [Header("Manual Camera Movement")]

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


    private float yaw = 0f;
    private float pitch = 0f;

    bool isCameraAbleToMove = true;
    bool isAdvancingForward = false;
    bool isAdvancingBackward = false;
    bool isAdvancingLeft = false;
    bool isAdvancingRight = false;
    bool isAdvancingUp = false;
    bool isAdvancingDown = false;

    Coroutine smoothLookAtCor, smoothFocusOnTargetCor;

    InputAction_Hold.Started advanceForwardStartDelegate;
    InputAction_Hold.Finished advanceForwardFinishedDelegate;

    InputAction_Hold.Started advanceBackStartDelegate;
    InputAction_Hold.Finished advanceBackFinishedDelegate;

    InputAction_Hold.Started advanceLeftStartDelegate;
    InputAction_Hold.Finished advanceLeftFinishedDelegate;

    InputAction_Hold.Started advanceRightStartDelegate;
    InputAction_Hold.Finished advanceRightFinishedDelegate;

    InputAction_Hold.Started advanceDownStartDelegate;
    InputAction_Hold.Finished advanceDownFinishedDelegate;

    InputAction_Hold.Started advanceUpStartDelegate;
    InputAction_Hold.Finished advanceUpFinishedDelegate;


    private void Awake()
    {    
        builderInputWrapper.OnMouseDrag += MouseDrag;
        builderInputWrapper.OnMouseDragRaw += MouseDragRaw;
        builderInputWrapper.OnMouseWheel += MouseWheel;

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
    }

    private void OnDestroy()
    {
        builderInputWrapper.OnMouseDrag -= MouseDrag;
        builderInputWrapper.OnMouseDragRaw -= MouseDragRaw;
        builderInputWrapper.OnMouseWheel -= MouseWheel;

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
    }

    private void Update()
    {
        Vector3 velocity = Vector3.zero;
        if (isAdvancingForward)
        {
            velocity += transform.forward;
        }
        if (isAdvancingBackward)
        {
            velocity += -transform.forward;
        }
        if (isAdvancingRight)
        {
            velocity += transform.right;
        }
        if (isAdvancingLeft)
        {
            velocity += -transform.right;
        }
        if (isAdvancingUp)
        {
            velocity += transform.up;
        }
        if (isAdvancingDown)
        {
            velocity += -transform.up;
        }
        transform.position += velocity * keyboardMovementSpeed * Time.deltaTime;
    }

    public void SetCameraCanMove(bool canMove)
    {
        isCameraAbleToMove = canMove;
    }

    private void OnGizmoTransformObjectEnd(string gizmoType)
    {
        isCameraAbleToMove = true;
    }

    private void OnGizmoTransformObjectStart(string gizmoType)
    {
        isCameraAbleToMove = false;
    }

    private void MouseWheel(float axis)
    {
         if(isCameraAbleToMove)
            transform.Translate(0, 0, axis * zoomSpeed, Space.Self);
    }

    private void MouseDragRaw(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if(buttonId == 1)
            CameraLook(axisX, axisY);
    }

    private void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (buttonId == 0 ||buttonId == 2)
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
        }
    }

    public override Vector3 OnGetRotation()
    {
        return transform.eulerAngles;
    }

    public void FocusOnEntities(List<DCLBuilderInWorldEntity> entitiesToFocus)
    {
        if (entitiesToFocus.Count > 0)
        {
            Vector3 middlePoint = FindMidPoint(entitiesToFocus);
            if (smoothFocusOnTargetCor != null) CoroutineStarter.Stop(smoothFocusOnTargetCor);
            smoothFocusOnTargetCor = CoroutineStarter.Start(SmoothFocusOnTarget(middlePoint));
            SmoothLookAt(middlePoint);
        }
    }


    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void LookAt(Transform transformToLookAt)
    {
        transform.LookAt(transformToLookAt);
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    public void SmoothLookAt(Transform transform)
    {

        SmoothLookAt(transform.position);
    }
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
        foreach(DCLBuilderInWorldEntity entity in entitiesToLook)
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
        while(advance <= 1)
        {
            advance += smoothLookAtSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, advance);
            yield return null;
        }
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }
}
