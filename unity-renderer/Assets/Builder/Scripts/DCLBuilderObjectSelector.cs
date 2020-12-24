using UnityEngine;
using DCL.Controllers;
using Builder.Gizmos;
using System.Collections.Generic;

namespace Builder
{
    public class DCLBuilderObjectSelector : MonoBehaviour
    {
        const float DRAGGING_THRESHOLD_TIME = 0.25f;

        public DCLBuilderRaycast builderRaycast;
        public DCLBuilderGizmoManager gizmosManager;

        public delegate void EntitySelectedDelegate(EditableEntity entity, string gizmoType);

        public delegate void EntityDeselectedDelegate(EditableEntity entity);

        public delegate void EntitySelectedListChangedDelegate(Transform selectionParent, List<EditableEntity> selectedEntities);

        public static event EntitySelectedDelegate OnMarkObjectSelected;
        public static event EntitySelectedDelegate OnSelectedObject;
        public static event EntityDeselectedDelegate OnDeselectedObject;
        public static event System.Action OnNoObjectSelected;
        public static event EntitySelectedListChangedDelegate OnSelectedObjectListChanged;
        public static event System.Action<DCLBuilderEntity, Vector3> OnEntityPressed;
        public static event System.Action<DCLBuilderGizmoAxis> OnGizmosAxisPressed;

        public Transform selectedEntitiesParent { private set; get; }

        private Dictionary<string, DCLBuilderEntity> entities = new Dictionary<string, DCLBuilderEntity>();
        private List<EditableEntity> selectedEntities = new List<EditableEntity>();
        private EntityPressedInfo lastPressedEntityInfo = new EntityPressedInfo();
        private bool isDirty = false;
        private bool isSelectionTransformed = false;

        private float groundClickTime = 0;

        private bool isGameObjectActive = false;

        private SceneBoundsChecker boundsChecker;
        private ParcelScene currentScene;

        private void Awake()
        {
            DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
            SelectionParentCreate();
        }

        private void OnDestroy()
        {
            if (selectedEntitiesParent != null)
                Destroy(selectedEntitiesParent.gameObject);

            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderInput.OnMouseDown += OnMouseDown;
                DCLBuilderInput.OnMouseUp += OnMouseUp;
                DCLBuilderBridge.OnResetObject += OnResetObject;
                DCLBuilderBridge.OnEntityAdded += OnEntityAdded;
                DCLBuilderBridge.OnEntityRemoved += OnEntityRemoved;
                DCLBuilderBridge.OnSceneChanged += OnSceneChanged;
                DCLBuilderBridge.OnBuilderSelectEntity += OnBuilderSelectEntity;
                DCLBuilderGizmoManager.OnGizmoTransformObject += OnGizmoTransform;
                DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformEnded;
                DCLBuilderObjectDragger.OnDraggingObject += OnObjectsDrag;
                DCLBuilderObjectDragger.OnDraggingObjectEnd += OnObjectsDragEnd;
            }

            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderInput.OnMouseDown -= OnMouseDown;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderBridge.OnResetObject -= OnResetObject;
            DCLBuilderBridge.OnEntityAdded -= OnEntityAdded;
            DCLBuilderBridge.OnEntityRemoved -= OnEntityRemoved;
            DCLBuilderBridge.OnSceneChanged -= OnSceneChanged;
            DCLBuilderBridge.OnBuilderSelectEntity -= OnBuilderSelectEntity;
            DCLBuilderGizmoManager.OnGizmoTransformObject -= OnGizmoTransform;
            DCLBuilderGizmoManager.OnGizmoTransformObjectEnd -= OnGizmoTransformEnded;
            DCLBuilderObjectDragger.OnDraggingObject -= OnObjectsDrag;
            DCLBuilderObjectDragger.OnDraggingObjectEnd -= OnObjectsDragEnd;
        }

        private void Update()
        {
            if (!isDirty)
            {
                return;
            }

            isDirty = false;
            SelectionParentReset();
            OnSelectedObjectListChanged?.Invoke(selectedEntitiesParent, selectedEntities);
            if (selectedEntities.Count == 0)
            {
                OnNoObjectSelected?.Invoke();
            }
        }

        private void OnMouseDown(int buttonId, Vector3 mousePosition)
        {
            if (buttonId != 0)
            {
                return;
            }

            bool gizmoOrEntityPressed = false;

            RaycastHit hit;
            if (builderRaycast.Raycast(mousePosition, builderRaycast.defaultMask | builderRaycast.gizmoMask, out hit, CompareSelectionHit))
            {
                DCLBuilderGizmoAxis gizmosAxis = hit.collider.gameObject.GetComponent<DCLBuilderGizmoAxis>();
                if (gizmosAxis != null)
                {
                    OnGizmosAxisPressed?.Invoke(gizmosAxis);
                    gizmoOrEntityPressed = true;
                }
                else
                {
                    var builderSelectionCollider = hit.collider.gameObject.GetComponent<DCLBuilderSelectionCollider>();
                    DCLBuilderEntity pressedEntity = null;

                    if (builderSelectionCollider != null)
                    {
                        pressedEntity = builderSelectionCollider.ownerEntity;
                    }

                    if (pressedEntity != null && CanSelect(pressedEntity))
                    {
                        SetLastPressedEntity(pressedEntity, hit.point);
                        OnEntityPressed?.Invoke(pressedEntity, hit.point);
                        gizmoOrEntityPressed = true;
                    }
                }
            }

            if (gizmoOrEntityPressed)
            {
                groundClickTime = 0;
            }
            else
            {
                groundClickTime = Time.unscaledTime;
            }
        }

