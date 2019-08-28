using UnityEngine;
using DCL.Models;

namespace Builder
{
    public class DCLBuilderObjectSelector : MonoBehaviour
    {
        public DCLBuilderRaycast builderRaycast;
        public Gizmo[] gizmos;

        public delegate void GizmoTransformDelegate(DecentralandEntity entity, Vector3 position, string gizmoType);
        public delegate void DragDelegate(DecentralandEntity entity, Vector3 position);
        public delegate void EntitySelectedDelegate(DecentralandEntity entity, string gizmoType);
        public delegate void EntityDeselectedDelegate(DecentralandEntity entity);

        public static event EntitySelectedDelegate OnSelectedObject;
        public static event EntityDeselectedDelegate OnDeselectedObject;
        public static event DragDelegate OnDraggingObjectStart;
        public static event DragDelegate OnDraggingObject;
        public static event DragDelegate OnDraggingObjectEnd;
        public static event GizmoTransformDelegate OnGizmoTransformObjectStart;
        public static event GizmoTransformDelegate OnGizmoTransformObjectEnd;

        private DecentralandEntity selectedEntity = null;
        private DecentralandEntity clickedEntity = null;

        private bool isDraggingObject = false;
        private bool canStartDragging = false;
        private bool isGizmoTransformingObject = false;
        private bool isSnapEnabled = true;

        private Gizmo activeGizmo = null;
        private GizmoAxis gizmoAxis = null;
        private GizmoAxis gizmoAxisOver = null;

        private bool isGameObjectActive = false;

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
                DCLBuilderInput.OnKeyboardButtonDown += OnKeyboardButtonDown;
                DCLBuilderInput.OnKeyboardButtonUp += OnKeyboardButtonUp;
                DCLBuilderBridge.OnSelectGizmo += OnSelectGizmo;
                DCLBuilderBridge.OnResetObject += OnResetObject;
                DCLBuilderBridge.OnEntityAdded += OnEntityAdded;
            }
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderInput.OnMouseDown -= OnMouseDown;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderInput.OnKeyboardButtonDown -= OnKeyboardButtonDown;
            DCLBuilderInput.OnKeyboardButtonUp -= OnKeyboardButtonUp;
            DCLBuilderBridge.OnSelectGizmo -= OnSelectGizmo;
            DCLBuilderBridge.OnResetObject -= OnResetObject;
            DCLBuilderBridge.OnEntityAdded -= OnEntityAdded;
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
                        OnGizmoTransformObjectStart?.Invoke(selectedEntity, selectedEntity.gameObject.transform.position, activeGizmo.gizmoType);
                    }
                    else
                    {
                        clickedEntity = DCLBuilderBridge.GetEntityFromGameObject(hit.collider.gameObject);
                        if (clickedEntity != null)
                        {
                            if (CanSelect(clickedEntity))
                            {
                                if (clickedEntity != selectedEntity)
                                {
                                    Select(clickedEntity);
                                }

                                isDraggingObject = true;
                                canStartDragging = false;
                                OnDraggingObjectStart?.Invoke(clickedEntity, clickedEntity.gameObject.transform.position);
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
                if (isDraggingObject && clickedEntity != null)
                {
                    OnDraggingObjectEnd?.Invoke(clickedEntity, selectedEntity.gameObject.transform.position);
                }

                isDraggingObject = false;
                clickedEntity = null;

                if (isGizmoTransformingObject)
                {
                    isGizmoTransformingObject = false;
                    OnGizmoTransformObjectEnd?.Invoke(selectedEntity, selectedEntity.gameObject.transform.position, activeGizmo.gizmoType);
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
                else if (isDraggingObject && clickedEntity != null && hasMouseMoved)
                {
                    DragObject(clickedEntity, mousePosition);
                }
            }
        }

        private void OnKeyboardButtonDown(KeyCode keyCode)
        {
            if (keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift)
            {
                isSnapEnabled = false;
            }
        }

        private void OnKeyboardButtonUp(KeyCode keyCode)
        {
            if (keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift)
            {
                isSnapEnabled = true;
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
                selectedEntity.gameObject.transform.localRotation = Quaternion.identity;

                if (activeGizmo != null)
                    activeGizmo.transform.localRotation = Quaternion.identity;
            }
        }

        private void OnEntityAdded(DecentralandEntity entity)
        {
            entity.OnShapeUpdated += OnEntityShapeUpdated;
        }

        private void OnEntityShapeUpdated(DecentralandEntity entity)
        {
            //TODO: avoid autoselecting scene ground
            if (!isDraggingObject && !isGizmoTransformingObject && CanSelect(entity))
            {
                Select(entity);
            }
            entity.OnShapeUpdated -= OnEntityShapeUpdated;
        }

        private bool CanSelect(DecentralandEntity entity)
        {
            return entity.components.ContainsKey(CLASS_ID_COMPONENT.GIZMOS);
        }

        private void Select(DecentralandEntity entity)
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

        private void Deselect(DecentralandEntity entity)
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

        private void DragObject(DecentralandEntity entity, Vector3 mousePosition)
        {
            Vector3 hit;
            if (builderRaycast.RaycastToGround(mousePosition, out hit))
            {
                Vector3 hitPos = new Vector3(hit.x, entity.gameObject.transform.position.y, hit.z);
                Vector3 newPosition = hitPos;

                if (!canStartDragging)
                {
                    const float sqMoveThreshold = 4 * 4;
                    if ((newPosition - entity.gameObject.transform.position).sqrMagnitude <= sqMoveThreshold)
                    {
                        canStartDragging = true;
                    }
                }
                else
                {
                    if (isSnapEnabled)
                    {
                        newPosition = new Vector3(Mathf.RoundToInt(hitPos.x), hitPos.y, Mathf.RoundToInt(hitPos.z));
                    }

                    entity.gameObject.transform.position = newPosition;

                    if (activeGizmo != null && !activeGizmo.transformWithObject)
                    {
                        activeGizmo.transform.position = entity.gameObject.transform.position;
                    }

                    OnDraggingObject?.Invoke(entity, newPosition);
                }
            }
        }

        private void UpdateGizmoAxis(Vector3 mousePosition)
        {
            Vector3 hit;
            if (builderRaycast.RaycastToGround(mousePosition, out hit))
            {
                Camera builderCamera = builderRaycast.builderCamera;
                Vector3 pointerWorldPos = builderCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, builderCamera.nearClipPlane));
                gizmoAxis.UpdateTransformation(new Vector3(mousePosition.x, mousePosition.y, 0), pointerWorldPos, selectedEntity.gameObject, hit);
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
            if (builderRaycast.Raycast(mousePosition, builderRaycast.gizmoMask, out hit))
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
    }
}