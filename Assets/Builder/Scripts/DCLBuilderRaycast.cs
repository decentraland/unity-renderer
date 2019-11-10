using UnityEngine;
using DCL.Controllers;
using DCL.Components;

namespace Builder
{
    public class DCLBuilderRaycast : MonoBehaviour
    {
        public Camera builderCamera;

        private const float RAYCAST_MAX_DISTANCE = 10000f;

        public LayerMask defaultMask { get; private set; }
        public LayerMask gizmoMask { get; private set; }
        public int selectionLayer { get; private set; }
        public int defaultLayer { get; private set; }

        private Plane groundPlane;
        private Plane hitPlane;

        private ParcelScene currentScene;
        private bool isGameObjectActive = false;

        private void Awake()
        {
            defaultMask = LayerMask.GetMask(OnPointerEventColliders.COLLIDER_LAYER) | LayerMask.GetMask("Selection");
            gizmoMask = LayerMask.GetMask("Gizmo");
            selectionLayer = LayerMask.NameToLayer("Selection");
            defaultLayer = LayerMask.NameToLayer(OnPointerEventColliders.COLLIDER_LAYER);

            groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        public void SetEntityHitPlane(float height)
        {
            hitPlane = new Plane(Vector3.up, new Vector3(0, height, 0));
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

            return Physics.Raycast(GetMouseRay(mousePosition), out hitInfo, RAYCAST_MAX_DISTANCE, mask);
        }

        public Ray GetMouseRay(Vector3 mousePosition)
        {
            return builderCamera.ScreenPointToRay(mousePosition);
        }

        public bool RaycastToGizmos(Ray ray, out RaycastHit hitInfo)
        {
            return Physics.Raycast(ray, out hitInfo, RAYCAST_MAX_DISTANCE, gizmoMask);
        }

        public bool RaycastToGizmos(Vector3 mousePosition, out RaycastHit hitInfo)
        {
            return RaycastToGizmos(GetMouseRay(mousePosition), out hitInfo);
        }

        public bool RaycastToGround(Vector3 mousePosition, out Vector3 hitPosition)
        {
            Ray ray = GetMouseRay(mousePosition);
            float enter = 0.0f;

            if (groundPlane.Raycast(ray, out enter))
            {
                hitPosition = ray.GetPoint(enter);
                return true;
            }
            hitPosition = Vector3.zero;
            return false;
        }

        public Vector3 RaycastToEntityHitPlane(Vector3 mousePosition)
        {
            Ray ray = GetMouseRay(mousePosition);
            float enter = 0.0f;

            if (hitPlane.Raycast(ray, out enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderBridge.OnSceneChanged += OnSceneChanged;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderBridge.OnSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(ParcelScene scene)
        {
            currentScene = scene;
        }
    }
}