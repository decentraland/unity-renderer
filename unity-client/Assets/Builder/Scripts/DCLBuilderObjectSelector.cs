using UnityEngine;
using DCL.Controllers;

namespace Builder
{
    public class DCLBuilderObjectSelector : MonoBehaviour
    {
        public DCLBuilderRaycast builderRaycast;
        public Gizmo[] gizmos;

        public delegate void GizmoTransformDelegate(DCLBuilderEntity entity, Vector3 position, string gizmoType);
        public delegate void DragDelegate(DCLBuilderEntity entity, Vector3 position);
        public delegate void EntitySelectedDelegate(DCLBuilderEntity entity, string gizmoType);
        public delegate void EntityDeselectedDelegate(DCLBuilderEntity entity);

        public static event EntitySelectedDelegate OnSelectedObject;
        public static event EntityDeselectedDelegate OnDeselectedObject;
        public static event DragDelegate OnDraggingObjectStart;
        public static event DragDelegate OnDraggingObject;
        public static event DragDelegate OnDraggingObjectEnd;
        public static event GizmoTransformDelegate OnGizmoTransformObjectStart;
        public static event GizmoTransformDelegate OnGizmoTransformObjectEnd;

        private DCLBuilderEntity selectedEntity = null;

        private DragInfo dragInfo = new DragInfo();
        private bool isGizmoTransformingObject = false;

        private float snapFactorPosition = 0;

        private Gizmo activeGizmo = null;
        private GizmoAxis gizmoAxis = null;
        private GizmoAxis gizmoAxisOver = null;

        private bool isGameObjectActive = false;

        private SceneBoundariesChecker boundariesChecker;

        private void Awake()
        {
            DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
        }

        private void OnDestroy()
        {
            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderInput.OnMouseDown += OnMouseDown;
                DCLBuilderInput.OnMouseDrag += OnMouseDrag;
                DCLBuilderInput.OnMouseUp += OnMouseUp;
                DCLBuilderBridge.OnSelectGizmo += OnSelectGizmo;
                DCLBuilderBridge.OnResetObject += OnResetObject;
                DCLBuilderBridge.OnEntityAdded += OnEntityAdded;
                DCLBuilderBridge.OnEntityRemoved += OnEntityRemoved;
                DCLBuilderBridge.OnSetGridResolution += OnSetGridResolution;
                DCLBuilderBridge.OnSceneChanged += OnSceneChanged;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderInput.OnMouseDown -= OnMouseDown;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderBridge.OnSelectGizmo -= OnSelectGizmo;
            DCLBuilderBridge.OnResetObject -= OnResetObject;
            DCLBuilderBridge.OnEntityAdded -= OnEntityAdded;
            DCLBuilderBridge.OnEntityRemoved -= OnEntityRemoved;
            DCLBuilderBridge.OnSetGridResolution -= OnSetGridResolution;
            DCLBuilderBridge.OnSceneChanged -= OnSceneChanged;
        }

        private void Update()
        {
            if (activeGizmo != null && !isGizmoTransformingObject)
            {
                CheckGizmoHover(Input.mousePosition);
            }
        }

        private void OnMouseDown(int buttonId, Vector3 mousePosition)
        {
            if (buttonId == 0)
            {
                RaycastHit hit;
                if (builderRaycast.Raycast(mousePosition, builderRaycast.defaultMask, out hit, true))
                {
                    gizmoAxis = hit.collider.gameObject.GetComponent<GizmoAxis>();
                    if (gizmoAxis != null)
                    {
                        gizmoAxis.SelectAxis(true);
                        isGizmoTransformingObject = true;
                        OnGizmoTransformObjectStart?.Invoke(selectedEntity, selectedEntity.transform.position, activeGizmo.gizmoType);
                    }
                    else
                    {
                        dragInfo.entity = hit.collider.gameObject.GetComponent<DCLBuilderEntity>();
                        if (dragInfo.entity != null)
                        {
                            if (CanSelect(dragInfo.entity))
                            {
                                if (dragInfo.entity != selectedEntity)
                                {
                                    Select(dragInfo.entity);
                                }

                                dragInfo.isDraggingObject = true;
                                builderRaycast.SetEntityHitPlane(hit.point.y);
                                dragInfo.hitToEntityOffset = dragInfo.entity.transform.position - hit.point;
                                OnDraggingObjectStart?.Invoke(dragInfo.entity, dragInfo.entity.transform.position);
                            }
                        }
                    }
                }
            }
        }

