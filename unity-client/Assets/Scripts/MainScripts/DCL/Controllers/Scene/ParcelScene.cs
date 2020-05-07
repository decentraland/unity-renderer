using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace DCL.Controllers
{
    public class ParcelScene : MonoBehaviour, ICleanable
    {
        public static bool VERBOSE = false;
        public enum State
        {
            NOT_READY,
            WAITING_FOR_INIT_MESSAGES,
            WAITING_FOR_COMPONENTS,
            READY,
        }

        public static ParcelScenesCleaner parcelScenesCleaner = new ParcelScenesCleaner();
        private const int ENTITY_POOL_PREWARM_COUNT = 2000;

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

        public ContentProvider contentProvider;
        public int disposableNotReadyCount => disposableNotReady.Count;

        [System.NonSerialized]
        public bool useBlockers = true;

        [System.NonSerialized]
        public bool isTestScene = false;

        [System.NonSerialized]
        public bool isPersistent = false;

        [System.NonSerialized]
        public bool unloadWithDistance = true;

        public BlockerHandler blockerHandler;
        public bool isReady => state == State.READY;

        readonly List<string> disposableNotReady = new List<string>();
        bool isReleased = false;
        State state = State.NOT_READY;

        public void Awake()
        {
            state = State.NOT_READY;

            if (DCLCharacterController.i)
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust += OnPrecisionAdjust;

            metricsController = new SceneMetricsController(this);
            metricsController.Enable();

            CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            OnRenderingStateChanged(CommonScriptableObjects.rendererState.Get(), false);
        }

        void OnDisable()
        {
            metricsController.Disable();
        }

        private void OnDestroy()
        {
            blockerHandler?.CleanBlockers();
            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
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

            if (useBlockers)
                blockerHandler = new BlockerHandler();

            if (DCLCharacterController.i != null)
                gameObject.transform.position = DCLCharacterController.i.characterPosition.WorldToUnityPosition(Utils.GridToWorldPosition(data.basePosition.x, data.basePosition.y));

#if UNITY_EDITOR
            //NOTE(Brian): Don't generate parcel blockers if debugScenes is active and is not the desired scene.
            if (SceneController.i.debugScenes && SceneController.i.debugSceneCoords != data.basePosition)
            {
                SetSceneReady();
                return;
            }
#endif
            blockerHandler?.SetupBlockers(parcels, metricsController.GetLimits().sceneHeight, this.transform);

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
            if (EnvironmentSettings.DEBUG && sceneData.parcels != null)
            {
                int sceneDataParcelsLength = sceneData.parcels.Length;
                for (int j = 0; j < sceneDataParcelsLength; j++)
                {
                    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

                    Object.Destroy(plane.GetComponent<MeshCollider>());

                    plane.name = $"parcel:{sceneData.parcels[j].x},{sceneData.parcels[j].y}";

                    plane.transform.SetParent(gameObject.transform);

                    // the plane mesh with scale 1 occupies a 10 units space
                    plane.transform.localScale = new Vector3(ParcelSettings.PARCEL_SIZE * 0.1f, 1f,
                        ParcelSettings.PARCEL_SIZE * 0.1f);

                    Vector3 position = Utils.GridToWorldPosition(sceneData.parcels[j].x, sceneData.parcels[j].y);
                    // SET TO A POSITION RELATIVE TO basePosition

                    position.Set(position.x + ParcelSettings.PARCEL_SIZE / 2, ParcelSettings.DEBUG_FLOOR_HEIGHT,
                        position.z + ParcelSettings.PARCEL_SIZE / 2);

                    plane.transform.position = DCLCharacterController.i.characterPosition.WorldToUnityPosition(position);

                    if (Configuration.ParcelSettings.VISUAL_LOADING_ENABLED)
                    {
                        Material finalMaterial = Utils.EnsureResourcesMaterial("Materials/DefaultPlane");
                        var matTransition = plane.AddComponent<MaterialTransitionController>();
                        matTransition.delay = 0;
                        matTransition.useHologram = false;
                        matTransition.fadeThickness = 20;
                        matTransition.OnDidFinishLoading(finalMaterial);
                    }
                    else
                    {
                        plane.GetComponent<MeshRenderer>().sharedMaterial =
                            Utils.EnsureResourcesMaterial("Materials/DefaultPlane");
                    }
                }
            }
        }

        public void Cleanup()
        {
            if (isReleased)
                return;

            if (DCLCharacterController.i)
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= OnPrecisionAdjust;

            if (!CommonScriptableObjects.rendererState.Get())
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
            return "gameObjectReference: " + this.ToString() + "\n" + sceneData.ToString();
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

        private CreateEntityMessage tmpCreateEntityMessage = new CreateEntityMessage();
        private const string EMPTY_GO_POOL_NAME = "Empty";
        public DecentralandEntity CreateEntity(string id)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("CreateEntity");
            tmpCreateEntityMessage.id = id;
            SceneController.i.OnMessageDecodeEnds?.Invoke("CreateEntity");

            if (entities.ContainsKey(tmpCreateEntityMessage.id))
            {
                return entities[tmpCreateEntityMessage.id];
            }

            var newEntity = new DecentralandEntity();
            newEntity.entityId = tmpCreateEntityMessage.id;

            // We need to manually create the Pool for empty game objects if it doesn't exist
            if (!PoolManager.i.ContainsPool(EMPTY_GO_POOL_NAME))
            {
                GameObject go = new GameObject();
                Pool pool = PoolManager.i.AddPool(EMPTY_GO_POOL_NAME, go, maxPrewarmCount: ENTITY_POOL_PREWARM_COUNT);
                pool.ForcePrewarm();
            }

            // As we know that the pool already exists, we just get one gameobject from it
            PoolableObject po = PoolManager.i.Get(EMPTY_GO_POOL_NAME);
            newEntity.gameObject = po.gameObject;
            newEntity.gameObject.name = "ENTITY_" + tmpCreateEntityMessage.id;
            newEntity.gameObject.transform.SetParent(gameObject.transform, false);
            newEntity.gameObject.SetActive(true);
            newEntity.scene = this;

            newEntity.OnCleanupEvent += po.OnCleanup;

            if (SceneController.i.useBoundariesChecker)
                newEntity.OnShapeUpdated += SceneController.i.boundariesChecker.AddEntityToBeChecked;

            entities.Add(tmpCreateEntityMessage.id, newEntity);

            OnEntityAdded?.Invoke(newEntity);

            return newEntity;
        }

        private RemoveEntityMessage tmpRemoveEntityMessage = new RemoveEntityMessage();

        public void RemoveEntity(string id, bool removeImmediatelyFromEntitiesList = true)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("RemoveEntity");
            tmpRemoveEntityMessage.id = id;
            SceneController.i.OnMessageDecodeEnds?.Invoke("RemoveEntity");
            if (entities.ContainsKey(tmpRemoveEntityMessage.id))
            {
                DecentralandEntity entity = entities[tmpRemoveEntityMessage.id];

                if (!entity.markedForCleanup)
                {
                    // This will also cleanup its children
                    CleanUpEntityRecursively(entity, removeImmediatelyFromEntitiesList);
                }

                if (SceneController.i.useBoundariesChecker)
                {
                    entity.OnShapeUpdated -= SceneController.i.boundariesChecker.AddEntityToBeChecked;
                    SceneController.i.boundariesChecker.RemoveEntityToBeChecked(entity);
                }

                entities.Remove(tmpRemoveEntityMessage.id);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError($"Couldn't remove entity with ID: {tmpRemoveEntityMessage.id} as it doesn't exist.");
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

            if (removeImmediatelyFromEntitiesList)
            {
                // Every entity ends up being removed through here
                entity.Cleanup();
                entities.Remove(entity.entityId);
            }
            else
            {
                parcelScenesCleaner.MarkForCleanup(entity);
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
                            parcelScenesCleaner.MarkRootEntityForCleanup(this, iterator.Current.Value);
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

        private SetEntityParentMessage tmpParentMessage = new SetEntityParentMessage();

        public void SetEntityParent(string entityId, string parentId)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("SetEntityParent");
            tmpParentMessage.entityId = entityId;
            tmpParentMessage.parentId = parentId;
            SceneController.i.OnMessageDecodeEnds?.Invoke("SetEntityParent");

            if (tmpParentMessage.entityId == tmpParentMessage.parentId)
            {
                return;
            }

            DecentralandEntity me = GetEntityForUpdate(tmpParentMessage.entityId);

            if (me != null && tmpParentMessage.parentId == "0")
            {
                me.SetParent(null);
                me.gameObject.transform.SetParent(gameObject.transform, false);
                return;
            }

            DecentralandEntity myParent = GetEntityForUpdate(tmpParentMessage.parentId);

            if (me != null && myParent != null)
            {
                me.SetParent(myParent);
            }
        }

        SharedComponentAttachMessage attachSharedComponentMessage = new SharedComponentAttachMessage();

        /**
          * This method is called when we need to attach a disposable component to the entity
          */
        public void SharedComponentAttach(string entityId, string id, string name)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("AttachEntityComponent");
            attachSharedComponentMessage.entityId = entityId;
            attachSharedComponentMessage.id = id;
            attachSharedComponentMessage.name = name;
            SceneController.i.OnMessageDecodeEnds?.Invoke("AttachEntityComponent");

            DecentralandEntity decentralandEntity = GetEntityForUpdate(attachSharedComponentMessage.entityId);

            if (decentralandEntity == null)
            {
                return;
            }

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(attachSharedComponentMessage.id, out disposableComponent)
                && disposableComponent != null)
            {
                disposableComponent.AttachTo(decentralandEntity);
            }
        }

        UUIDCallbackMessage uuidMessage = new UUIDCallbackMessage();
        EntityComponentCreateMessage createEntityComponentMessage = new EntityComponentCreateMessage();
        public BaseComponent EntityComponentCreateOrUpdate(string entityId, string name, int classIdNum, string data, out CleanableYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;

            SceneController.i.OnMessageDecodeStart?.Invoke("UpdateEntityComponent");
            createEntityComponentMessage.name = name;
            createEntityComponentMessage.classId = classIdNum;
            createEntityComponentMessage.entityId = entityId;
            createEntityComponentMessage.json = data;

            SceneController.i.OnMessageDecodeEnds?.Invoke("UpdateEntityComponent");

            DecentralandEntity entity = GetEntityForUpdate(createEntityComponentMessage.entityId);

            if (entity == null)
            {
                Debug.LogError($"scene '{sceneData.id}': Can't create entity component if the entity {createEntityComponentMessage.entityId} doesn't exist!");
                return null;
            }

            CLASS_ID_COMPONENT classId = (CLASS_ID_COMPONENT)createEntityComponentMessage.classId;

            if (classId == CLASS_ID_COMPONENT.TRANSFORM)
            {
                MessageDecoder.DecodeTransform(data, ref DCLTransform.model);

                if (entity.OnTransformChange != null)
                {
                    entity.OnTransformChange.Invoke(DCLTransform.model);
                }
                else
                {
                    entity.gameObject.transform.localPosition = DCLTransform.model.position;
                    entity.gameObject.transform.localRotation = DCLTransform.model.rotation;
                    entity.gameObject.transform.localScale = DCLTransform.model.scale;

                    SceneController.i.boundariesChecker?.AddEntityToBeChecked(entity);
                }

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

                OnPointerEvent.Model model = JsonUtility.FromJson<OnPointerEvent.Model>(createEntityComponentMessage.json);

                type = model.type;

                if (!entity.uuidComponents.ContainsKey(type))
                {
                    //NOTE(Brian): We have to contain it in a gameObject or it will be pooled with the components attached.
                    var go = new GameObject("UUID Component");
                    go.transform.SetParent(entity.gameObject.transform, false);

                    switch (type)
                    {
                        case OnClick.NAME:
                            newComponent = Utils.GetOrCreateComponent<OnClick>(go);
                            break;
                        case OnPointerDown.NAME:
                            newComponent = Utils.GetOrCreateComponent<OnPointerDown>(go);
                            break;
                        case OnPointerUp.NAME:
                            newComponent = Utils.GetOrCreateComponent<OnPointerUp>(go);
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
                        newComponent.UpdateFromJSON(createEntityComponentMessage.json);
                    }
                }
                else
                {
                    newComponent = EntityComponentUpdate(entity, classId, createEntityComponentMessage.json);
                }
            }

            if (newComponent != null && newComponent.isRoutineRunning)
                yieldInstruction = newComponent.yieldInstruction;

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

        SharedComponentCreateMessage sharedComponentCreatedMessage = new SharedComponentCreateMessage();

        public BaseDisposable SharedComponentCreate(string id, string name, int classId)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentCreated");
            sharedComponentCreatedMessage.id = id;
            sharedComponentCreatedMessage.name = name;
            sharedComponentCreatedMessage.classId = classId;
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentCreated");

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(sharedComponentCreatedMessage.id, out disposableComponent))
            {
                return disposableComponent;
            }

            BaseDisposable newComponent = null;

            switch ((CLASS_ID)sharedComponentCreatedMessage.classId)
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

                default:
                    Debug.LogError($"Unknown classId");
                    break;
            }

            if (newComponent != null)
            {
                newComponent.id = sharedComponentCreatedMessage.id;
                disposableComponents.Add(sharedComponentCreatedMessage.id, newComponent);

                if (state != State.READY)
                {
                    disposableNotReady.Add(id);
                }
            }

            return newComponent;
        }

        SharedComponentDisposeMessage sharedComponentDisposedMessage = new SharedComponentDisposeMessage();

        public void SharedComponentDispose(string id)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentDisposed");
            sharedComponentDisposedMessage.id = id;
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentDisposed");

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(sharedComponentDisposedMessage.id, out disposableComponent))
            {
                if (disposableComponent != null)
                {
                    disposableComponent.Dispose();
                }

                disposableComponents.Remove(sharedComponentDisposedMessage.id);
            }
        }

        EntityComponentRemoveMessage entityComponentRemovedMessage = new EntityComponentRemoveMessage();

        public void EntityComponentRemove(string entityId, string name)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentRemoved");

            entityComponentRemovedMessage.entityId = entityId;
            entityComponentRemovedMessage.name = name;

            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentRemoved");

            DecentralandEntity decentralandEntity = GetEntityForUpdate(entityComponentRemovedMessage.entityId);
            if (decentralandEntity == null)
            {
                return;
            }

            RemoveEntityComponent(decentralandEntity, entityComponentRemovedMessage.name);
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

        SharedComponentUpdateMessage sharedComponentUpdatedMessage = new SharedComponentUpdateMessage();

        public BaseDisposable SharedComponentUpdate(string id, string json, out CleanableYieldInstruction yieldInstruction)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentUpdated");
            BaseDisposable newComponent = SharedComponentUpdate(id, json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentUpdated");

            yieldInstruction = null;

            if (newComponent != null && newComponent.isRoutineRunning)
                yieldInstruction = newComponent.yieldInstruction;

            return newComponent;
        }

        public BaseDisposable SharedComponentUpdate(string id, string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentUpdated");
            sharedComponentUpdatedMessage.json = json;
            sharedComponentUpdatedMessage.id = id;
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentUpdated");

            BaseDisposable disposableComponent = null;

            if (disposableComponents.TryGetValue(sharedComponentUpdatedMessage.id, out disposableComponent))
            {
                disposableComponent.UpdateFromJSON(sharedComponentUpdatedMessage.json);
                return disposableComponent;
            }
            else
            {
                if (gameObject == null)
                {
                    Debug.LogError($"Unknown disposableComponent {sharedComponentUpdatedMessage.id} -- scene has been destroyed?");
                }
                else
                {
                    Debug.LogError($"Unknown disposableComponent {sharedComponentUpdatedMessage.id}", gameObject);
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

            blockerHandler?.CleanBlockers();

            SceneController.i.SendSceneReady(sceneData.id);
            RefreshName();

            OnSceneReady?.Invoke(this);
        }

        void OnRenderingStateChanged(bool isEnable, bool prevState)
        {
            if (isEnable)
            {
                parcelScenesCleaner.ForceCleanup();
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
