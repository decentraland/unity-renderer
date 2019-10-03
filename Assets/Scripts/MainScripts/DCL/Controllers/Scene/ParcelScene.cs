using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Environment = DCL.Configuration.Environment;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Collections;

namespace DCL.Controllers
{
    public class ParcelScene : MonoBehaviour, ICleanable
    {
        public static bool VERBOSE = false;
        static Vector3 MORDOR = new Vector3(1000, 1000, 1000);

        enum State
        {
            NOT_READY,
            WAITING_FOR_INIT_MESSAGES,
            WAITING_FOR_COMPONENTS,
            READY,
        }

        private const string PARCEL_BLOCKER_PREFAB = "Prefabs/ParcelBlocker";
        private const float MAX_CLEANUP_BUDGET = 0.014f;
        private const float CLEANUP_NOISE = 0.0025f;
        private const int ENTITY_POOL_PREWARM_COUNT = 2000;

        public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();
        public Dictionary<string, BaseDisposable> disposableComponents = new Dictionary<string, BaseDisposable>();
        public LoadParcelScenesMessage.UnityParcelScene sceneData { get; protected set; }
        public SceneController ownerController;
        public SceneMetricsController metricsController;
        public UIScreenSpace uiScreenSpace;

        public event System.Action<DecentralandEntity> OnEntityAdded;
        public event System.Action<DecentralandEntity> OnEntityRemoved;
        public ContentProvider contentProvider;
        public int disposableNotReadyCount => disposableNotReady.Count;

        [System.NonSerialized]
        public bool isTestScene = false;

        [System.NonSerialized]
        public bool isPersistent = false;

        [System.NonSerialized]
        public bool unloadWithDistance = true;

        private readonly List<GameObject> blockers = new List<GameObject>();
        private readonly List<string> disposableNotReady = new List<string>();
        private List<string> entitiesMarkedForRemoval = new List<string>();
        private bool flaggedToUnload = false;
        private bool isReleased = false;
        private State state = State.NOT_READY;
        public SceneBoundariesChecker boundariesChecker { private set; get; }

        private static GameObject blockerPrefab;

        public void Awake()
        {
            state = State.NOT_READY;

            metricsController = new SceneMetricsController(this);
            metricsController.Enable();

            if (SceneController.i.isDebugMode)
                boundariesChecker = new SceneBoundariesDebugModeChecker(this);
            else
                boundariesChecker = new SceneBoundariesChecker(this);

            if (DCLCharacterController.i)
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust += OnPrecisionAdjust;
        }

        private void Update()
        {
            if (state == State.READY && RenderingController.i.renderingEnabled)
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

            gameObject.transform.position = DCLCharacterController.i.characterPosition.WorldToUnityPosition(Utils.GridToWorldPosition(data.basePosition.x, data.basePosition.y));

#if UNITY_EDITOR
            //NOTE(Brian): Don't generate parcel blockers if debugScenes is active and is not the desired scene.
            if (SceneController.i.debugScenes && SceneController.i.debugSceneCoords != data.basePosition)
            {
                SetSceneReady();
                return;
            }
#endif
            SetupBlockers(data.parcels);
        }

        public void CleanBlockers()
        {
            int blockersCount = blockers.Count;
            for (int i = 0; i < blockersCount; i++)
            {
                Destroy(blockers[i]);
            }

            blockers.Clear();
        }

