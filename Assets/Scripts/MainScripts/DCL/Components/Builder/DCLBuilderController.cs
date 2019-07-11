using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DCL.Components;
using DCL.Models;

public static class GizmoType
{
    public const string NONE = "NONE";
    public const string MOVE = "MOVE";
    public const string ROTATE = "ROTATE";

}

public class DCLBuilderController : MonoBehaviour
{
    public DCLCharacterController characterController;
    public DCLCinemachineCameraBuilderController cameraController;

    public Camera camera;
    public Color selectedObjectColor;
    public GameObject selectedObject;
    public Color originalColor;
    public bool transformingObject;
    public bool moveActivated;
    public List<Gizmo> gizmos;
    public bool rotateActivated;
    public Gizmo activeGizmo;
    public GizmoAxis gizmoAxis;

    public GizmoAxis gizmoAxisOver;
    Vector3 lastMouseDownPosition;
    float lastMouseDownTime;
    RaycastHit hitInfo;
    LayerMask defaultMask;
    LayerMask groundMask;

    void Start()
    {
        originPointerPosition = Vector3.zero;
        gizmoAxis = null;
        defaultMask = LayerMask.GetMask("Default");
        groundMask = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (!IsPointerOverUIObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMouseDownPosition = Input.mousePosition;
                bool hit = MousePointerRaycast(defaultMask);
                if (hit)
                {
                    GizmoAxis gizmoAxis = hitInfo.collider.gameObject.GetComponent<GizmoAxis>();
                    if (gizmoAxis != null)
                    {
                        gizmoAxis.SelectAxis(true);
                    }
                    else if (hitInfo.collider.gameObject != selectedObject)
                    {
                        DeselectObject();
                        SelectObject(GetEntity(hitInfo.collider.gameObject));
                    }
                    transformingObject = true;
                    lastMouseDownTime = Time.time;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                //Deselect the object only when not dragging 
                bool isOrbitingCamera = Vector2.Distance(lastMouseDownPosition, Input.mousePosition) > 0.01f;
                // Avoid deselect when just select
                bool justSelected = (Time.time - lastMouseDownTime) < 0.2f;
                if (!isOrbitingCamera && !justSelected)
                {
                    if (selectedObject != null)
                    {
                        DeselectObject();
                    }
                }
                if (gizmoAxis != null)
                {
                    gizmoAxis.SelectAxis(false);
                    gizmoAxis.ResetTransformation();
                    gizmoAxis = null;
                }
                originPointerPosition = Vector3.zero;
                transformingObject = false;
            }
        }
        if (transformingObject)
        {
            UpdateTransformation();
        }
        else
        {
            UpdateGizmoOver();
        }
    }

    void SelectObject(GameObject selectedObject)
    {
        if (selectedObject != null)
        {
            SelectionEffect(selectedObject);
            if (activeGizmo != null)
                ActivateGizmo(activeGizmo, true);

            LoadWrapper wrapper = selectedObject.GetComponentInChildren<LoadWrapper>();
            if (wrapper == null)
                return;

            DecentralandEntity entity = wrapper.entity;
            DCL.Interface.WebInterface.ReportGizmoEvent(entity.scene.sceneData.id, entity.entityId, "gizmoSelected", activeGizmo != null ? activeGizmo.gizmoType : GizmoType.NONE);

        }
    }

    void SelectionEffect(GameObject s)
    {
        Renderer selectedRenderer = s.GetComponentInChildren<Renderer>();
        if (selectedRenderer != null)
        {
            selectedObjectColor = selectedRenderer.material.color;
            selectedRenderer.material.color = Color.red;
        }
    }

    void UnSelectionEffect(GameObject s)
    {
        Renderer selectedRenderer = s.GetComponentInChildren<Renderer>();
        if (selectedRenderer != null)
        {
            selectedRenderer.material.color = selectedObjectColor;
        }

    }

    GameObject GetEntity(GameObject currentSelected)
    {

        LoadWrapper wrapper = currentSelected.GetComponent<LoadWrapper>();
        if (wrapper?.entity != null)
        {
            return wrapper.entity.gameObject;
        }
        else
        {
            if (currentSelected.transform.parent != null)
                return GetEntity(currentSelected.transform.parent.gameObject);
            return null;
        }

    }

