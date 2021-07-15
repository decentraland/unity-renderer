using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using UnityEngine;

public interface IBIWRaycastController
{
    public event System.Action<BIWGizmosAxis> OnGizmosAxisPressed;
    public bool RaycastToGizmos(Vector3 mousePosition, out RaycastHit hitInfo);
    public bool Raycast(Vector3 mousePosition, LayerMask mask, out RaycastHit hitInfo, System.Func<RaycastHit[], RaycastHit> hitComparer);
    public Ray GetMouseRay(Vector3 mousePosition);
}

public class BIWRaycastController : BIWController, IBIWRaycastController
{
    public event System.Action<BIWGizmosAxis> OnGizmosAxisPressed;

    private Camera builderCamera;

    private const float RAYCAST_MAX_DISTANCE = 10000f;

    public LayerMask gizmoMask { get; private set; }

    public override void Init(BIWContext context)
    {
        base.Init(context);

        gizmoMask = BIWSettings.GIZMOS_LAYER;
        BIWInputWrapper.OnMouseDown += OnMouseDown;

        builderCamera = InitialSceneReferences.i.mainCamera;
    }

    public override void Dispose()
    {
        base.Dispose();
        BIWInputWrapper.OnMouseDown -= OnMouseDown;
    }

    private void OnMouseDown(int buttonId, Vector3 mousePosition)
    {
        if (buttonId != 0)
        {
            return;
        }
        CheckGizmosRaycast(mousePosition);
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

    #region Gizmos

    private void CheckGizmosRaycast(Vector3 mousePosition)
    {
        RaycastHit hit;
        if (Raycast(mousePosition, gizmoMask, out hit, CompareSelectionHit))
        {
            BIWGizmosAxis gizmosAxis = hit.collider.gameObject.GetComponent<BIWGizmosAxis>();
            if (gizmosAxis != null)
            {
                OnGizmosAxisPressed?.Invoke(gizmosAxis);
            }
        }
    }

    public bool RaycastToGizmos(Ray ray, out RaycastHit hitInfo) { return Physics.Raycast(ray, out hitInfo, RAYCAST_MAX_DISTANCE, gizmoMask); }

    public bool RaycastToGizmos(Vector3 mousePosition, out RaycastHit hitInfo) { return RaycastToGizmos(GetMouseRay(mousePosition), out hitInfo); }

    private RaycastHit CompareSelectionHit(RaycastHit[] hits)
    {
        RaycastHit closestHit = hits[0];

        if (IsGizmoHit(closestHit)) // Gizmos has always priority
        {
            return closestHit;
        }
        return closestHit;
    }

    private bool IsGizmoHit(RaycastHit hit) { return hit.collider.gameObject.GetComponent<BIWGizmosAxis>() != null; }

    #endregion

}