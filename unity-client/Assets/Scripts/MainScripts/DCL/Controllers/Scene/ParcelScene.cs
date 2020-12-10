using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using DCL.Controllers.ParcelSceneDebug;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Controllers
{
    public class ParcelScene : MonoBehaviour
    {
        public static bool VERBOSE = false;

        public enum State
        {
            NOT_READY,
            WAITING_FOR_INIT_MESSAGES,
            WAITING_FOR_COMPONENTS,
            READY,
        }

        public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();
        public Dictionary<string, BaseDisposable> disposableComponents = new Dictionary<string, BaseDisposable>();
        public LoadParcelScenesMessage.UnityParcelScene sceneData { get; protected set; }
        public HashSet<Vector2Int> parcels = new HashSet<Vector2Int>();
        public SceneController ownerController;
        public SceneMetricsController metricsController;
        public UIScreenSpace uiScreenSpace;

        public event System.Action<DecentralandEntity> OnEntityAdded;
        public event System.Action<DecentralandEntity> OnEntityRemoved;
        public event System.Action<ParcelScene> OnSceneReady;
        public event System.Action<ParcelScene> OnStateRefreshed;

        public ContentProvider contentProvider;
        public int disposableNotReadyCount => disposableNotReady.Count;

        [System.NonSerialized] public bool isTestScene = false;

        [System.NonSerialized] public bool isPersistent = false;

        [System.NonSerialized] public bool unloadWithDistance = true;

        public bool isReady => state == State.READY;

        readonly List<string> disposableNotReady = new List<string>();
        bool isReleased = false, isEditModeActive = false;

        SceneDebugPlane sceneDebugPlane = null;

        State stateValue = State.NOT_READY;

        public State state
        {
            get { return stateValue; }
            set
            {
                stateValue = value;
                OnStateRefreshed?.Invoke(this);
            }
        }

        public void Awake()
        {
            state = State.NOT_READY;

            if (DCLCharacterController.i)
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust += OnPrecisionAdjust;

            metricsController = new SceneMetricsController(this);
            metricsController.Enable();
        }

        void OnDisable()
        {
            metricsController.Disable();
        }

        private void Update()
        {
            if (state == State.READY && CommonScriptableObjects.rendererState.Get())
                SendMetricsEvent();
        }

        protected virtual string prettyName => sceneData.basePosition.ToString();

        protected void RefreshName()
        {
#if UNITY_EDITOR
            switch (state)
            {
                case State.NOT_READY:
                    this.name = gameObject.name = $"scene:{prettyName} - not ready...";
                    break;
                case State.WAITING_FOR_INIT_MESSAGES:
                    this.name = gameObject.name = $"scene:{prettyName} - waiting for init messages...";
                    break;
                case State.WAITING_FOR_COMPONENTS:

                    if (disposableComponents != null && disposableComponents.Count > 0)
                        this.name = gameObject.name = $"scene:{prettyName} - left to ready:{disposableComponents.Count - disposableNotReadyCount}/{disposableComponents.Count}";
                    else
                        this.name = gameObject.name = $"scene:{prettyName} - no components. waiting...";

                    break;
                case State.READY:
                    this.name = gameObject.name = $"scene:{prettyName} - ready!";
                    break;
            }
#endif
        }

        public void SetEditMode(bool isActive)
        {
            isEditModeActive = isActive;
        }

        public bool IsEditModeActive()
        {
            return isEditModeActive;
        }

        public virtual void SetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            this.sceneData = data;

            contentProvider = new ContentProvider();
            contentProvider.baseUrl = data.baseUrl;
            contentProvider.contents = data.contents;
            contentProvider.BakeHashes();

            state = State.WAITING_FOR_INIT_MESSAGES;
            RefreshName();

            parcels.Clear();
            for (int i = 0; i < sceneData.parcels.Length; i++)
            {
                parcels.Add(sceneData.parcels[i]);
            }

            if (DCLCharacterController.i != null)
                gameObject.transform.position = DCLCharacterController.i.characterPosition.WorldToUnityPosition(Utils.GridToWorldPosition(data.basePosition.x, data.basePosition.y));

#if UNITY_EDITOR
            DebugConfig debugConfig = DataStore.debugConfig;
            //NOTE(Brian): Don't generate parcel blockers if debugScenes is active and is not the desired scene.
            if (debugConfig.soloScene && debugConfig.soloSceneCoords != data.basePosition)
            {
                SetSceneReady();
                return;
            }
#endif

            if (isTestScene)
                SetSceneReady();
        }

        void OnPrecisionAdjust(DCLCharacterPosition position)
        {
            gameObject.transform.position = position.WorldToUnityPosition(Utils.GridToWorldPosition(sceneData.basePosition.x, sceneData.basePosition.y));
        }

        public virtual void SetUpdateData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            contentProvider = new ContentProvider();
            contentProvider.baseUrl = data.baseUrl;
            contentProvider.contents = data.contents;
            contentProvider.BakeHashes();
        }

        public void InitializeDebugPlane()
        {
            if (EnvironmentSettings.DEBUG && sceneData.parcels != null && sceneDebugPlane == null)
            {
                sceneDebugPlane = new SceneDebugPlane(sceneData, gameObject.transform);
            }
        }

        public void RemoveDebugPlane()
        {
            if (sceneDebugPlane != null)
            {
                sceneDebugPlane.Dispose();
                sceneDebugPlane = null;
            }
        }

        public void Cleanup(bool immediate)
        {
            if (isReleased)
                return;

            if (sceneDebugPlane != null)
            {
                sceneDebugPlane.Dispose();
                sceneDebugPlane = null;
            }

            DisposeAllSceneComponents();

            if (DCLCharacterController.i)
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= OnPrecisionAdjust;

            if (immediate) //!CommonScriptableObjects.rendererState.Get())
            {
                RemoveAllEntitiesImmediate();
            }
            else
            {
                if (entities.Count > 0)
                {
                    this.gameObject.transform.position = EnvironmentSettings.MORDOR;
                    this.gameObject.SetActive(false);

                    RemoveAllEntities();
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }

            isReleased = true;
        }

        public override string ToString()
        {
            return "Parcel Scene: " + base.ToString() + "\n" + sceneData.ToString();
        }

        public bool IsInsideSceneBoundaries(DCLCharacterPosition charPosition)
        {
            return IsInsideSceneBoundaries(Utils.WorldToGridPosition(charPosition.worldPosition));
        }

        public bool IsInsideSceneBoundaries(Bounds objectBounds)
        {
            if (!IsInsideSceneBoundaries(objectBounds.min + CommonScriptableObjects.playerUnityToWorldOffset, objectBounds.max.y)) return false;
            if (!IsInsideSceneBoundaries(objectBounds.max + CommonScriptableObjects.playerUnityToWorldOffset, objectBounds.max.y)) return false;

            return true;
        }

        public virtual bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0f)
        {
            if (parcels.Count == 0) return false;

            float heightLimit = metricsController.GetLimits().sceneHeight;
            if (height > heightLimit) return false;

            return parcels.Contains(gridPosition);
        }

        public virtual bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f)
        {
            if (parcels.Count == 0) return false;

            float heightLimit = metricsController.GetLimits().sceneHeight;
            if (height > heightLimit) return false;

            int noThresholdZCoordinate = Mathf.FloorToInt(worldPosition.z / ParcelSettings.PARCEL_SIZE);
            int noThresholdXCoordinate = Mathf.FloorToInt(worldPosition.x / ParcelSettings.PARCEL_SIZE);

            // We check the target world position
            Vector2Int targetCoordinate = new Vector2Int(noThresholdXCoordinate, noThresholdZCoordinate);
            if (parcels.Contains(targetCoordinate)) return true;

            // We need to check using a threshold from the target point, in order to cover correctly the parcel "border/edge" positions
            Vector2Int coordinateMin = new Vector2Int();
            coordinateMin.x = Mathf.FloorToInt((worldPosition.x - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);
            coordinateMin.y = Mathf.FloorToInt((worldPosition.z - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);

            Vector2Int coordinateMax = new Vector2Int();
            coordinateMax.x = Mathf.FloorToInt((worldPosition.x + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);
            coordinateMax.y = Mathf.FloorToInt((worldPosition.z + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);

            // We check the east/north-threshold position
            targetCoordinate.Set(coordinateMax.x, coordinateMax.y);
            if (parcels.Contains(targetCoordinate)) return true;

            // We check the east/south-threshold position
            targetCoordinate.Set(coordinateMax.x, coordinateMin.y);
            if (parcels.Contains(targetCoordinate)) return true;

            // We check the west/north-threshold position
            targetCoordinate.Set(coordinateMin.x, coordinateMax.y);
            if (parcels.Contains(targetCoordinate)) return true;

            // We check the west/south-threshold position
            targetCoordinate.Set(coordinateMin.x, coordinateMin.y);
            if (parcels.Contains(targetCoordinate)) return true;

            return false;
        }

        public DecentralandEntity CreateEntity(string id)
        {
            if (entities.ContainsKey(id))
            {
                return entities[id];
            }

            var newEntity = new DecentralandEntity();
            newEntity.entityId = id;

            SceneController.i.EnsureEntityPool();

            // As we know that the pool already exists, we just get one gameobject from it
            PoolableObject po = PoolManager.i.Get(SceneController.EMPTY_GO_POOL_NAME);

            newEntity.meshesInfo.innerGameObject = po.gameObject;
            newEntity.gameObject = po.gameObject;

#if UNITY_EDITOR
            newEntity.gameObject.name = "ENTITY_" + id;
#endif
            newEntity.gameObject.transform.SetParent(gameObject.transform, false);
            newEntity.gameObject.SetActive(true);
            newEntity.scene = this;

            newEntity.OnCleanupEvent += po.OnCleanup;

            if (Environment.i.sceneBoundsChecker.enabled)
                newEntity.OnShapeUpdated += Environment.i.sceneBoundsChecker.AddEntityToBeChecked;

            entities.Add(id, newEntity);

            OnEntityAdded?.Invoke(newEntity);

            return newEntity;
        }

        public DecentralandEntity DuplicateEntity(DecentralandEntity decentralandEntity)
        {
            if (!entities.ContainsKey(decentralandEntity.entityId)) return null;

            DecentralandEntity duplicatedEntity = CreateEntity(System.Guid.NewGuid().ToString());

            if (decentralandEntity.children.Count > 0)
            {
                using (var iterator = decentralandEntity.children.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        DecentralandEntity childDuplicate = DuplicateEntity(iterator.Current.Value);
                        childDuplicate.SetParent(duplicatedEntity);
                    }
                }
            }

            if (decentralandEntity.parent != null) SetEntityParent(duplicatedEntity.entityId, decentralandEntity.parent.entityId);

            DCLTransform.model.position = Environment.i.worldState.ConvertUnityToScenePosition(decentralandEntity.gameObject.transform.position);
            DCLTransform.model.rotation = decentralandEntity.gameObject.transform.rotation;
            DCLTransform.model.scale = decentralandEntity.gameObject.transform.lossyScale;

            foreach (KeyValuePair<CLASS_ID_COMPONENT, BaseComponent> component in decentralandEntity.components)
            {
                EntityComponentCreateOrUpdateFromUnity(duplicatedEntity.entityId, component.Key, DCLTransform.model);
            }

            foreach (KeyValuePair<System.Type, BaseDisposable> component in decentralandEntity.GetSharedComponents())
            {
                SharedComponentAttach(duplicatedEntity.entityId, component.Value.id);
            }

            //TODO: (Adrian) Evaluate if all created components should be handle as equals instead of different
            foreach (KeyValuePair<string, UUIDComponent> component in decentralandEntity.uuidComponents)
            {
                EntityComponentCreateOrUpdateFromUnity(duplicatedEntity.entityId, CLASS_ID_COMPONENT.UUID_CALLBACK, component.Value.model);
            }

            return duplicatedEntity;
        }

        public void RemoveEntity(string id, bool removeImmediatelyFromEntitiesList = true)
        {
            if (entities.ContainsKey(id))
            {
                DecentralandEntity entity = entities[id];

                if (!entity.markedForCleanup)
                {
                    // This will also cleanup its children
                    CleanUpEntityRecursively(entity, removeImmediatelyFromEntitiesList);
                }

                entities.Remove(id);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError($"Couldn't remove entity with ID: {id} as it doesn't exist.");
            }
#endif
        }

        void CleanUpEntityRecursively(DecentralandEntity entity, bool removeImmediatelyFromEntitiesList)
        {
            // Iterate through all entity children
            using (var iterator = entity.children.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    CleanUpEntityRecursively(iterator.Current.Value, removeImmediatelyFromEntitiesList);
                }
            }

            OnEntityRemoved?.Invoke(entity);

            if (Environment.i.sceneBoundsChecker.enabled)
            {
                entity.OnShapeUpdated -= Environment.i.sceneBoundsChecker.AddEntityToBeChecked;
                Environment.i.sceneBoundsChecker.RemoveEntityToBeChecked(entity);
            }

            if (removeImmediatelyFromEntitiesList)
            {
                // Every entity ends up being removed through here
                entity.Cleanup();
                entities.Remove(entity.entityId);
            }
            else
            {
                Environment.i.parcelScenesCleaner.MarkForCleanup(entity);
            }
        }

        void RemoveAllEntities(bool instant = false)
        {
            //NOTE(Brian): We need to remove only the rootEntities.
            //             If we don't, duplicated entities will get removed when destroying
            //             recursively, making this more complicated than it should.
            List<DecentralandEntity> rootEntities = new List<DecentralandEntity>();

            using (var iterator = entities.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.parent == null)
                    {
                        if (instant)
                            rootEntities.Add(iterator.Current.Value);
                        else
                            Environment.i.parcelScenesCleaner.MarkRootEntityForCleanup(this, iterator.Current.Value);
                    }
                }
            }

            if (instant)
            {
                int rootEntitiesCount = rootEntities.Count;
                for (int i = 0; i < rootEntitiesCount; i++)
                {
                    DecentralandEntity entity = rootEntities[i];
                    RemoveEntity(entity.entityId, instant);
                }

                entities.Clear();

                Destroy(this.gameObject);
            }
        }

        private void RemoveAllEntitiesImmediate()
        {
            RemoveAllEntities(instant: true);
        }

        public void SetEntityParent(string entityId, string parentId)
        {
            if (entityId == parentId)
            {
                return;
            }

            DecentralandEntity me = GetEntityForUpdate(entityId);

            if (me == null)
                return;

            if (parentId == "FirstPersonCameraEntityReference" || parentId == "PlayerEntityReference") // PlayerEntityReference is for compatibility purposes
            {
                // In this case, the entity will attached to the first person camera
                // On first person mode, the entity will rotate with the camera. On third person mode, the entity will rotate with the avatar
                me.SetParent(DCLCharacterController.i.firstPersonCameraReference);
                Environment.i.sceneBoundsChecker.AddPersistent(me);
            }
            else if (parentId == "AvatarEntityReference" || parentId == "AvatarPositionEntityReference") // AvatarPositionEntityReference is for compatibility purposes
            {
                // In this case, the entity will be attached to the avatar
                // It will simply rotate with the avatar, regardless of where the camera is pointing
                me.SetParent(DCLCharacterController.i.avatarReference);
                Environment.i.sceneBoundsChecker.AddPersistent(me);
            }
            else
            {
                if (me.parent == DCLCharacterController.i.firstPersonCameraReference || me.parent == DCLCharacterController.i.avatarReference)
                {
                    Environment.i.sceneBoundsChecker.RemoveEntityToBeChecked(me);
                }

                if (parentId == "0")
                {
                    // The entity will be child of the scene directly
                    me.SetParent(null);
                    me.gameObject.transform.SetParent(gameObject.transform, false);
                }
                else
                {
                    DecentralandEntity myParent = GetEntityForUpdate(parentId);

                    if (myParent != null)
                    {
                        me.SetParent(myParent);
                    }
                }
            }

            Environment.i.cullingController.MarkDirty();
            Environment.i.physicsSyncController.MarkDirty();
        }

        /**
          * This method is called when we need to attach a disposable component to the entity
          */
        public void SharedComponentAttach(string entityId, string id)
        {
            DecentralandEntity decentralandEntity = GetEntityForUpdate(entityId);

            if (decentralandEntity == null)
            {
                return;
            }

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(id, out disposableComponent)
                && disposableComponent != null)
            {
                disposableComponent.AttachTo(decentralandEntity);
            }
        }


        public BaseComponent EntityComponentCreateOrUpdateFromUnity(string entityId, CLASS_ID_COMPONENT classId, object data)
        {
            DecentralandEntity entity = GetEntityForUpdate(entityId);

            if (entity == null)
            {
                Debug.LogError($"scene '{sceneData.id}': Can't create entity component if the entity {entityId} doesn't exist!");
                return null;
            }

            if (classId == CLASS_ID_COMPONENT.TRANSFORM)
            {
                if (!(data is DCLTransform.Model))
                {
                    Debug.LogError("Data is not a DCLTransform.Model type!");
                    return null;
                }

                DCLTransform.Model modelRecovered = (DCLTransform.Model) data;

                if (!entity.components.ContainsKey(classId))
                    entity.components.Add(classId, null);


                if (entity.OnTransformChange != null)
                {
                    entity.OnTransformChange.Invoke(modelRecovered);
                }
                else
                {
                    entity.gameObject.transform.localPosition = modelRecovered.position;
                    entity.gameObject.transform.localRotation = modelRecovered.rotation;
                    entity.gameObject.transform.localScale = modelRecovered.scale;

                    Environment.i.sceneBoundsChecker?.AddEntityToBeChecked(entity);
                }

                Environment.i.physicsSyncController.MarkDirty();
                Environment.i.cullingController.MarkDirty();
                return null;
            }

            BaseComponent newComponent = null;
            DCLComponentFactory factory = ownerController.componentFactory;
            Assert.IsNotNull(factory, "Factory is null?");

            if (classId == CLASS_ID_COMPONENT.UUID_CALLBACK)
            {
                string type = "";
                if (!(data is OnPointerEvent.Model))
                {
                    Debug.LogError("Data is not a DCLTransform.Model type!");
                    return null;
                }

                OnPointerEvent.Model model = (OnPointerEvent.Model) data;

                type = model.type;

                if (!entity.uuidComponents.ContainsKey(type))
                {
                    //NOTE(Brian): We have to contain it in a gameObject or it will be pooled with the components attached.
                    var go = new GameObject("UUID Component");
                    go.transform.SetParent(entity.gameObject.transform, false);

                    switch (type)
                    {
                        case OnClick.NAME:
                            newComponent = go.GetOrCreateComponent<OnClick>();
                            break;
                        case OnPointerDown.NAME:
                            newComponent = go.GetOrCreateComponent<OnPointerDown>();
                            break;
                        case OnPointerUp.NAME:
                            newComponent = go.GetOrCreateComponent<OnPointerUp>();
                            break;
                    }

                    if (newComponent != null)
                    {
                        UUIDComponent uuidComponent = newComponent as UUIDComponent;

                        if (uuidComponent != null)
                        {
                            uuidComponent.Setup(this, entity, model);
                            entity.uuidComponents.Add(type, uuidComponent);
                        }
                        else
                        {
                            Debug.LogError("uuidComponent is not of UUIDComponent type!");
                        }
                    }
                    else
                    {
                        Debug.LogError("EntityComponentCreateOrUpdate: Invalid UUID type!");
                    }
                }
                else
                {
                    newComponent = EntityUUIDComponentUpdate(entity, type, model);
                }
            }

            Environment.i.physicsSyncController.MarkDirty();
            Environment.i.cullingController.MarkDirty();
            return newComponent;
        }

        public BaseComponent EntityComponentCreateOrUpdate(string entityId, CLASS_ID_COMPONENT classId, string data, out CleanableYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;

            DecentralandEntity entity = GetEntityForUpdate(entityId);

            if (entity == null)
            {
                Debug.LogError($"scene '{sceneData.id}': Can't create entity component if the entity {entityId} doesn't exist!");
                return null;
            }

            if (classId == CLASS_ID_COMPONENT.TRANSFORM)
            {
                MessageDecoder.DecodeTransform(data, ref DCLTransform.model);

                if (!entity.components.ContainsKey(classId))
                {
                    entity.components.Add(classId, null);
                }

                if (entity.OnTransformChange != null)
                {
                    entity.OnTransformChange.Invoke(DCLTransform.model);
                }
                else
                {
                    entity.gameObject.transform.localPosition = DCLTransform.model.position;
                    entity.gameObject.transform.localRotation = DCLTransform.model.rotation;
                    entity.gameObject.transform.localScale = DCLTransform.model.scale;

                    Environment.i.sceneBoundsChecker?.AddEntityToBeChecked(entity);
                }

                Environment.i.physicsSyncController.MarkDirty();
                Environment.i.cullingController.MarkDirty();
                return null;
            }

            BaseComponent newComponent = null;
            DCLComponentFactory factory = ownerController.componentFactory;
            Assert.IsNotNull(factory, "Factory is null?");

            // HACK: (Zak) will be removed when we separate each
            // uuid component as a different class id
            if (classId == CLASS_ID_COMPONENT.UUID_CALLBACK)
            {
                string type = "";

                OnPointerEvent.Model model = JsonUtility.FromJson<OnPointerEvent.Model>(data);

                type = model.type;

                if (!entity.uuidComponents.ContainsKey(type))
                {
                    //NOTE(Brian): We have to contain it in a gameObject or it will be pooled with the components attached.
                    var go = new GameObject("UUID Component");
                    go.transform.SetParent(entity.gameObject.transform, false);

                    switch (type)
                    {
                        case OnClick.NAME:
                            newComponent = go.GetOrCreateComponent<OnClick>();
                            break;
                        case OnPointerDown.NAME:
                            newComponent = go.GetOrCreateComponent<OnPointerDown>();
                            break;
                        case OnPointerUp.NAME:
                            newComponent = go.GetOrCreateComponent<OnPointerUp>();
                            break;
                    }

                    if (newComponent != null)
                    {
                        UUIDComponent uuidComponent = newComponent as UUIDComponent;

                        if (uuidComponent != null)
                        {
                            uuidComponent.Setup(this, entity, model);
                            entity.uuidComponents.Add(type, uuidComponent);
                        }
                        else
                        {
                            Debug.LogError("uuidComponent is not of UUIDComponent type!");
                        }
                    }
                    else
                    {
                        Debug.LogError("EntityComponentCreateOrUpdate: Invalid UUID type!");
                    }
                }
                else
                {
                    newComponent = EntityUUIDComponentUpdate(entity, type, model);
                }
            }
            else
            {
                if (!entity.components.ContainsKey(classId))
                {
                    newComponent = factory.CreateItemFromId<BaseComponent>(classId);

                    if (newComponent != null)
                    {
                        newComponent.scene = this;
                        newComponent.entity = entity;

                        entity.components.Add(classId, newComponent);

                        newComponent.transform.SetParent(entity.gameObject.transform, false);
                        newComponent.UpdateFromJSON(data);
                    }
                }
                else
                {
                    newComponent = EntityComponentUpdate(entity, classId, data);
                }
            }

            if (newComponent != null)
            {
                if (newComponent is IOutOfSceneBoundariesHandler)
                    Environment.i.sceneBoundsChecker?.AddEntityToBeChecked(entity);

                if (newComponent.isRoutineRunning)
                    yieldInstruction = newComponent.yieldInstruction;
            }

            Environment.i.physicsSyncController.MarkDirty();
            Environment.i.cullingController.MarkDirty();
            return newComponent;
        }

        // HACK: (Zak) will be removed when we separate each
        // uuid component as a different class id
        public UUIDComponent EntityUUIDComponentUpdate(DecentralandEntity entity, string type, UUIDComponent.Model model)
        {
            if (entity == null)
            {
                Debug.LogError($"Can't update the {type} uuid component of a nonexistent entity!", this);
                return null;
            }

            if (!entity.uuidComponents.ContainsKey(type))
            {
                Debug.LogError($"Entity {entity.entityId} doesn't have a {type} uuid component to update!", this);
                return null;
            }

            UUIDComponent targetComponent = entity.uuidComponents[type];
            targetComponent.Setup(this, entity, model);

            return targetComponent;
        }

        // The EntityComponentUpdate() parameters differ from other similar methods because there is no EntityComponentUpdate protocol message yet.
        public BaseComponent EntityComponentUpdate(DecentralandEntity entity, CLASS_ID_COMPONENT classId,
            string componentJson)
        {
            if (entity == null)
            {
                Debug.LogError($"Can't update the {classId} component of a nonexistent entity!", this);
                return null;
            }

            if (!entity.components.ContainsKey(classId))
            {
                Debug.LogError($"Entity {entity.entityId} doesn't have a {classId} component to update!", this);
                return null;
            }

            BaseComponent targetComponent = entity.components[classId];
            targetComponent.UpdateFromJSON(componentJson);

            return targetComponent;
        }

        public BaseDisposable SharedComponentCreate(string id, int classId)
        {
            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(id, out disposableComponent))
            {
                return disposableComponent;
            }

            BaseDisposable newComponent = null;

            switch ((CLASS_ID) classId)
            {
                case CLASS_ID.BOX_SHAPE:
                {
                    newComponent = new BoxShape(this);
                    break;
                }

                case CLASS_ID.SPHERE_SHAPE:
                {
                    newComponent = new SphereShape(this);
                    break;
                }

                case CLASS_ID.CONE_SHAPE:
                {
                    newComponent = new ConeShape(this);
                    break;
                }

                case CLASS_ID.CYLINDER_SHAPE:
                {
                    newComponent = new CylinderShape(this);
                    break;
                }

                case CLASS_ID.PLANE_SHAPE:
                {
                    newComponent = new PlaneShape(this);
                    break;
                }

                case CLASS_ID.GLTF_SHAPE:
                {
                    newComponent = new GLTFShape(this);
                    break;
                }

                case CLASS_ID.NFT_SHAPE:
                {
                    newComponent = new NFTShape(this);
                    break;
                }

                case CLASS_ID.OBJ_SHAPE:
                {
                    newComponent = new OBJShape(this);
                    break;
                }

                case CLASS_ID.BASIC_MATERIAL:
                {
                    newComponent = new BasicMaterial(this);
                    break;
                }

                case CLASS_ID.PBR_MATERIAL:
                {
                    newComponent = new PBRMaterial(this);
                    break;
                }

                case CLASS_ID.AUDIO_CLIP:
                {
                    newComponent = new DCLAudioClip(this);
                    break;
                }

                case CLASS_ID.TEXTURE:
                {
                    newComponent = new DCLTexture(this);
                    break;
                }

                case CLASS_ID.UI_INPUT_TEXT_SHAPE:
                {
                    newComponent = new UIInputText(this);
                    break;
                }

                case CLASS_ID.UI_FULLSCREEN_SHAPE:
                case CLASS_ID.UI_SCREEN_SPACE_SHAPE:
                {
                    if (uiScreenSpace == null)
                    {
                        newComponent = new UIScreenSpace(this);
                    }

                    break;
                }

                case CLASS_ID.UI_CONTAINER_RECT:
                {
                    newComponent = new UIContainerRect(this);
                    break;
                }

                case CLASS_ID.UI_SLIDER_SHAPE:
                {
                    newComponent = new UIScrollRect(this);
                    break;
                }

                case CLASS_ID.UI_CONTAINER_STACK:
                {
                    newComponent = new UIContainerStack(this);
                    break;
                }

                case CLASS_ID.UI_IMAGE_SHAPE:
                {
                    newComponent = new UIImage(this);
                    break;
                }

                case CLASS_ID.UI_TEXT_SHAPE:
                {
                    newComponent = new UIText(this);
                    break;
                }

                case CLASS_ID.VIDEO_CLIP:
                {
                    newComponent = new DCLVideoClip(this);
                    break;
                }

                case CLASS_ID.VIDEO_TEXTURE:
                {
                    newComponent = new DCLVideoTexture(this);
                    break;
                }

                case CLASS_ID.FONT:
                {
                    newComponent = new DCLFont(this);
                    break;
                }

                case CLASS_ID.NAME:
                {
                    newComponent = new DCLName(this);
                    break;
                }
                default:
                    Debug.LogError($"Unknown classId");
                    break;
            }

            if (newComponent != null)
            {
                newComponent.id = id;
                disposableComponents.Add(id, newComponent);

                if (state != State.READY)
                {
                    disposableNotReady.Add(id);
                }
            }

            return newComponent;
        }

        public void SharedComponentDispose(string id)
        {
            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(id, out disposableComponent))
            {
                if (disposableComponent != null)
                {
                    disposableComponent.Dispose();
                }

                disposableComponents.Remove(id);
            }
        }

        public void EntityComponentRemove(string entityId, string name)
        {
            DecentralandEntity decentralandEntity = GetEntityForUpdate(entityId);
            if (decentralandEntity == null)
            {
                return;
            }

            RemoveEntityComponent(decentralandEntity, name);
        }

        private void RemoveComponentType<T>(DecentralandEntity entity, CLASS_ID_COMPONENT classId)
            where T : MonoBehaviour
        {
            var component = entity.components[classId].GetComponent<T>();

            if (component != null)
            {
                Utils.SafeDestroy(component);
            }
        }

        // HACK: (Zak) will be removed when we separate each
        // uuid component as a different class id
        private void RemoveUUIDComponentType<T>(DecentralandEntity entity, string type)
            where T : UUIDComponent
        {
            var component = entity.uuidComponents[type].GetComponent<T>();

            if (component != null)
            {
                Utils.SafeDestroy(component);
                entity.uuidComponents.Remove(type);
            }
        }

        private void RemoveEntityComponent(DecentralandEntity entity, string componentName)
        {
            switch (componentName)
            {
                case "shape":
                    if (entity.meshesInfo.currentShape != null)
                    {
                        entity.meshesInfo.currentShape.DetachFrom(entity);
                    }

                    return;
                case OnClick.NAME:
                    RemoveUUIDComponentType<OnClick>(entity, componentName);
                    return;
                case OnPointerDown.NAME:
                    RemoveUUIDComponentType<OnPointerDown>(entity, componentName);
                    return;
                case OnPointerUp.NAME:
                    RemoveUUIDComponentType<OnPointerUp>(entity, componentName);
                    return;
            }
        }

        public void SharedComponentUpdate(string id, string json, out CleanableYieldInstruction yieldInstruction)
        {
            ProfilingEvents.OnMessageDecodeStart?.Invoke("ComponentUpdated");
            BaseDisposable newComponent = SharedComponentUpdate(id, json);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke("ComponentUpdated");

            yieldInstruction = null;

            if (newComponent != null && newComponent.isRoutineRunning)
                yieldInstruction = newComponent.yieldInstruction;
        }

        public BaseDisposable SharedComponentUpdate(string id, string json)
        {
            if (disposableComponents.TryGetValue(id, out BaseDisposable disposableComponent))
            {
                disposableComponent.UpdateFromJSON(json);
                return disposableComponent;
            }
            else
            {
                if (gameObject == null)
                {
                    Debug.LogError($"Unknown disposableComponent {id} -- scene has been destroyed?");
                }
                else
                {
                    Debug.LogError($"Unknown disposableComponent {id}", gameObject);
                }
            }

            return null;
        }

        protected virtual void SendMetricsEvent()
        {
            if (Time.frameCount % 10 == 0)
                metricsController.SendEvent();
        }


        public BaseDisposable GetSharedComponent(string componentId)
        {
            BaseDisposable result;

            if (!disposableComponents.TryGetValue(componentId, out result))
            {
                return null;
            }

            return result;
        }

        private DecentralandEntity GetEntityForUpdate(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                Debug.LogError("Null or empty entityId");
                return null;
            }

            DecentralandEntity decentralandEntity;

            if (!entities.TryGetValue(entityId, out decentralandEntity))
            {
                return null;
            }

            //NOTE(Brian): This is for removing stray null references? This should never happen.
            //             Maybe move to a different 'clean-up' method to make this method have a single responsibility?.
            if (decentralandEntity == null || decentralandEntity.gameObject == null)
            {
                entities.Remove(entityId);
                return null;
            }

            return decentralandEntity;
        }

        private void OnDisposableReady(BaseDisposable disposable)
        {
            if (isReleased)
                return;

            disposableNotReady.Remove(disposable.id);

            if (VERBOSE)
            {
                Debug.Log($"{sceneData.basePosition} Disposable objects left... {disposableNotReady.Count}");
            }

            if (disposableNotReady.Count == 0)
            {
                SetSceneReady();
            }

            OnStateRefreshed?.Invoke(this);
            RefreshName();
        }

        public void SetInitMessagesDone()
        {
            if (isReleased)
                return;

            if (state == State.READY)
            {
                Debug.LogWarning($"Init messages done after ready?! {sceneData.basePosition}", gameObject);
                return;
            }

            state = State.WAITING_FOR_COMPONENTS;
            RefreshName();

            if (disposableNotReadyCount > 0)
            {
                //NOTE(Brian): Here, we have to split the iterations. If not, we will get repeated calls of
                //             SetSceneReady(), as the disposableNotReady count is 1 and gets to 0
                //             in each OnDisposableReady() call.

                using (var iterator = disposableComponents.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        disposableComponents[iterator.Current.Value.id].CallWhenReady(OnDisposableReady);
                    }
                }
            }
            else
            {
                SetSceneReady();
            }
        }

        private void SetSceneReady()
        {
            if (state == State.READY)
                return;

            if (VERBOSE)
                Debug.Log($"{sceneData.basePosition} Scene Ready!");

            state = State.READY;

            SceneController.i.SendSceneReady(sceneData.id);
            RefreshName();

            OnSceneReady?.Invoke(this);
        }

        private void DisposeAllSceneComponents()
        {
            List<string> allDisposableComponents = disposableComponents.Select(x => x.Key).ToList();
            foreach (string id in allDisposableComponents)
            {
                Environment.i.parcelScenesCleaner.MarkDisposableComponentForCleanup(this, id);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Get Waiting Components Debug Info")]
        public void GetWaitingComponentsDebugInfo()
        {
            switch (state)
            {
                case State.WAITING_FOR_COMPONENTS:

                    foreach (string componentId in disposableNotReady)
                    {
                        if (disposableComponents.ContainsKey(componentId))
                        {
                            var component = disposableComponents[componentId];

                            Debug.Log($"Waiting for: {component.ToString()}");

                            foreach (var entity in component.attachedEntities)
                            {
                                var loader = LoadableShape.GetLoaderForEntity(entity);

                                string loadInfo = "No loader";

                                if (loader != null)
                                {
                                    loadInfo = loader.ToString();
                                }

                                Debug.Log($"This shape is attached to {entity.entityId} entity. Click here for highlight it.\nLoading info: {loadInfo}", entity.gameObject);
                            }
                        }
                        else
                        {
                            Debug.Log($"Waiting for missing component? id: {componentId}");
                        }
                    }

                    break;

                default:
                    Debug.Log("This scene is not waiting for any components. Its current state is " + state);
                    break;
            }
        }
#endif
    }
}