    void DeselectObject()
    {
        if (selectedObject != null)
        {
            UnSelectionEffect(selectedObject);
            selectedObject = null;
            if (activeGizmo != null && activeGizmo.gameObject.activeSelf)
            {
                activeGizmo.gameObject.SetActive(false);
            }
        }
    }

    public void ResetObject()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.localRotation = Quaternion.identity;
            if (activeGizmo != null)
                activeGizmo.transform.localRotation = Quaternion.identity;
        }
    }

    public void SetPlayMode(string on)
    {
        bool bon = bool.Parse(on);
        characterController.gameObject.SetActive(bon);
        cameraController.SetPosition(new Vector3(characterController.transform.position.x,
                                                0,
                                                characterController.transform.position.z));
        camera.gameObject.SetActive(!bon);
    }

    public void TogglePlayMode()
    {
        bool playMode = !characterController.gameObject.activeSelf;
        characterController.gameObject.SetActive(playMode);
        cameraController.SetPosition(new Vector3(characterController.transform.position.x,
                                                0,
                                                characterController.transform.position.z));
        camera.gameObject.SetActive(!playMode);
    }

    public void SelectGizmo(string gizmoType)
    {
        ActivateGizmo(activeGizmo, false);
        Gizmo gizmo = GetGizmo(gizmoType);
        if (gizmo != null)
        {

            ActivateGizmo(gizmo, true);
        }
    }

    Gizmo GetGizmo(string gizmoType)
    {
        for (int i = 0; i < gizmos.Count; i++)
        {
            Gizmo gizmo = gizmos[i];
            if (gizmo.gizmoType.CompareTo(gizmoType) == 0)
                return gizmo;
        }
        return null;
    }

    void ActivateGizmo(Gizmo gizmo, bool activate)
    {
        if (activate)
        {
            if (activeGizmo != null && activeGizmo != gizmo)
            {
                ActivateGizmo(activeGizmo, false);
            }
            activeGizmo = gizmo;
            if (selectedObject != null)
            {
                gizmo.SetObject(selectedObject);
            }
        }
        else
        {
            activeGizmo = null;
            if (gizmo != null)
                gizmo.SetObject(null);
        }
    }

    Vector3 originPointerPosition;
    public float snapFactor = 1;

    void UpdateTransformation()
    {
        Vector3 pointerPosition = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane));
        if (gizmoAxis == null)
        {
            DragMove(pointerPosition);
        }
        else
        {
            gizmoAxis.UpdateTransformation(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0), selectedObject);
        }
    }

    void DragMove(Vector3 pointerPosition)
    {
        if (originPointerPosition == Vector3.zero)
        {
            originPointerPosition = pointerPosition;
        }
        if (Mathf.Abs(Vector3.Distance(originPointerPosition, pointerPosition)) > snapFactor)
        {
            bool hit = MousePointerRaycast(groundMask);
            if (hit && selectedObject != null)
            {

                selectedObject.transform.position = new Vector3(hitInfo.point.x,
                                                               selectedObject.transform.position.y,
                                                               hitInfo.point.z);
                if (activeGizmo != null && !activeGizmo.transformWithObject)
                {
                    activeGizmo.transform.position = selectedObject.transform.position;
                }
            }
            originPointerPosition = pointerPosition;
        }
    }



    //When Touching UI
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }


    void UpdateGizmoOver()
    {

        bool hit = MousePointerRaycast(defaultMask);
        if (hit)
        {
            GizmoAxis gizmoAxis = hitInfo.collider.gameObject.GetComponent<GizmoAxis>();
            if (gizmoAxis != null)
            {
                gizmoAxisOver = gizmoAxis;
                gizmoAxisOver.SelectAxis(true);
            }
        }
        else if (gizmoAxisOver != null)
        {
            gizmoAxisOver.SelectAxis(false);
        }
    }

    bool MousePointerRaycast(LayerMask mask)
    {
        hitInfo = new RaycastHit();
        return Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hitInfo, 10000, mask);
    }

    public void ZoomDelta(string delta)
    {
        if (int.Parse(delta) < 0)
        {
            ZoomIn();
        }
        else
        {
            ZoomOut();
        }
    }

    // Camera Functions
    public void ZoomIn()
    {
        cameraController.ZoomIn();
    }
    public void ZoomOut()
    {
        cameraController.ZoomOut();
    }

    public void ResetCamera()
    {
        cameraController.Reset();
    }

}
