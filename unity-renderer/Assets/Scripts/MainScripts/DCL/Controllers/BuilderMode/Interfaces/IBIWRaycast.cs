using UnityEngine;

public interface IBIWRaycastController : IBIWController
{
    event System.Action<IBIWGizmosAxis> OnGizmosAxisPressed;
    bool RayCastFloor(out Vector3 position);
    Vector3 GetFloorPointAtMouse(Vector3 mousePosition);
    bool RaycastToGizmos(Vector3 mousePosition, out RaycastHit hitInfo);
    bool Raycast(Vector3 mousePosition, LayerMask mask, out RaycastHit hitInfo, System.Func<RaycastHit[], RaycastHit> hitComparer);
    Ray GetMouseRay(Vector3 mousePosition);
    BIWEntity GetEntityOnPointer();
}