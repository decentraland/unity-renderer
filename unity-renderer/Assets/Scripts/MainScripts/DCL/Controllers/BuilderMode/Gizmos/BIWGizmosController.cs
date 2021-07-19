using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Camera;
using UnityEngine;

public interface IBIWGizmosController
{
    public delegate void GizmoTransformDelegate(string gizmoType);

    public event GizmoTransformDelegate OnGizmoTransformObjectStart;
    public event GizmoTransformDelegate OnGizmoTransformObject;
    public event GizmoTransformDelegate OnGizmoTransformObjectEnd;
    public event Action<Vector3> OnChangeTransformValue;

    public IBIWGizmos activeGizmo { get;  set; }

    public string GetSelectedGizmo();
    public void SetSnapFactor(float position, float rotation, float scale);
    public void SetSelectedEntities(Transform selectionParent, List<BIWEntity> entities);
    public void ShowGizmo();
    public void HideGizmo(bool setInactiveGizmos = false);
    public bool IsGizmoActive();
    public void ForceRelativeScaleRatio();
    public bool HasAxisHover();
    public void SetGizmoType(string gizmoType);
}

public class BIWGizmosController : BIWController, IBIWGizmosController
{
    public event IBIWGizmosController.GizmoTransformDelegate OnGizmoTransformObjectStart;
    public event IBIWGizmosController.GizmoTransformDelegate OnGizmoTransformObject;
    public event IBIWGizmosController.GizmoTransformDelegate OnGizmoTransformObjectEnd;

    public event Action<Vector3> OnChangeTransformValue;

    private IBIWRaycastController raycastController;

    private IBIWGizmos[] gizmos;

    private bool isTransformingObject;
    public IBIWGizmos activeGizmo { get; set; }

    private SnapInfo snapInfo = new SnapInfo();

    private BIWGizmosAxis hoveredAxis = null;

    private Transform selectedEntitiesParent;
    private List<BIWEntity> selectedEntities;
    private GameObject gizmosGO;
    private FreeCameraMovement freeCameraMovement;

    public override void Init(BIWContext context)
    {
        base.Init(context);
        gizmosGO = GameObject.Instantiate(context.projectReferences.gizmosPrefab, context.projectReferences.gizmosPrefab.transform.position, context.projectReferences.gizmosPrefab.transform.rotation);
        gizmos = gizmosGO.GetComponentsInChildren<IBIWGizmos>(true);

        raycastController = context.raycastController;

        raycastController.OnGizmosAxisPressed += OnGizmosAxisPressed;
        BIWInputWrapper.OnMouseUp += OnMouseUp;
        BIWInputWrapper.OnMouseDrag += OnMouseDrag;
        if (context.sceneReferences.cameraController.TryGetCameraStateByType<FreeCameraMovement>(out CameraStateBase cameraState))
            freeCameraMovement = (FreeCameraMovement)cameraState;

        // NOTE(Adrian): Take into account that right now to get the relative scale of the gizmos, we set the gizmos in the player position and the camera
        InitializeGizmos(context.sceneReferences.mainCamera, freeCameraMovement.transform);
        ForceRelativeScaleRatio();
    }

    public override void Dispose()
    {
        base.Dispose();
        if (gizmosGO != null)
            GameObject.Destroy(gizmosGO);
        raycastController.OnGizmosAxisPressed -= OnGizmosAxisPressed;
        BIWInputWrapper.OnMouseUp -= OnMouseUp;
        BIWInputWrapper.OnMouseDrag -= OnMouseDrag;
    }

    public string GetSelectedGizmo()
    {
        if (IsGizmoActive())
        {
            return activeGizmo.GetGizmoType();
        }
        return DCL.Components.DCLGizmos.Gizmo.NONE;
    }

    public void SetSnapFactor(float position, float rotation, float scale)
    {
        snapInfo.position = position;
        snapInfo.rotation = rotation;
        snapInfo.scale = scale;

        if (activeGizmo != null)
        {
            activeGizmo.SetSnapFactor(snapInfo);
        }
    }

    private void OnBeginDrag(BIWGizmosAxis hittedAxis)
    {
        isTransformingObject = true;
        activeGizmo = hittedAxis.GetGizmo();
        activeGizmo.OnBeginDrag(hittedAxis, selectedEntitiesParent);
        freeCameraMovement.SetCameraCanMove(false);
        OnGizmoTransformObjectStart?.Invoke(activeGizmo.GetGizmoType());
    }

    private void OnDrag(Vector3 hitPoint, Vector2 mousePosition)
    {
        float value = activeGizmo.OnDrag(hitPoint, mousePosition);
        OnGizmoTransformObject?.Invoke(activeGizmo.GetGizmoType());
        OnChangeTransformValue?.Invoke(value * activeGizmo.GetActiveAxisVector());
    }

