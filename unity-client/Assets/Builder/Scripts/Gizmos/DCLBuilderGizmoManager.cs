using UnityEngine;

namespace Builder.Gizmos
{
    public class DCLBuilderGizmoManager : MonoBehaviour
    {
        public delegate void GizmoTransformDelegate(DCLBuilderEntity entity, Vector3 position, string gizmoType);

        public static event GizmoTransformDelegate OnGizmoTransformObjectStart;
        public static event GizmoTransformDelegate OnGizmoTransformObjectEnd;

        [SerializeField] private DCLBuilderGizmo[] gizmos = null;

        public bool isTransformingObject { private set; get; }
        public DCLBuilderGizmo activeGizmo { private set; get; }

        private DCLBuilderEntity targetEntity;

        private SnapInfo snapInfo = new SnapInfo();

        private bool isGameObjectActive = false;
        private bool isGizmosInitialized = false;

        private DCLBuilderGizmoAxis hoveredAxis = null;

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

        public void OnBeginDrag(DCLBuilderGizmoAxis hittedAxis, DCLBuilderEntity entity)
        {
            isTransformingObject = true;
            activeGizmo = hittedAxis.GetGizmo();
            targetEntity = entity;
            activeGizmo.OnBeginDrag(hittedAxis, entity.transform);
            OnGizmoTransformObjectStart?.Invoke(entity, entity.transform.position, activeGizmo.GetGizmoType());
        }

        public void OnDrag(Vector3 hitPoint, Vector2 mousePosition)
        {
            activeGizmo.OnDrag(hitPoint, mousePosition);
        }

        public void OnEndDrag()
        {
            activeGizmo.OnEndDrag();
            OnGizmoTransformObjectEnd?.Invoke(targetEntity, targetEntity.transform.position, activeGizmo.GetGizmoType());
            isTransformingObject = false;
        }

        public void SetAxisHover(DCLBuilderGizmoAxis axis)
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

        public void ShowGizmo(Transform entityTransform)
        {
            if (activeGizmo != null)
            {
                activeGizmo.SetTargetTransform(entityTransform);
                activeGizmo.gameObject.SetActive(true);
            }
        }

        public void HideGizmo()
        {
            if (activeGizmo != null)
            {
                activeGizmo.gameObject.SetActive(false);
            }
        }

        public bool IsGizmoShowing()
        {
            return IsGizmoActive() && activeGizmo.gameObject.activeInHierarchy;
        }

        public bool IsGizmoActive()
        {
            return activeGizmo != null;
        }

        public string GetSelectedGizmo()
        {
            if (activeGizmo != null)
            {
                return activeGizmo.GetGizmoType();
            }
            return DCL.Components.DCLGizmos.Gizmo.NONE;
        }

        public bool RaycastHit(Ray ray, out Vector3 hitPoint)
        {
            if (activeGizmo != null)
            {
                return activeGizmo.RaycastHit(ray, out hitPoint);
            }
            hitPoint = Vector3.zero;
            return false;
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderBridge.OnSelectGizmo += SetGizmoType;
                DCLBuilderBridge.OnEntityRemoved += OnEntityRemoved;
                DCLBuilderBridge.OnSetGridResolution += OnSetGridResolution;
                DCLBuilderCamera.OnCameraZoomChanged += OnCameraZoomChanged;
                DCLBuilderObjectSelector.OnSelectedObject += OnEntitySelected;
                DCLBuilderObjectSelector.OnDeselectedObject += OnEntityDeselected;
                isGameObjectActive = true;
            }
        }

        private void OnDisable()
        {
            DCLBuilderBridge.OnSelectGizmo -= SetGizmoType;
            DCLBuilderBridge.OnEntityRemoved -= OnEntityRemoved;
            DCLBuilderBridge.OnSetGridResolution -= OnSetGridResolution;
            DCLBuilderCamera.OnCameraZoomChanged -= OnCameraZoomChanged;
            DCLBuilderObjectSelector.OnSelectedObject -= OnEntitySelected;
            DCLBuilderObjectSelector.OnDeselectedObject -= OnEntityDeselected;
            isGameObjectActive = false;
        }

        private void SetGizmoType(string gizmoType)
        {
            if (gizmoType == DCL.Components.DCLGizmos.Gizmo.NONE)
            {
                if (IsGizmoShowing())
                {
                    HideGizmo();
                }
                activeGizmo = null;
            }
            else
            {
                if (IsGizmoShowing() && activeGizmo.GetGizmoType() != gizmoType)
                {
                    HideGizmo();
                    activeGizmo = null;
                }
                for (int i = 0; i < gizmos.Length; i++)
                {
                    if (gizmos[i].GetGizmoType() == gizmoType)
                    {
                        activeGizmo = gizmos[i];
                        activeGizmo.SetSnapFactor(snapInfo);
                        break;
                    }
                }
            }
            if (activeGizmo != null && targetEntity != null)
            {
                ShowGizmo(targetEntity.transform);
            }
        }

        private void InitializeGizmos(Camera camera)
        {
            if (!isGizmosInitialized)
            {
                for (int i = 0; i < gizmos.Length; i++)
                {
                    if (!gizmos[i].initialized)
                    {
                        gizmos[i].Initialize(camera);
                    }
                }
                isGizmosInitialized = true;
            }
        }

        private void OnCameraZoomChanged(Camera camera, float zoom)
        {
            InitializeGizmos(camera);
        }

        private void OnEntitySelected(DCLBuilderEntity entity, string gizmoType)
        {
            targetEntity = entity;
            ShowGizmo(targetEntity.transform);
        }

        private void OnEntityRemoved(DCLBuilderEntity entity)
        {
            if (targetEntity == entity)
            {
                if (IsGizmoActive())
                {
                    HideGizmo();
                }
                isTransformingObject = false;
                targetEntity = null;
            }
        }

        private void OnEntityDeselected(DCLBuilderEntity entity)
        {
            if (targetEntity == entity)
            {
                if (IsGizmoActive())
                {
                    HideGizmo();
                }
                isTransformingObject = false;
                targetEntity = null;
            }
        }

        private void OnSetGridResolution(float position, float rotation, float scale)
        {
            SetSnapFactor(position, rotation, scale);
        }

        public class SnapInfo
        {
            public float position = 0;
            public float rotation = 0;
            public float scale = 0;
        }
    }
}