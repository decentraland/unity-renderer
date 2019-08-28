using UnityEngine;

namespace Builder
{
    public class DCLBuilderRaycast : MonoBehaviour
    {
        public Camera builderCamera;

        const float raycastMaxDistance = 10000f;

        public LayerMask defaultMask { get; private set; }
        public LayerMask gizmoMask { get; private set; }
        public int selectionLayer { get; private set; }
        public int defaultLayer { get; private set; }

        Plane groundPlane;

        private void Awake()
        {
            defaultMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Selection");
            gizmoMask = LayerMask.GetMask("Gizmo");
            selectionLayer = LayerMask.NameToLayer("Selection");
            defaultLayer = LayerMask.NameToLayer("Default");

            groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        public bool Raycast(Vector3 mousePosition, LayerMask mask, out RaycastHit hitInfo, bool checkGizmo = false)
        {
            if (checkGizmo)
            {
                if (Raycast(mousePosition, gizmoMask, out hitInfo))
                {
                    return true;
                }
            }

            return Physics.Raycast(builderCamera.ScreenPointToRay(mousePosition), out hitInfo, raycastMaxDistance, mask);
        }

        public bool RaycastToGround(Vector3 mousePosition, out Vector3 hitPosition)
        {
            Ray ray = builderCamera.ScreenPointToRay(mousePosition);
            float enter = 0.0f;

            if (groundPlane.Raycast(ray, out enter))
            {
                hitPosition = ray.GetPoint(enter);
                return true;
            }
            hitPosition = Vector3.zero;
            return false;
        }
    }
}