        private void OnMouseUp(int buttonId, Vector3 mousePosition)
        {
            if (buttonId != 0)
            {
                return;
            }

            if (lastPressedEntityInfo.pressedEntity != null)
            {
                // NOTE: we only process entity as selected if we are not considering that user was holding mouse button to rotate the camera
                if ((Time.unscaledTime - lastPressedEntityInfo.pressedTime) < DRAGGING_THRESHOLD_TIME)
                {
                    ProcessEntityPressed(lastPressedEntityInfo.pressedEntity, lastPressedEntityInfo.hitPoint);
                }
            }

            lastPressedEntityInfo.pressedEntity = null;

            // NOTE: deselect all entities if the user click on the ground and it wasn't holding the mouse left button
            if (groundClickTime != 0 && (Time.unscaledTime - groundClickTime) < DRAGGING_THRESHOLD_TIME)
            {
                if (selectedEntities != null)
                {
                    OnNoObjectSelected?.Invoke();
                }
            }

            groundClickTime = 0;
        }

        private void OnResetObject()
        {
            for (int i = 0; i < selectedEntities.Count; i++)
            {
                selectedEntities[i].transform.localRotation = Quaternion.identity;
            }
        }

        private void OnEntityAdded(DCLBuilderEntity entity)
        {
            if (!entities.ContainsKey(entity.rootEntity.entityId))
            {
                entities.Add(entity.rootEntity.entityId, entity);
            }
        }

        private void OnEntityRemoved(DCLBuilderEntity entity)
        {
            if (selectedEntities.Contains(entity))
            {
                Deselect(entity);
            }

            if (entities.ContainsKey(entity.rootEntity.entityId))
            {
                entities.Remove(entity.rootEntity.entityId);
            }
        }

        private void OnSceneChanged(ParcelScene scene)
        {
            boundsChecker = DCL.Environment.i.world.sceneBoundsChecker;
            currentScene = scene;
        }

        private void OnBuilderSelectEntity(string[] entitiesId)
        {
            List<EditableEntity> entitiesToDeselect = new List<EditableEntity>(selectedEntities);

            for (int i = 0; i < entitiesId.Length; i++)
            {
                if (entities.ContainsKey(entitiesId[i]))
                {
                    DCLBuilderEntity entity = entities[entitiesId[i]];
                    if (!SelectionParentHasChild(entity.transform))
                    {
                        Select(entity);
                    }
                    else
                    {
                        entitiesToDeselect.Remove(entity);
                    }
                }
            }

            for (int i = 0; i < entitiesToDeselect.Count; i++)
            {
                Deselect(entitiesToDeselect[i]);
            }
        }

        private void OnGizmoTransform(string gizmoType)
        {
            isSelectionTransformed = true;
        }

        private void OnGizmoTransformEnded(string gizmoType)
        {
            if (isSelectionTransformed)
            {
                SelectionParentReset();
            }

            isSelectionTransformed = false;
        }

        private void OnObjectsDrag()
        {
            isSelectionTransformed = true;
        }

        private void OnObjectsDragEnd()
        {
            if (isSelectionTransformed)
            {
                SelectionParentReset();
            }

            isSelectionTransformed = false;
        }

        private bool CanSelect(DCLBuilderEntity entity)
        {
            return entity.hasGizmoComponent;
        }

        private void MarkSelected(DCLBuilderEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            OnMarkObjectSelected?.Invoke(entity, gizmosManager.GetSelectedGizmo());
        }

        private void Select(DCLBuilderEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            if (!selectedEntities.Contains(entity))
            {
                selectedEntities.Add(entity);
            }

            SelectionParentAddEntity(entity);
            entity.SetSelectLayer();

            OnSelectedObject?.Invoke(entity, gizmosManager.GetSelectedGizmo());
            isDirty = true;
        }

        private void Deselect(EditableEntity entity)
        {
            if (entity != null)
            {
                SelectionParentRemoveEntity(entity);
                OnDeselectedObject?.Invoke(entity);
                entity.SetDefaultLayer();
            }

            if (selectedEntities.Contains(entity))
            {
                selectedEntities.Remove(entity);
            }

            isDirty = true;
        }