        private void SetupBlockers(Vector2Int[] parcels)
        {
            if (blockerPrefab == null)
                blockerPrefab = Resources.Load<GameObject>(PARCEL_BLOCKER_PREFAB);

            CleanBlockers();

            int parcelsLength = parcels.Length;
            for (int i = 0; i < parcelsLength; i++)
            {
                Vector2Int pos = parcels[i];
                var blocker = Instantiate(blockerPrefab, transform);
                blocker.transform.position = DCLCharacterController.i.characterPosition.WorldToUnityPosition(Utils.GridToWorldPosition(pos.x, pos.y)) + (Vector3.up * blockerPrefab.transform.localPosition.y) + new Vector3(ParcelSettings.PARCEL_SIZE / 2, 0, ParcelSettings.PARCEL_SIZE / 2);

                float sceneHeight = metricsController.GetLimits().sceneHeight;
                blocker.transform.position = new Vector3(blocker.transform.position.x, sceneHeight / 2, blocker.transform.position.z);
                blocker.transform.localScale = new Vector3(blocker.transform.localScale.x, sceneHeight, blocker.transform.localScale.z);
                blockers.Add(blocker);
            }
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
            if (Environment.DEBUG && sceneData.parcels != null)
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

            if (!RenderingController.i.renderingEnabled)
            {
                RemoveAllEntitiesImmediate();
            }
            else
            {
                if (entities.Count > 0)
                {
                    this.gameObject.transform.position = MORDOR;

                    if (DCLCharacterController.i)
                        DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= OnPrecisionAdjust;

                    CoroutineStarter.Start(RemoveAllEntitiesCoroutine());
                }
                else
                {
                    Destroy(this.gameObject);

                    if (DCLCharacterController.i)
                        DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= OnPrecisionAdjust;
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
            if (sceneData.parcels == null) return false;

            float heightLimit = metricsController.GetLimits().sceneHeight;

            for (int i = 0; i < sceneData.parcels.Length; i++)
            {
                if (height > heightLimit) continue;

                if (sceneData.parcels[i] == gridPosition) return true;
            }

            return false;
        }

        public virtual bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f)
        {
            if (sceneData.parcels == null) return false;

            float heightLimit = metricsController.GetLimits().sceneHeight;

            for (int i = 0; i < sceneData.parcels.Length; i++)
            {
                if (height > heightLimit) continue;

                if (worldPosition.x < sceneData.parcels[i].x * ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD
                    && worldPosition.x > sceneData.parcels[i].x * ParcelSettings.PARCEL_SIZE - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD
                    && worldPosition.z < sceneData.parcels[i].y * ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD
                    && worldPosition.z > sceneData.parcels[i].y * ParcelSettings.PARCEL_SIZE - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD)
                {
                    return true;
                }
            }

            return false;
        }

        private CreateEntityMessage tmpCreateEntityMessage = new CreateEntityMessage();
        private const string EMPTY_GO_POOL_NAME = "Empty";
        public DecentralandEntity CreateEntity(string id, string json)
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
            newEntity.OnShapeUpdated += boundariesChecker.EvaluateEntityPosition;

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
                if (!entitiesMarkedForRemoval.Contains(tmpRemoveEntityMessage.id))
                {
                    DecentralandEntity entity = entities[tmpRemoveEntityMessage.id];

                    // This will also cleanup its children
                    CleanUpEntityRecursively(entity, !removeImmediatelyFromEntitiesList);
                }
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError($"Couldn't remove entity with ID: {tmpRemoveEntityMessage.id} as it doesn't exist.");
            }
#endif
        }

        void CleanUpEntityRecursively(DecentralandEntity entity, bool delayed)
        {
            // Iterate through all entity children
            using (var iterator = entity.children.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    CleanUpEntityRecursively(iterator.Current.Value, delayed);
                }
            }

            OnEntityRemoved?.Invoke(entity);