        private void OnMouseUp(int buttonId, Vector3 mousePosition)
        {
            if (buttonId == 0)
            {
                if (dragInfo.isDraggingObject && dragInfo.entity != null)
                {
                    OnDraggingObjectEnd?.Invoke(dragInfo.entity, dragInfo.entity.transform.position);
                }

                dragInfo.isDraggingObject = false;
                dragInfo.entity = null;

                if (isGizmoTransformingObject)
                {
                    isGizmoTransformingObject = false;
                    OnGizmoTransformObjectEnd?.Invoke(selectedEntity, selectedEntity.transform.position, activeGizmo.gizmoType);
                }

                if (gizmoAxis != null)
                {
                    gizmoAxis.SelectAxis(false);
                    gizmoAxis.ResetTransformation();
                    gizmoAxis = null;
                }
            }
        }

        private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 0)
            {
                bool hasMouseMoved = (axisX != 0 || axisY != 0);
                if (gizmoAxis != null)
                {
                    UpdateGizmoAxis(mousePosition);
                }
                else if (dragInfo.isDraggingObject && dragInfo.entity != null && hasMouseMoved)
                {
                    DragObject(dragInfo.entity, mousePosition);
                }
            }
        }

        private void OnSetGridResolution(float position, float rotation, float scale)
        {
            snapFactorPosition = position;
            for (int i = 0; i < gizmos.Length; i++)
            {
                gizmos[i].SetSnapFactor(position, rotation, scale);
            }
        }

        private void OnSelectGizmo(string gizmoType)
        {
            ActivateGizmo(activeGizmo, false);

            Gizmo gizmo = GetGizmo(gizmoType);

            if (gizmo != null)
            {
                ActivateGizmo(gizmo, true);
            }
        }

        private void OnResetObject()
        {

            if (selectedEntity != null)
            {
                selectedEntity.transform.localRotation = Quaternion.identity;

                if (activeGizmo != null)
                    activeGizmo.transform.localRotation = Quaternion.identity;
            }
        }

        private void OnEntityRemoved(DCLBuilderEntity entity)
        {
            if (selectedEntity == entity)
            {
                Deselect();
                if (activeGizmo != null)
                {
                    activeGizmo.SetObject(null);
                }
            }
        }

        private void OnEntityAdded(DCLBuilderEntity entity)
        {
            if (!dragInfo.isDraggingObject && !isGizmoTransformingObject && CanSelect(entity))
            {
                Select(entity);
            }
        }

        private void OnSceneChanged(ParcelScene scene)
        {
            boundariesChecker = scene.boundariesChecker;
        }

        private bool CanSelect(DCLBuilderEntity entity)
        {
            return entity.hasGizmoComponent;
        }

        private void Select(DCLBuilderEntity entity)
        {
            Deselect();

            selectedEntity = entity;
            if (activeGizmo != null)
            {
                ActivateGizmo(activeGizmo, true);
            }
            SelectionEffect(entity.gameObject);
            OnSelectedObject?.Invoke(entity, activeGizmo != null ? activeGizmo.gizmoType : DCL.Components.DCLGizmos.Gizmo.NONE);
        }

        private void Deselect(DCLBuilderEntity entity)
        {
            if (selectedEntity == entity)
            {
                OnDeselectedObject?.Invoke(entity);
                selectedEntity = null;
                if (activeGizmo != null && activeGizmo.gameObject.activeSelf)
                {
                    activeGizmo.gameObject.SetActive(false);
                }
                UnSelectionEffect(entity.gameObject);
            }
        }

        private void Deselect()
        {
            if (selectedEntity != null)
            {
                Deselect(selectedEntity);
            }
        }

        private void DragObject(DCLBuilderEntity entity, Vector3 mousePosition)
        {
            Vector3 hitPosition = builderRaycast.RaycastToEntityHitPlane(mousePosition);
            Vector3 newPosition = hitPosition + dragInfo.hitToEntityOffset;
            newPosition.y = entity.transform.position.y;

            if (snapFactorPosition > 0)
            {
                newPosition.x = newPosition.x - (newPosition.x % snapFactorPosition);
                newPosition.z = newPosition.z - (newPosition.z % snapFactorPosition);
            }

            entity.transform.position = newPosition;
            boundariesChecker?.EvaluateEntityPosition(selectedEntity.rootEntity);

            if (activeGizmo != null && !activeGizmo.transformWithObject)
            {
                activeGizmo.transform.position = entity.transform.position;
            }

            OnDraggingObject?.Invoke(entity, newPosition);
        }

        private void UpdateGizmoAxis(Vector3 mousePosition)
        {
            Vector3 hit;
            if (builderRaycast.RaycastToGround(mousePosition, out hit))
            {
                Camera builderCamera = builderRaycast.builderCamera;
                Vector3 pointerWorldPos = builderCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, builderCamera.nearClipPlane));
                gizmoAxis.UpdateTransformation(new Vector3(mousePosition.x, mousePosition.y, 0), pointerWorldPos, selectedEntity.gameObject, hit);
                boundariesChecker?.EvaluateEntityPosition(selectedEntity.rootEntity);
            }
        }

        private void ActivateGizmo(Gizmo gizmo, bool activate)
        {
            if (activate)
            {
                if (activeGizmo != null && activeGizmo != gizmo)
                {
                    ActivateGizmo(activeGizmo, false);
                }

                activeGizmo = gizmo;

                if (selectedEntity != null)
                {
                    gizmo.SetObject(selectedEntity.gameObject);
                }
            }
            else
            {
                activeGizmo = null;

                if (gizmo != null)
                {
                    gizmo.SetObject(null);
                }
            }
        }

        private Gizmo GetGizmo(string gizmoType)
        {
            for (int i = 0; i < gizmos.Length; i++)
            {
                Gizmo gizmo = gizmos[i];
                if (gizmo.gizmoType.CompareTo(gizmoType) == 0)
                    return gizmo;
            }

            return null;
        }

        private void CheckGizmoHover(Vector3 mousePosition)
        {
            RaycastHit hit;
            if (builderRaycast.RaycastToGizmos(mousePosition, out hit))
            {
                GizmoAxis gizmoAxis = hit.collider.gameObject.GetComponent<GizmoAxis>();

                if (gizmoAxis != null)
                {
                    if (gizmoAxisOver != gizmoAxis && gizmoAxisOver != null)
                    {
                        gizmoAxisOver.SelectAxis(false);
                    }
                    gizmoAxisOver = gizmoAxis;
                    gizmoAxisOver.SelectAxis(true);
                }
            }
            else if (gizmoAxisOver != null)
            {
                gizmoAxisOver.SelectAxis(false);
            }
        }
        private void SelectionEffect(GameObject gameObject)
        {
            gameObject.layer = builderRaycast.selectionLayer;
            ChangeLayersRecursively(gameObject.transform, builderRaycast.selectionLayer, builderRaycast.defaultLayer);
        }

        private void UnSelectionEffect(GameObject gameObject)
        {
            gameObject.layer = builderRaycast.defaultLayer;
            ChangeLayersRecursively(gameObject.transform, builderRaycast.defaultLayer, builderRaycast.selectionLayer);
        }

        private void ChangeLayersRecursively(Transform root, int layer, int currentLayer)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.gameObject.layer == currentLayer)
                {
                    child.gameObject.layer = layer;
                    ChangeLayersRecursively(child, layer, currentLayer);
                }
            }
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            Deselect();
            gameObject.SetActive(!isPreview);
        }

        private class DragInfo
        {
            public DCLBuilderEntity entity = null;
            public bool isDraggingObject = false;
            public Vector3 hitToEntityOffset = Vector3.zero;
        }
    }
}