        private void DeselectAll()
        {
            for (int i = selectedEntities.Count - 1; i >= 0; i--)
            {
                Deselect(selectedEntities[i]);
            }

            SelectionParentRemoveAllEntities();
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            DeselectAll();
            gameObject.SetActive(!isPreview);
        }

        private void SetLastPressedEntity(DCLBuilderEntity pressedEntity, Vector3 hitPoint)
        {
            lastPressedEntityInfo.pressedEntity = pressedEntity;
            lastPressedEntityInfo.pressedTime = Time.unscaledTime;
            lastPressedEntityInfo.hitPoint = hitPoint;
        }

        private void ProcessEntityPressed(DCLBuilderEntity pressedEntity, Vector3 hitPoint)
        {
            if (CanSelect(pressedEntity))
            {
                MarkSelected(pressedEntity);
            }
        }

        private void SelectionParentCreate()
        {
            selectedEntitiesParent = new GameObject("BuilderSelectedEntitiesParent").GetComponent<Transform>();
        }

        private void SelectionParentReset()
        {
            if (selectedEntitiesParent.childCount == 0)
            {
                return;
            }

            Transform entitiyTransform = selectedEntitiesParent.GetChild(0);
            Vector3 min = entitiyTransform.position;
            Vector3 max = entitiyTransform.position;

            List<Transform> children = new List<Transform>(selectedEntitiesParent.childCount);

            children.Add(entitiyTransform);
            SelectionParentRemoveEntity(entitiyTransform);

            for (int i = selectedEntitiesParent.childCount - 1; i >= 0; i--)
            {
                entitiyTransform = selectedEntitiesParent.GetChild(i);
                if (entitiyTransform.position.x < min.x) min.x = entitiyTransform.position.x;
                if (entitiyTransform.position.y < min.y) min.y = entitiyTransform.position.y;
                if (entitiyTransform.position.z < min.z) min.z = entitiyTransform.position.z;
                if (entitiyTransform.position.x > max.x) max.x = entitiyTransform.position.x;
                if (entitiyTransform.position.y > max.y) max.y = entitiyTransform.position.y;
                if (entitiyTransform.position.z > max.z) max.z = entitiyTransform.position.z;

                children.Add(entitiyTransform);
                SelectionParentRemoveEntity(entitiyTransform);
            }

            selectedEntitiesParent.position = min + (max - min) * 0.5f;
            selectedEntitiesParent.localScale = Vector3.one;
            selectedEntitiesParent.rotation = Quaternion.identity;

            for (int i = 0; i < children.Count; i++)
            {
                SelectionParentAddEntity(children[i]);
            }
        }

        private void SelectionParentAddEntity(DCLBuilderEntity entity)
        {
            SelectionParentAddEntity(entity.transform);
        }

        private void SelectionParentAddEntity(Transform entityTransform)
        {
            entityTransform.SetParent(selectedEntitiesParent, true);
        }

        private void SelectionParentRemoveEntity(EditableEntity entity)
        {
            SelectionParentRemoveEntity(entity.transform);
        }

        private void SelectionParentRemoveEntity(Transform entityTransform)
        {
            entityTransform.SetParent(currentScene.transform, true);
        }

        private void SelectionParentRemoveAllEntities()
        {
            for (int i = selectedEntitiesParent.childCount - 1; i >= 0; i--)
            {
                SelectionParentRemoveEntity(selectedEntitiesParent.GetChild(i));
            }
        }

        private bool SelectionParentHasChild(Transform transform)
        {
            return transform.parent == selectedEntitiesParent;
        }

        private RaycastHit CompareSelectionHit(RaycastHit[] hits)
        {
            RaycastHit closestHit = hits[0];
            bool isHitASelectedObject = IsEntityHitAndSelected(closestHit);

            if (IsGizmoHit(closestHit)) // Gizmos has always priority
            {
                return closestHit;
            }

            RaycastHit hit;
            for (int i = 1; i < hits.Length; i++)
            {
                hit = hits[i];
                if (IsGizmoHit(hit)) // Gizmos has always priority
                {
                    return hit;
                }

                if (hit.distance < closestHit.distance)
                {
                    isHitASelectedObject = IsEntityHitAndSelected(hit);
                    closestHit = hit;
                }
                else if (hit.distance == closestHit.distance && !isHitASelectedObject)
                {
                    isHitASelectedObject = IsEntityHitAndSelected(hit);
                    closestHit = hit;
                }
            }

            return closestHit;
        }

        private bool IsGizmoHit(RaycastHit hit)
        {
            return hit.collider.gameObject.GetComponent<DCLBuilderGizmoAxis>() != null;
        }

        private bool IsEntityHitAndSelected(RaycastHit hit)
        {
            var collider = hit.collider.gameObject.GetComponent<DCLBuilderSelectionCollider>();
            if (collider != null)
            {
                return SelectionParentHasChild(collider.ownerEntity.transform);
            }

            return false;
        }

        private class EntityPressedInfo
        {
            public DCLBuilderEntity pressedEntity;
            public float pressedTime;
            public Vector3 hitPoint;
        }
    }
}