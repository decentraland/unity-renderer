using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DCL.Components;
using DCL.Models;
using DCL.Configuration;

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
    LayerMask gizmoMask;
    LayerMask groundMask;
    int selectionLayer;
    int defaultLayer;

    void Start()
    {
        originPointerPosition = Vector3.zero;
        gizmoAxis = null;
        defaultMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Selection");
        gizmoMask = LayerMask.GetMask("Gizmo");
        groundMask = LayerMask.GetMask("Ground");
        selectionLayer = LayerMask.NameToLayer("Selection");
        defaultLayer = LayerMask.NameToLayer("Default");
        ReportMovement();

    }


    void Update()
    {
        if (!IsPointerOverUIObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMouseDownPosition = Input.mousePosition;
                bool hit = MousePointerRaycast(defaultMask, true);
                if (hit)
                {
                    gizmoAxis = hitInfo.collider.gameObject.GetComponent<GizmoAxis>();
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
                if (selectedObject != null)
                {
                    NotifyGizmoEvent(selectedObject);
                }
                originPointerPosition = Vector3.zero;
                transformingObject = false;
                if (isOrbitingCamera)
                {
                    ReportMovement();
                }
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

        if (Input.GetMouseButtonUp(1))
        {
            ReportMovement();
        }


    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ReportMovement();
        }
    }


    void SelectObject(GameObject sObject)
    {
        if (sObject != null)
        {
            selectedObject = sObject;

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
        s.layer = selectionLayer;
        ChangeLayersRecursively(s.transform, selectionLayer, defaultLayer);
    }
    void UnSelectionEffect(GameObject s)
    {
        s.layer = defaultLayer;
        ChangeLayersRecursively(s.transform, defaultLayer, selectionLayer);
    }

    void ChangeLayersRecursively(Transform trans, int layer, int currentLayer)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            Transform child = trans.GetChild(i);
            if (child.gameObject.layer == currentLayer)
            {
                child.gameObject.layer = layer;
                ChangeLayersRecursively(child, layer, currentLayer);
            }
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

    public void SetReady()
    {
        characterController.gameObject.SetActive(false);
        cameraController.SetPosition(new Vector3(characterController.transform.position.x + ParcelSettings.PARCEL_SIZE / 2,
                                                0,
                                                characterController.transform.position.z + ParcelSettings.PARCEL_SIZE / 2));
        camera.gameObject.SetActive(true);

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
            MousePointerRaycast(groundMask);
            gizmoAxis.UpdateTransformation(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0), pointerPosition, selectedObject, hitInfo);
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
            bool hit = MousePointerRaycast(groundMask, true);
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

        bool hit = MousePointerRaycast(gizmoMask);
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

    bool MousePointerRaycast(LayerMask mask, bool checkGizmo = false)
    {
        hitInfo = new RaycastHit();
        if (checkGizmo)
        {
            bool hit = MousePointerRaycast(gizmoMask);
            if (hit)
                return true;
        }
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

    public void NotifyGizmoEvent(GameObject selectedObject)
    {
        LoadWrapper wrapper = selectedObject.GetComponentInChildren<LoadWrapper>();
        if (wrapper == null)
            return;

        DecentralandEntity entity = wrapper.entity;
        DCL.Interface.WebInterface.ReportGizmoEvent(entity.scene.sceneData.id, entity.entityId, "gizmoDragEnded", GizmoType.NONE, selectedObject.transform);
    }

    void ReportMovement()
    {
        var rotation = camera.transform.rotation;
        var reportCameraPosition = cameraController.GetLookAtPosition();
        DCL.Interface.WebInterface.ReportPosition(reportCameraPosition, rotation, 0);
    }


}
