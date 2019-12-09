using UnityEngine;
using DCL.Controllers;
using Builder.Gizmos;

namespace Builder
{
    public class DCLBuilderObjectSelector : MonoBehaviour
    {
        public DCLBuilderRaycast builderRaycast;
        public DCLBuilderGizmoManager gizmosManager;

        public delegate void DragDelegate(DCLBuilderEntity entity, Vector3 position);
        public delegate void EntitySelectedDelegate(DCLBuilderEntity entity, string gizmoType);
        public delegate void EntityDeselectedDelegate(DCLBuilderEntity entity);

        public static event EntitySelectedDelegate OnSelectedObject;
        public static event EntityDeselectedDelegate OnDeselectedObject;
        public static event System.Action OnNoObjectSelected;
        public static event DragDelegate OnDraggingObjectStart;
        public static event DragDelegate OnDraggingObject;
        public static event DragDelegate OnDraggingObjectEnd;

        private DCLBuilderEntity selectedEntity = null;

        private DragInfo dragInfo = new DragInfo();
        private float groundClickTime = 0;

        private float snapFactorPosition = 0;

        private bool isGameObjectActive = false;

        private SceneBoundariesChecker boundariesChecker;
        private ParcelScene currentScene;

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
                DCLBuilderBridge.OnResetObject += OnResetObject;
                DCLBuilderBridge.OnEntityRemoved += OnEntityRemoved;
                DCLBuilderBridge.OnSetGridResolution += OnSetGridResolution;
                DCLBuilderBridge.OnSceneChanged += OnSceneChanged;
                DCLBuilderBridge.OnBuilderSelectEntity += OnBuilderSelectEntity;
                DCLBuilderBridge.OnBuilderDeselectEntity += OnBuilderDeselectEntity;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderInput.OnMouseDown -= OnMouseDown;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderBridge.OnResetObject -= OnResetObject;
            DCLBuilderBridge.OnEntityRemoved -= OnEntityRemoved;
            DCLBuilderBridge.OnSetGridResolution -= OnSetGridResolution;
            DCLBuilderBridge.OnSceneChanged -= OnSceneChanged;
            DCLBuilderBridge.OnBuilderSelectEntity -= OnBuilderSelectEntity;
            DCLBuilderBridge.OnBuilderDeselectEntity -= OnBuilderDeselectEntity;
        }

        private void Update()
        {
            if (!gizmosManager.isTransformingObject)
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
                    DCLBuilderGizmoAxis gizmosAxis = hit.collider.gameObject.GetComponent<DCLBuilderGizmoAxis>();
                    if (gizmosAxis != null)
                    {
                        gizmosManager.OnBeginDrag(gizmosAxis, selectedEntity);
                    }
                    else
                    {
                        var builderSelectionCollider = hit.collider.gameObject.GetComponent<DCLBuilderSelectionCollider>();
                        if (builderSelectionCollider != null)
                        {
                            dragInfo.entity = builderSelectionCollider.ownerEntity;
                        }

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
                    groundClickTime = 0;
                }
                else
                {
                    groundClickTime = Time.unscaledTime;
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

                if (gizmosManager.isTransformingObject)
                {
                    gizmosManager.OnEndDrag();
                }

                if (groundClickTime != 0 && (Time.unscaledTime - groundClickTime) < 0.25f)
                {
                    if (selectedEntity != null)
                    {
                        OnNoObjectSelected?.Invoke();
                    }
                    Deselect();
                }
                groundClickTime = 0;
            }
        }

        private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 0)
            {
                bool hasMouseMoved = (axisX != 0 || axisY != 0);
                if (gizmosManager.isTransformingObject)
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
        }

        private void OnResetObject()
        {
            if (selectedEntity != null)
            {
                selectedEntity.transform.localRotation = Quaternion.identity;
            }
        }

        private void OnEntityRemoved(DCLBuilderEntity entity)
        {
            if (selectedEntity == entity)
            {
                Deselect();
                dragInfo.isDraggingObject = false;
                dragInfo.entity = null;
                OnNoObjectSelected?.Invoke();
            }
        }

        private void OnSceneChanged(ParcelScene scene)
        {
            boundariesChecker = scene.boundariesChecker;
            currentScene = scene;
        }

        private void OnBuilderSelectEntity(string entityId)
        {
            if (currentScene && currentScene.entities.ContainsKey(entityId))
            {
                DCLBuilderEntity entity = currentScene.entities[entityId].gameObject.GetComponent<DCLBuilderEntity>();
                if (entity && !dragInfo.isDraggingObject && !gizmosManager.isTransformingObject && CanSelect(entity))
                {
                    entity.SetOnShapeLoaded(() =>
                    {
                        Select(entity);
                    });
                }
            }
        }

        private void OnBuilderDeselectEntity()
        {
            Deselect();
        }

        private bool CanSelect(DCLBuilderEntity entity)
        {
            return entity.hasGizmoComponent;
        }

        private void Select(DCLBuilderEntity entity)
        {
            Deselect();
            if (entity != null)
            {
                selectedEntity = entity;
                selectedEntity.SetSelectLayer();

                OnSelectedObject?.Invoke(entity, gizmosManager.GetSelectedGizmo());
            }
        }

        private void Deselect(DCLBuilderEntity entity)
        {
            if (selectedEntity == entity)
            {
                OnDeselectedObject?.Invoke(entity);
                if (entity != null)
                {
                    entity.SetDefaultLayer();
                }
                selectedEntity = null;
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

            OnDraggingObject?.Invoke(entity, newPosition);
        }

        private void UpdateGizmoAxis(Vector3 mousePosition)
        {
            Vector3 hit;
            if (gizmosManager.RaycastHit(builderRaycast.GetMouseRay(mousePosition), out hit))
            {
                gizmosManager.OnDrag(hit, mousePosition);
                boundariesChecker?.EvaluateEntityPosition(selectedEntity.rootEntity);
            }
        }

        private void CheckGizmoHover(Vector3 mousePosition)
        {
            RaycastHit hit;
            if (builderRaycast.RaycastToGizmos(mousePosition, out hit))
            {
                DCLBuilderGizmoAxis gizmoAxis = hit.collider.gameObject.GetComponent<DCLBuilderGizmoAxis>();
                gizmosManager.SetAxisHover(gizmoAxis);
            }
            else
            {
                gizmosManager.SetAxisHover(null);
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