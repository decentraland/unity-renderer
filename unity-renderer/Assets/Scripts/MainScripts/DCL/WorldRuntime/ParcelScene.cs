using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using DCL.Controllers.ParcelSceneDebug;
using System.Collections.Generic;
using System.Linq;
using MainScripts.DCL.WorldRuntime;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Controllers
{
    public class ParcelScene : MonoBehaviour, IParcelScene, ISceneMessageProcessor
    {
        public Dictionary<string, IDCLEntity> entities { get; private set; } = new Dictionary<string, IDCLEntity>();
        public Dictionary<string, ISharedComponent> disposableComponents { get; private set; } = new Dictionary<string, ISharedComponent>();
        public LoadParcelScenesMessage.UnityParcelScene sceneData { get; protected set; }

        public HashSet<Vector2Int> parcels = new HashSet<Vector2Int>();
        public ISceneMetricsCounter metricsCounter { get; set; }
        public event System.Action<IDCLEntity> OnEntityAdded;
        public event System.Action<IDCLEntity> OnEntityRemoved;
        public event System.Action<IComponent> OnComponentAdded;
        public event System.Action<IComponent> OnComponentRemoved;
        public event System.Action OnChanged;
        public event System.Action<LoadParcelScenesMessage.UnityParcelScene> OnSetData;
        public event System.Action<string, ISharedComponent> OnAddSharedComponent;
        public event System.Action<float> OnLoadingStateUpdated;

        public ContentProvider contentProvider { get; set; }

        public bool isTestScene { get; set; } = false;
        public bool isPersistent { get; set; } = false;
        public float loadingProgress { get; private set; }

        [System.NonSerialized]
        public string sceneName;

        [System.NonSerialized]
        public bool unloadWithDistance = true;

        SceneDebugPlane sceneDebugPlane = null;

        public SceneLifecycleHandler sceneLifecycleHandler;

        public bool isReleased { get; private set; }

        public void Awake()
        {
            CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
            sceneLifecycleHandler = new SceneLifecycleHandler(this);
            metricsCounter = new SceneMetricsCounter(DataStore.i.sceneWorldObjects);
        }

        private void OnDestroy()
        {
            CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
            metricsCounter?.Dispose();
        }

        void OnDisable() { metricsCounter?.Disable(); }

        private void Update()
        {
            if (sceneLifecycleHandler.state == SceneLifecycleHandler.State.READY
                && CommonScriptableObjects.rendererState.Get())
                SendMetricsEvent();
        }

        protected virtual string prettyName => sceneData.basePosition.ToString();

        public virtual void SetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            Assert.IsTrue( !string.IsNullOrEmpty(data.id), "Scene must have an ID!" );

            this.sceneData = data;

            contentProvider = new ContentProvider();
            contentProvider.baseUrl = data.baseUrl;
            contentProvider.contents = data.contents;
            contentProvider.BakeHashes();

            parcels.Clear();

            for (int i = 0; i < sceneData.parcels.Length; i++)
            {
                parcels.Add(sceneData.parcels[i]);
            }

            if (DCLCharacterController.i != null)
            {
                gameObject.transform.position = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(data.basePosition.x, data.basePosition.y));
            }

            DataStore.i.sceneWorldObjects.AddScene(sceneData.id);

            metricsCounter.Configure(sceneData.id, sceneData.basePosition, sceneData.parcels.Length);
            metricsCounter.Enable();

            OnSetData?.Invoke(data);
        }

        void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            Vector3 sceneWorldPos = Utils.GridToWorldPosition(sceneData.basePosition.x, sceneData.basePosition.y);
            gameObject.transform.position = PositionUtils.WorldToUnityPosition(sceneWorldPos);
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

            if (immediate) //!CommonScriptableObjects.rendererState.Get())
            {
                RemoveAllEntitiesImmediate();
                PoolManager.i.Cleanup(true, true);
                DataStore.i.sceneWorldObjects.RemoveScene(sceneData.id);
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
                    DataStore.i.sceneWorldObjects.RemoveScene(sceneData.id);
                }
            }

            isReleased = true;
        }

        public override string ToString() { return "Parcel Scene: " + base.ToString() + "\n" + sceneData; }

        public string GetSceneName()
        {
            return string.IsNullOrEmpty(sceneName) ? "Unnamed" : sceneName;
        }

        public bool IsInsideSceneBoundaries(Bounds objectBounds)
        {
            if (!IsInsideSceneBoundaries(objectBounds.min + CommonScriptableObjects.worldOffset, objectBounds.max.y))
                return false;
            if (!IsInsideSceneBoundaries(objectBounds.max + CommonScriptableObjects.worldOffset, objectBounds.max.y))
                return false;

            return true;
        }

        public virtual bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0f)
        {
            if (parcels.Count == 0)
                return false;

            float heightLimit = metricsCounter.maxCount.sceneHeight;

            if (height > heightLimit)
                return false;

            return parcels.Contains(gridPosition);
        }

        public virtual bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f)
        {
            if (parcels.Count == 0)
                return false;

            float heightLimit = metricsCounter.maxCount.sceneHeight;
            if (height > heightLimit)
                return false;

            int noThresholdZCoordinate = Mathf.FloorToInt(worldPosition.z / ParcelSettings.PARCEL_SIZE);
            int noThresholdXCoordinate = Mathf.FloorToInt(worldPosition.x / ParcelSettings.PARCEL_SIZE);

            // We check the target world position
            Vector2Int targetCoordinate = new Vector2Int(noThresholdXCoordinate, noThresholdZCoordinate);
            if (parcels.Contains(targetCoordinate))
                return true;

            // We need to check using a threshold from the target point, in order to cover correctly the parcel "border/edge" positions
            Vector2Int coordinateMin = new Vector2Int();
            coordinateMin.x = Mathf.FloorToInt((worldPosition.x - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);
            coordinateMin.y = Mathf.FloorToInt((worldPosition.z - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);

            Vector2Int coordinateMax = new Vector2Int();
            coordinateMax.x = Mathf.FloorToInt((worldPosition.x + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);
            coordinateMax.y = Mathf.FloorToInt((worldPosition.z + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);

            // We check the east/north-threshold position
            targetCoordinate.Set(coordinateMax.x, coordinateMax.y);
            if (parcels.Contains(targetCoordinate))
                return true;

            // We check the east/south-threshold position
            targetCoordinate.Set(coordinateMax.x, coordinateMin.y);
            if (parcels.Contains(targetCoordinate))
                return true;

            // We check the west/north-threshold position
            targetCoordinate.Set(coordinateMin.x, coordinateMax.y);
            if (parcels.Contains(targetCoordinate))
                return true;

            // We check the west/south-threshold position
            targetCoordinate.Set(coordinateMin.x, coordinateMin.y);
            if (parcels.Contains(targetCoordinate))
                return true;

            return false;
        }

        public Transform GetSceneTransform() { return transform; }

        public IDCLEntity CreateEntity(string id)
        {
            if (entities.ContainsKey(id))
            {
                return entities[id];
            }

            var newEntity = new DecentralandEntity();
            newEntity.entityId = id;

            PoolManagerFactory.EnsureEntityPool(false);

            // As we know that the pool already exists, we just get one gameobject from it
            PoolableObject po = PoolManager.i.Get(PoolManagerFactory.EMPTY_GO_POOL_NAME);

            newEntity.meshesInfo.innerGameObject = po.gameObject;
            newEntity.gameObject = po.gameObject;

#if UNITY_EDITOR
            newEntity.gameObject.name = "ENTITY_" + id;
#endif
            newEntity.gameObject.transform.SetParent(gameObject.transform, false);
            newEntity.gameObject.SetActive(true);
            newEntity.scene = this;

            newEntity.OnCleanupEvent += po.OnCleanup;

            if (Environment.i.world.sceneBoundsChecker.enabled)
                newEntity.OnShapeUpdated += Environment.i.world.sceneBoundsChecker.AddEntityToBeChecked;

            entities.Add(id, newEntity);

            DataStore.i.sceneWorldObjects.sceneData[sceneData.id].owners.Add(id);

            OnEntityAdded?.Invoke(newEntity);

            return newEntity;
        }

        public void RemoveEntity(string id, bool removeImmediatelyFromEntitiesList = true)
        {
            if (entities.ContainsKey(id))
            {
                IDCLEntity entity = entities[id];

                if (!entity.markedForCleanup)
                {
                    // This will also cleanup its children
                    CleanUpEntityRecursively(entity, removeImmediatelyFromEntitiesList);
                }

                entities.Remove(id);

                var data = DataStore.i.sceneWorldObjects.sceneData;

                if (data.ContainsKey(sceneData.id))
                {
                    data[sceneData.id].owners.Remove(id);
                }
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogWarning($"Couldn't remove entity with ID: {id} as it doesn't exist.");
            }
#endif
        }

        void CleanUpEntityRecursively(IDCLEntity entity, bool removeImmediatelyFromEntitiesList)
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

            if (Environment.i.world.sceneBoundsChecker.enabled)
            {
                entity.OnShapeUpdated -= Environment.i.world.sceneBoundsChecker.AddEntityToBeChecked;
                Environment.i.world.sceneBoundsChecker.RemoveEntityToBeChecked(entity);
            }

            if (removeImmediatelyFromEntitiesList)
            {
                // Every entity ends up being removed through here
                entity.Cleanup();
                entities.Remove(entity.entityId);
            }
            else
            {
                Environment.i.platform.parcelScenesCleaner.MarkForCleanup(entity);
            }
        }

        void RemoveAllEntities(bool instant = false)
        {
            //NOTE(Brian): We need to remove only the rootEntities.
            //             If we don't, duplicated entities will get removed when destroying
            //             recursively, making this more complicated than it should.
            List<IDCLEntity> rootEntities = new List<IDCLEntity>();

            using (var iterator = entities.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.parent == null)
                    {
                        if (instant)
                            rootEntities.Add(iterator.Current.Value);
                        else
                            Environment.i.platform.parcelScenesCleaner.MarkRootEntityForCleanup(this, iterator.Current.Value);
                    }
                }
            }

            if (instant)
            {
                int rootEntitiesCount = rootEntities.Count;
                for (int i = 0; i < rootEntitiesCount; i++)
                {
                    IDCLEntity entity = rootEntities[i];
                    RemoveEntity(entity.entityId, instant);
                }

                entities.Clear();

                Destroy(this.gameObject);
            }
        }

        private void RemoveAllEntitiesImmediate() { RemoveAllEntities(instant: true); }

        public void SetEntityParent(string entityId, string parentId)
        {
            if (entityId == parentId)
            {
                return;
            }

            IDCLEntity me = GetEntityForUpdate(entityId);

            if (me == null)
                return;

            Environment.i.platform.cullingController.MarkDirty();
            Environment.i.platform.physicsSyncController.MarkDirty();

            if ( DCLCharacterController.i != null )
            {
                if (parentId == "FirstPersonCameraEntityReference" || parentId == "PlayerEntityReference") // PlayerEntityReference is for compatibility purposes
                {
                    // In this case, the entity will attached to the first person camera
                    // On first person mode, the entity will rotate with the camera. On third person mode, the entity will rotate with the avatar
                    me.SetParent(DCLCharacterController.i.firstPersonCameraReference);
                    Environment.i.world.sceneBoundsChecker.AddPersistent(me);
                    return;
                }

                if (parentId == "AvatarEntityReference" || parentId == "AvatarPositionEntityReference") // AvatarPositionEntityReference is for compatibility purposes
                {
                    // In this case, the entity will be attached to the avatar
                    // It will simply rotate with the avatar, regardless of where the camera is pointing
                    me.SetParent(DCLCharacterController.i.avatarReference);
                    Environment.i.world.sceneBoundsChecker.AddPersistent(me);
                    return;
                }

                if (me.parent == DCLCharacterController.i.firstPersonCameraReference || me.parent == DCLCharacterController.i.avatarReference)
                {
                    Environment.i.world.sceneBoundsChecker.RemoveEntityToBeChecked(me);
                }
            }

            if (parentId == "0")
            {
                // The entity will be child of the scene directly
                me.SetParent(null);
                me.gameObject.transform.SetParent(gameObject.transform, false);
            }
            else
            {
                IDCLEntity myParent = GetEntityForUpdate(parentId);

                if (myParent != null)
                {
                    me.SetParent(myParent);
                }
            }
        }

        /**
          * This method is called when we need to attach a disposable component to the entity
          */
        public void SharedComponentAttach(string entityId, string componentId)
        {
            IDCLEntity entity = GetEntityForUpdate(entityId);

            if (entity == null)
                return;

            if (disposableComponents.TryGetValue(componentId, out ISharedComponent sharedComponent))
            {
                sharedComponent.AttachTo(entity);
            }
        }

        public IEntityComponent EntityComponentCreateOrUpdateWithModel(string entityId, CLASS_ID_COMPONENT classId, object data)
        {
            IDCLEntity entity = GetEntityForUpdate(entityId);

            if (entity == null)
            {
                Debug.LogError($"scene '{sceneData.id}': Can't create entity component if the entity {entityId} doesn't exist!");
                return null;
            }

            IEntityComponent newComponent = null;

            if (classId == CLASS_ID_COMPONENT.UUID_CALLBACK)
            {
                OnPointerEvent.Model model = JsonUtility.FromJson<OnPointerEvent.Model>(data as string);
                classId = model.GetClassIdFromType();
            }
            // NOTE: TRANSFORM and AVATAR_ATTACH can't be used in the same Entity at the same time.
            // so we remove AVATAR_ATTACH (if exists) when a TRANSFORM is created.
            else if (classId == CLASS_ID_COMPONENT.TRANSFORM
                     && entity.TryGetBaseComponent(CLASS_ID_COMPONENT.AVATAR_ATTACH, out IEntityComponent component))
            {
                component.Cleanup();
                entity.components.Remove( CLASS_ID_COMPONENT.AVATAR_ATTACH );
            }

            if (!entity.components.ContainsKey(classId))
            {
                var factory = Environment.i.world.componentFactory;
                newComponent = factory.CreateComponent((int) classId) as IEntityComponent;

                if (newComponent != null)
                {
                    entity.components.Add(classId, newComponent);
                    OnComponentAdded?.Invoke(newComponent);

                    newComponent.Initialize(this, entity);

                    if (data is string json)
                    {
                        newComponent.UpdateFromJSON(json);
                    }
                    else
                    {
                        newComponent.UpdateFromModel(data as BaseModel);
                    }
                }
            }
            else
            {
                newComponent = EntityComponentUpdate(entity, classId, data as string);
            }

            if (newComponent != null && newComponent is IOutOfSceneBoundariesHandler)
                Environment.i.world.sceneBoundsChecker?.AddEntityToBeChecked(entity);

            OnChanged?.Invoke();
            Environment.i.platform.physicsSyncController.MarkDirty();
            Environment.i.platform.cullingController.MarkDirty();
            return newComponent;
        }

        public IEntityComponent EntityComponentCreateOrUpdate(string entityId, CLASS_ID_COMPONENT classId, string data) { return EntityComponentCreateOrUpdateWithModel(entityId, classId, data); }

        // The EntityComponentUpdate() parameters differ from other similar methods because there is no EntityComponentUpdate protocol message yet.
        public IEntityComponent EntityComponentUpdate(IDCLEntity entity, CLASS_ID_COMPONENT classId,
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

            IComponent targetComponent = entity.components[classId];
            targetComponent.UpdateFromJSON(componentJson);

            return targetComponent as IEntityComponent;
        }

        public ISharedComponent SharedComponentCreate(string id, int classId)
        {
            if (disposableComponents.TryGetValue(id, out ISharedComponent component))
                return component;

            if (classId == (int) CLASS_ID.UI_SCREEN_SPACE_SHAPE || classId == (int) CLASS_ID.UI_FULLSCREEN_SHAPE)
            {
                if (GetSharedComponent<UIScreenSpace>() != null)
                    return null;
            }

            var factory = Environment.i.world.componentFactory;
            ISharedComponent newComponent = factory.CreateComponent(classId) as ISharedComponent;

            if (newComponent == null)
                return null;

            disposableComponents.Add(id, newComponent);
            OnAddSharedComponent?.Invoke(id, newComponent);

            newComponent.Initialize(this, id);

            return newComponent;
        }

        public void SharedComponentDispose(string id)
        {
            if (disposableComponents.TryGetValue(id, out ISharedComponent sharedComponent))
            {
                sharedComponent?.Dispose();
                disposableComponents.Remove(id);
                OnComponentRemoved?.Invoke(sharedComponent);
            }
        }

        public void EntityComponentRemove(string entityId, string name)
        {
            IDCLEntity decentralandEntity = GetEntityForUpdate(entityId);

            if (decentralandEntity == null)
            {
                return;
            }

            RemoveEntityComponent(decentralandEntity, name);
        }

        public T GetSharedComponent<T>()
            where T : class
        {
            return disposableComponents.Values.FirstOrDefault(x => x is T) as T;
        }

        private void RemoveComponentType<T>(IDCLEntity entity, CLASS_ID_COMPONENT classId)
            where T : MonoBehaviour
        {
            var component = entity.components[classId] as IEntityComponent;

            if (component == null)
                return;

            var monoBehaviour = component.GetTransform().GetComponent<T>();

            if (monoBehaviour != null)
            {
                Utils.SafeDestroy(monoBehaviour);
            }
        }

        private void RemoveEntityComponent(IDCLEntity entity, string componentName)
        {
            switch (componentName)
            {
                case "shape":
                    if (entity.meshesInfo.currentShape is BaseShape baseShape)
                    {
                        baseShape.DetachFrom(entity);
                    }

                    return;

                case OnClick.NAME:
                    {
                        if ( entity.TryGetBaseComponent(CLASS_ID_COMPONENT.UUID_ON_CLICK, out IEntityComponent component ))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            entity.components.Remove( CLASS_ID_COMPONENT.UUID_ON_CLICK );
                        }

                        return;
                    }
                case OnPointerDown.NAME:
                    {
                        if ( entity.TryGetBaseComponent(CLASS_ID_COMPONENT.UUID_ON_DOWN, out IEntityComponent component ))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            entity.components.Remove( CLASS_ID_COMPONENT.UUID_ON_DOWN );
                        }
                    }
                    return;
                case OnPointerUp.NAME:
                    {
                        if ( entity.TryGetBaseComponent(CLASS_ID_COMPONENT.UUID_ON_UP, out IEntityComponent component ))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            entity.components.Remove( CLASS_ID_COMPONENT.UUID_ON_UP );
                        }
                    }
                    return;
                case OnPointerHoverEnter.NAME:
                    {
                        if ( entity.TryGetBaseComponent(CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER, out IEntityComponent component ))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            entity.components.Remove( CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER );
                        }
                    }
                    return;
                case OnPointerHoverExit.NAME:
                    {
                        if ( entity.TryGetBaseComponent(CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT, out IEntityComponent component ))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            entity.components.Remove( CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT );
                        }
                    }
                    return;
                case "transform":
                    {
                        if ( entity.TryGetBaseComponent(CLASS_ID_COMPONENT.AVATAR_ATTACH, out IEntityComponent component ))
                        {
                            component.Cleanup();
                            entity.components.Remove( CLASS_ID_COMPONENT.AVATAR_ATTACH );
                        }
                    }
                    return;
            }
        }

        public ISharedComponent SharedComponentUpdate(string id, BaseModel model)
        {
            if (disposableComponents.TryGetValue(id, out ISharedComponent sharedComponent))
            {
                sharedComponent.UpdateFromModel(model);
                return sharedComponent;
            }

            if (gameObject == null)
            {
                Debug.LogError($"Unknown disposableComponent {id} -- scene has been destroyed?");
            }
            else
            {
                Debug.LogError($"Unknown disposableComponent {id}", gameObject);
            }

            return null;
        }

        public ISharedComponent SharedComponentUpdate(string id, string json)
        {
            if (disposableComponents.TryGetValue(id, out ISharedComponent disposableComponent))
            {
                disposableComponent.UpdateFromJSON(json);
                return disposableComponent;
            }

            if (gameObject == null)
            {
                Debug.LogError($"Unknown disposableComponent {id} -- scene has been destroyed?");
            }
            else
            {
                Debug.LogError($"Unknown disposableComponent {id}", gameObject);
            }

            return null;
        }

        protected virtual void SendMetricsEvent()
        {
            if (Time.frameCount % 10 == 0)
                metricsCounter.SendEvent();
        }

        public ISharedComponent GetSharedComponent(string componentId)
        {
            if (!disposableComponents.TryGetValue(componentId, out ISharedComponent result))
            {
                return null;
            }

            return result;
        }

        private IDCLEntity GetEntityForUpdate(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                Debug.LogError("Null or empty entityId");
                return null;
            }

            if (!entities.TryGetValue(entityId, out IDCLEntity entity))
            {
                return null;
            }

            //NOTE(Brian): This is for removing stray null references? This should never happen.
            //             Maybe move to a different 'clean-up' method to make this method have a single responsibility?.
            if (entity == null || entity.gameObject == null)
            {
                entities.Remove(entityId);
                return null;
            }

            return entity;
        }

        private void DisposeAllSceneComponents()
        {
            List<string> allDisposableComponents = disposableComponents.Select(x => x.Key).ToList();
            foreach (string id in allDisposableComponents)
            {
                Environment.i.platform.parcelScenesCleaner.MarkDisposableComponentForCleanup(this, id);
            }
        }

        public string GetStateString()
        {
            string baseState = isPersistent ? "global-scene" : "scene";
            switch (sceneLifecycleHandler.state)
            {
                case SceneLifecycleHandler.State.NOT_READY:
                    return $"{baseState}:{prettyName} - not ready...";
                case SceneLifecycleHandler.State.WAITING_FOR_INIT_MESSAGES:
                    return $"{baseState}:{prettyName} - waiting for init messages...";
                case SceneLifecycleHandler.State.WAITING_FOR_COMPONENTS:
                    if (disposableComponents != null && disposableComponents.Count > 0)
                        return $"{baseState}:{prettyName} - left to ready:{disposableComponents.Count - sceneLifecycleHandler.disposableNotReadyCount}/{disposableComponents.Count} ({loadingProgress}%)";
                    else
                        return $"{baseState}:{prettyName} - no components. waiting...";
                case SceneLifecycleHandler.State.READY:
                    return $"{baseState}:{prettyName} - ready!";
            }

            return $"scene:{prettyName} - no state?";
        }

        public void RefreshLoadingState()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif
            
            CalculateSceneLoadingState();