    private void OnEndDrag()
    {
        activeGizmo.OnEndDrag();
        freeCameraMovement.SetCameraCanMove(true);
        OnGizmoTransformObjectEnd?.Invoke(activeGizmo.GetGizmoType());
        isTransformingObject = false;
    }

    public bool HasAxisHover() { return hoveredAxis != null; }

    private void SetAxisHover(BIWGizmosAxis axis)
    {
        if (hoveredAxis != null && hoveredAxis != axis)
        {
            hoveredAxis.SetColorDefault();
        }
        else if (axis != null)
        {
            axis.SetColorHighlight();
        }
        hoveredAxis = axis;
    }

    public void ForceRelativeScaleRatio()
    {
        for (int i = 0; i < gizmos.Length; i++)
        {
            gizmos[i].ForceRelativeScaleRatio();
        }
    }

    public void ShowGizmo()
    {
        gizmosGO.gameObject.SetActive(true);
        if (activeGizmo != null)
        {
            activeGizmo.SetTargetTransform(selectedEntitiesParent);
            activeGizmo.currentGameObject.SetActive(true);
        }
    }

    public void HideGizmo(bool setInactiveGizmos = false)
    {
        if (activeGizmo != null)
        {
            activeGizmo.currentGameObject.SetActive(false);
        }
        if (setInactiveGizmos)
        {
            SetGizmoType(DCL.Components.DCLGizmos.Gizmo.NONE);
        }
        gizmosGO.gameObject.SetActive(false);
    }

    public bool IsGizmoActive() { return activeGizmo != null; }

    private bool RaycastHit(Ray ray, out Vector3 hitPoint)
    {
        if (activeGizmo != null)
        {
            return activeGizmo.RaycastHit(ray, out hitPoint);
        }
        hitPoint = Vector3.zero;
        return false;
    }

    public override void Update()
    {
        base.Update();
        if (!isTransformingObject)
        {
            CheckGizmoHover(Input.mousePosition);
        }
    }

    public void SetGizmoType(string gizmoType)
    {
        HideGizmo();

        if (gizmoType != DCL.Components.DCLGizmos.Gizmo.NONE)
        {
            bool wasGizmoActive = IsGizmoActive();

            for (int i = 0; i < gizmos.Length; i++)
            {
                if (gizmos[i].GetGizmoType() == gizmoType)
                {
                    activeGizmo = gizmos[i];
                    activeGizmo.SetSnapFactor(snapInfo);
                    break;
                }
            }

            bool areEntitiesSelected = selectedEntities != null && selectedEntities.Count > 0;
            if (wasGizmoActive && areEntitiesSelected)
            {
                ShowGizmo();
            }
            else
            {
                GizmoStatusUpdate();
            }
        }
        else
        {
            activeGizmo = null;
        }
    }

    private void InitializeGizmos(Camera camera, Transform cameraTransform)
    {
        for (int i = 0; i < gizmos.Length; i++)
        {
            if (!gizmos[i].initialized)
            {
                gizmos[i].Initialize(camera, cameraTransform);
            }
        }
    }

    public void SetSelectedEntities(Transform selectionParent, List<BIWEntity> entities)
    {
        selectedEntities = entities;
        selectedEntitiesParent = selectionParent;
        GizmoStatusUpdate();
    }
    private void OnGizmosAxisPressed(BIWGizmosAxis pressedAxis) { OnBeginDrag(pressedAxis); }

    private void OnMouseUp(int buttonId, Vector3 mousePosition)
    {
        if (!isTransformingObject)
        {
            return;
        }

        if (buttonId == 0)
        {
            OnEndDrag();
        }
    }

    private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (buttonId == 0)
        {
            bool hasMouseMoved = (axisX != 0 || axisY != 0);
            if (isTransformingObject && hasMouseMoved)
            {
                Vector3 hit;
                if (RaycastHit(raycastController.GetMouseRay(mousePosition), out hit))
                {
                    OnDrag(hit, mousePosition);
                }
            }
        }
    }

    private void CheckGizmoHover(Vector3 mousePosition)
    {
        RaycastHit hit;
        if (raycastController.RaycastToGizmos(mousePosition, out hit))
        {
            BIWGizmosAxis gizmoAxis = hit.collider.gameObject.GetComponent<BIWGizmosAxis>();
            SetAxisHover(gizmoAxis);
        }
        else
        {
            SetAxisHover(null);
        }
    }

    private void GizmoStatusUpdate()
    {
        if (IsGizmoActive())
        {
            if (selectedEntities == null || selectedEntities.Count == 0)
            {
                HideGizmo();
            }
            else
            {
                ShowGizmo();
            }
        }
    }

    public class SnapInfo
    {
        public float position = 0;
        public float rotation = 0;
        public float scale = 0;
    }
}