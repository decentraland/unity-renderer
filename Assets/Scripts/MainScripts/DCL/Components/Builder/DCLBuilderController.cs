using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public Gizmo moveGizmo;
    public Gizmo rotateGizmo;
    public bool rotateActivated;
    public Gizmo activeGizmo;
    public GizmoAxis gizmoAxis;

    public GizmoAxis gizmoAxisOver;
    Vector3 lastMouseDownPosition;
    float lastMouseDownTime;
    RaycastHit hitInfo;
    LayerMask defaultMask;
    LayerMask groundMask;

    // Start is called before the first frame update
    void Start()
    {
        originPointerPosition = Vector3.zero;
        gizmoAxis = null;
        defaultMask = LayerMask.GetMask("Default");
        groundMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
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
                    if (hitInfo.collider.gameObject.GetComponent<GizmoAxis>())
                    {
                        gizmoAxis = hitInfo.collider.gameObject.GetComponent<GizmoAxis>();
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

    void SelectObject(GameObject s)
    {
        if (s != null)
        {
            selectedObject = s;
            SelectionEffect(selectedObject);
            if (activeGizmo != null)
                ActivateGizmo(activeGizmo, true);
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
        Debug.Log(currentSelected.name);

        if (currentSelected.name.ToLower().Contains("entity") || currentSelected.GetComponent<Renderer>())
        {
            return currentSelected;
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

    public void ToggleMove()
    {
        ActivateGizmo(moveGizmo, activeGizmo != moveGizmo);
    }

    public void ToggleRotate()
    {
        ActivateGizmo(rotateGizmo, activeGizmo != rotateGizmo);
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

    public void TogglePlayMode()
    {
        bool playMode = !characterController.gameObject.activeSelf;
        characterController.gameObject.SetActive(playMode);
        cameraController.SetPosition(new Vector3(characterController.transform.position.x,
                                                0,
                                                characterController.transform.position.z));
        camera.gameObject.SetActive(!playMode);
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
            gizmoAxis.UpdateTransformation(pointerPosition, selectedObject);
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
            Debug.Log("DRAG");

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
        if (hit && hitInfo.collider.gameObject.GetComponent<GizmoAxis>())
        {
            gizmoAxisOver = hitInfo.collider.gameObject.GetComponent<GizmoAxis>();
            gizmoAxisOver.SelectAxis(true);
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
}