            MarkForRemoval(entity);
        }

        void MarkForRemoval(DecentralandEntity entity)
        {
            if (!entitiesMarkedForRemoval.Contains(entity.entityId))
            {
                entitiesMarkedForRemoval.Add(entity.entityId);
            }
        }

        IEnumerator RemoveAllEntitiesCoroutine(bool instant = false)
        {
            if (!instant)
                yield return WaitForSecondsCache.Get(Random.Range(0, CLEANUP_NOISE));

            //NOTE(Brian): We need to remove only the rootEntities. 
            //             If we don't, duplicated entities will get removed when destroying 
            //             recursively, making this more complicated than it should.
            float maxBudget = MAX_CLEANUP_BUDGET;
            float lastTime = DCLTime.realtimeSinceStartup;

            using (var iterator = entities.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.parent == null)
                    {
                        DecentralandEntity entity = iterator.Current.Value;

                        RemoveEntity(entity.entityId, removeImmediatelyFromEntitiesList: instant);

                        if (!instant && DCLTime.realtimeSinceStartup - lastTime >= maxBudget)
                        {
                            yield return null;
                            lastTime = DCLTime.realtimeSinceStartup;
                        }
                    }
                }
            }

            int entitiesMarkedForRemovalCount = entitiesMarkedForRemoval.Count;

            for (int i = 0; i < entitiesMarkedForRemovalCount; i++)
            {
                DecentralandEntity entity = entities[entitiesMarkedForRemoval[i]];

                entities.Remove(entitiesMarkedForRemoval[i]);

                entity.SetParent(null);
                entity.Cleanup();

                if (!instant && DCLTime.realtimeSinceStartup - lastTime >= maxBudget)
                {
                    yield return null;
                    lastTime = DCLTime.realtimeSinceStartup;
                }
            }

            Destroy(this.gameObject);

            if (DCLCharacterController.i)
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= OnPrecisionAdjust;
        }

        private void RemoveAllEntitiesImmediate()
        {
            var enumerator = RemoveAllEntitiesCoroutine(instant: true);
            //IEnumerator needs call MoveNext to be executed
            enumerator.MoveNext();
        }

        private SetEntityParentMessage tmpParentMessage = new SetEntityParentMessage();

        public void SetEntityParent(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("SetEntityParent");
            tmpParentMessage.FromJSON(json);
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
        public void SharedComponentAttach(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("AttachEntityComponent");
            attachSharedComponentMessage.FromJSON(json);
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

        public BaseComponent EntityComponentCreateOrUpdate(string json, out CleanableYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;

            SceneController.i.OnMessageDecodeStart?.Invoke("UpdateEntityComponent");

            createEntityComponentMessage.FromJSON(json);

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
                JsonUtility.FromJsonOverwrite(createEntityComponentMessage.json, DCLTransform.model);

                if (entity.OnTransformChange != null)
                {
                    entity.OnTransformChange.Invoke(DCLTransform.model);
                }
                else
                {
                    entity.gameObject.transform.localPosition = DCLTransform.model.position;
                    entity.gameObject.transform.localRotation = DCLTransform.model.rotation;
                    entity.gameObject.transform.localScale = DCLTransform.model.scale;

                    boundariesChecker.EvaluateEntityPosition(entity);
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

                UUIDComponent.Model model = JsonUtility.FromJson<UUIDComponent.Model>(createEntityComponentMessage.json);

                type = model.type;

                if (!entity.uuidComponents.ContainsKey(type))
                {
                    switch (type)
                    {
                        case OnClickComponent.NAME:
                            newComponent = Utils.GetOrCreateComponent<OnClickComponent>(entity.gameObject);
                            break;
                        case OnPointerDownComponent.NAME:
                            newComponent = Utils.GetOrCreateComponent<OnPointerDownComponent>(entity.gameObject);
                            break;
                        case OnPointerUpComponent.NAME:
                            newComponent = Utils.GetOrCreateComponent<OnPointerUpComponent>(entity.gameObject);
                            break;
                    }

                    if (newComponent != null)
                    {
                        UUIDComponent uuidComponent = newComponent as UUIDComponent;

                        if (uuidComponent != null)
                        {
                            uuidComponent.SetForEntity(this, entity, model);
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
                    newComponent = EntityUUIDComponentUpdate(entity, type, createEntityComponentMessage.json);
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
        public UUIDComponent EntityUUIDComponentUpdate(DecentralandEntity entity, string type,
            string componentJson)
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
            targetComponent.UpdateFromJSON(componentJson);


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

        public BaseDisposable SharedComponentCreate(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentCreated");
            sharedComponentCreatedMessage.FromJSON(json);
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

                default:
                    Debug.LogError($"Unknown classId {json}");
                    break;
            }

            if (newComponent != null)
            {
                newComponent.id = sharedComponentCreatedMessage.id;
                disposableComponents.Add(sharedComponentCreatedMessage.id, newComponent);
            }

            return newComponent;
        }

        SharedComponentDisposeMessage sharedComponentDisposedMessage = new SharedComponentDisposeMessage();

        public void SharedComponentDispose(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentDisposed");
            sharedComponentDisposedMessage.FromJSON(json);
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

        public void EntityComponentRemove(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentRemoved");

            entityComponentRemovedMessage.FromJSON(json);

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
                case OnClickComponent.NAME:
                    RemoveUUIDComponentType<OnClickComponent>(entity, componentName);
                    return;
                case OnPointerDownComponent.NAME:
                    RemoveUUIDComponentType<OnPointerDownComponent>(entity, componentName);
                    return;
                case OnPointerUpComponent.NAME:
                    RemoveUUIDComponentType<OnPointerUpComponent>(entity, componentName);
                    return;
            }
        }

        SharedComponentUpdateMessage sharedComponentUpdatedMessage = new SharedComponentUpdateMessage();

        public BaseDisposable SharedComponentUpdate(string json, out CleanableYieldInstruction yieldInstruction)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentUpdated");
            BaseDisposable newComponent = SharedComponentUpdate(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentUpdated");

            yieldInstruction = null;

            if (newComponent != null && newComponent.isRoutineRunning)
                yieldInstruction = newComponent.yieldInstruction;

            return newComponent;
        }

        public BaseDisposable SharedComponentUpdate(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentUpdated");
            sharedComponentUpdatedMessage.FromJSON(json);
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
            if (state == State.READY)
            {
                Debug.LogWarning($"Init messages done after ready?! {sceneData.basePosition}", gameObject);
                return;
            }

            state = State.WAITING_FOR_COMPONENTS;
            RefreshName();
            disposableNotReady.Clear();

            if (disposableComponents.Count > 0)
            {
                //NOTE(Brian): Here, we have to split the iterations. If not, we will get repeated calls of
                //             SetSceneReady(), as the disposableNotReady count is 1 and gets to 0
                //             in each OnDisposableReady() call.

                using (var iterator = disposableComponents.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        disposableNotReady.Add(iterator.Current.Key);
                    }
                }

                var listCopy = new List<string>(disposableNotReady);

                int listCopyCount = listCopy.Count;

                for (int i = 0; i < listCopyCount; i++)
                {
                    disposableComponents[listCopy[i]].CallWhenReady(OnDisposableReady);
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
            {
                return;
            }

            if (VERBOSE)
            {
                Debug.Log($"{sceneData.basePosition} Scene Ready!");
            }

            state = State.READY;

            CleanBlockers();
            SceneController.i.SendSceneReady(sceneData.id);
            RefreshName();
        }
    }
}