#if UNITY_EDITOR
            gameObject.name = GetStateString();
#endif
        }

        [ContextMenu("Get Waiting Components Debug Info")]
        public void GetWaitingComponentsDebugInfo()
        {
            switch (sceneLifecycleHandler.state)
            {
                case SceneLifecycleHandler.State.WAITING_FOR_COMPONENTS:

                    foreach (string componentId in sceneLifecycleHandler.disposableNotReady)
                    {
                        if (disposableComponents.ContainsKey(componentId))
                        {
                            var component = disposableComponents[componentId];

                            Debug.Log($"Waiting for: {component.ToString()}");

                            foreach (var entity in component.GetAttachedEntities())
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
                    Debug.Log("This scene is not waiting for any components. Its current state is " + sceneLifecycleHandler.state);
                    break;
            }
        }

        /// <summary>
        /// Calculates the current loading progress of the scene and raise the event OnLoadingStateUpdated with the percentage.
        /// </summary>
        public void CalculateSceneLoadingState()
        {
            loadingProgress = 0f;

            if (sceneLifecycleHandler.state == SceneLifecycleHandler.State.WAITING_FOR_COMPONENTS ||
                sceneLifecycleHandler.state == SceneLifecycleHandler.State.READY)
            {
                loadingProgress = disposableComponents != null && disposableComponents.Count > 0 ? (disposableComponents.Count - sceneLifecycleHandler.disposableNotReadyCount) * 100f / disposableComponents.Count : 100f;
            }

            OnLoadingStateUpdated?.Invoke(loadingProgress);
        }
    }
}