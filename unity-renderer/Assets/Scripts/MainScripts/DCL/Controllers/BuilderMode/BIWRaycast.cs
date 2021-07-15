using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using UnityEngine;

public class BIWRaycast : MonoBehaviour
{
    public Camera builderCamera;

    private const float RAYCAST_MAX_DISTANCE = 10000f;

    public LayerMask defaultMask { get; private set; }
    public LayerMask gizmoMask { get; private set; }

    private Plane groundPlane;
    private Plane entityHitPlane;

    private void Awake()
    {
        defaultMask = BIWSettings.COLLIDER_SELECTION_LAYER;
        gizmoMask = BIWSettings.GIZMOS_LAYER;

        groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (builderCamera == null)
            builderCamera = InitialSceneReferences.i.mainCamera;
    }

    public void SetEntityHitPlane(float height) { entityHitPlane = new Plane(Vector3.up, new Vector3(0, height, 0)); }

    public bool Raycast(Vector3 mousePosition, LayerMask mask, out RaycastHit hitInfo, bool checkGizmo = false)
    {
        if (checkGizmo)
        {
            if (Raycast(mousePosition, gizmoMask, out hitInfo))
            {
                return true;
            }
        }

        return Physics.Raycast(GetMouseRay(mousePosition), out hitInfo, RAYCAST_MAX_DISTANCE, mask);
    }

    public bool Raycast(Vector3 mousePosition, LayerMask mask, out RaycastHit hitInfo, System.Func<RaycastHit[], RaycastHit> hitComparer)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(GetMouseRay(mousePosition), RAYCAST_MAX_DISTANCE, mask);
        if (hits.Length > 0)
        {
            hitInfo = hitComparer(hits);
            return true;
        }
        hitInfo = new RaycastHit();
        return false;
    }

    public Ray GetMouseRay(Vector3 mousePosition) { return builderCamera.ScreenPointToRay(mousePosition); }

    public bool RaycastToGizmos(Ray ray, out RaycastHit hitInfo) { return Physics.Raycast(ray, out hitInfo, RAYCAST_MAX_DISTANCE, gizmoMask); }

    public bool RaycastToGizmos(Vector3 mousePosition, out RaycastHit hitInfo) { return RaycastToGizmos(GetMouseRay(mousePosition), out hitInfo); }

    public bool RaycastToGround(Vector3 mousePosition, out Vector3 hitPosition)
    {
        Ray ray = GetMouseRay(mousePosition);
        return RaycastToGround(ray, out hitPosition);
    }

    public bool RaycastToGround(Ray ray, out Vector3 hitPosition)
    {
        float raycastDistance = 0.0f;

        if (groundPlane.Raycast(ray, out raycastDistance))
        {
            hitPosition = ray.GetPoint(raycastDistance);
            return true;
        }
        hitPosition = Vector3.zero;
        return false;
    }

    public Vector3 RaycastToEntityHitPlane(Vector3 mousePosition)
    {
        Ray ray = GetMouseRay(mousePosition);
        float raycastDistance = 0.0f;

        if (entityHitPlane.Raycast(ray, out raycastDistance))
        {
            return ray.GetPoint(raycastDistance);
        }

        return Vector3.zero;
    }
}