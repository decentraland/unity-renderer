using Cysharp.Threading.Tasks;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using DCL.Controllers.ParcelSceneDebug;
using System.Collections.Generic;
using DCL.Interface;
using MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Controllers
{
    public class ParcelScene : MonoBehaviour, IParcelScene
    {
        private const string NEW_CDN_FF = "ab-new-cdn";

        [Header("Debug")]
        [SerializeField]
        private bool renderOuterBoundsGizmo = true;

        public Dictionary<long, IDCLEntity> entities { get; private set; } = new Dictionary<long, IDCLEntity>();
        public IECSComponentsManagerLegacy componentsManagerLegacy { get; private set; }
        public LoadParcelScenesMessage.UnityParcelScene sceneData { get; protected set; }
        public HashSet<Vector2Int> parcels = new HashSet<Vector2Int>();
        public ISceneMetricsCounter metricsCounter { get; set; }
        public event System.Action<IDCLEntity> OnEntityAdded;
        public event System.Action<IDCLEntity> OnEntityRemoved;
        public event System.Action<LoadParcelScenesMessage.UnityParcelScene> OnSetData;
        public event System.Action<float> OnLoadingStateUpdated;

        public ContentProvider contentProvider { get; set; }

        public bool isTestScene { get; set; } = false;
        public bool isPersistent { get; set; } = false;
        public bool isPortableExperience { get; set; } = false;

        public float loadingProgress { get; private set; }

        [System.NonSerialized]
        public string sceneName;

        [System.NonSerialized]
        public bool unloadWithDistance = true;

        SceneDebugPlane sceneDebugPlane = null;

        public SceneLifecycleHandler sceneLifecycleHandler;

        public bool isReleased { get; private set; }

        private Bounds outerBounds = new Bounds();

        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();

        public void Awake()
        {
            CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
            componentsManagerLegacy = new ECSComponentsManagerLegacy(this);
            sceneLifecycleHandler = new SceneLifecycleHandler(this);
            metricsCounter = new SceneMetricsCounter(DataStore.i.sceneWorldObjects);
        }

        private void OnDestroy()
        {
            CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
            metricsCounter?.Dispose();
        }

        void OnDisable()
        {
            metricsCounter?.Disable();
        }

        private void Update()
        {
            if (sceneLifecycleHandler.state == SceneLifecycleHandler.State.READY
                && CommonScriptableObjects.rendererState.Get())
                SendMetricsEvent();
        }

        protected virtual string prettyName => sceneData.basePosition.ToString();

        public virtual async UniTask SetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            Assert.IsTrue(data.sceneNumber > 0, "Scene must have a valid scene number!");

            this.sceneData = data;

            contentProvider = new ContentProvider
            {
                baseUrl = data.baseUrl,
                contents = data.contents,
                sceneCid = data.id,
            };

            contentProvider.BakeHashes();

            if (featureFlags.IsFeatureEnabled(NEW_CDN_FF))
            {
                var sceneAb = await FetchSceneAssetBundles(data.id, data.baseUrlBundles);
                if (sceneAb.IsSceneConverted())
                {
                    contentProvider.assetBundles = sceneAb.GetConvertedFiles();
                    contentProvider.assetBundlesBaseUrl = sceneAb.GetBaseUrl();
                }
            }

            SetupPositionAndParcels();

            DataStore.i.sceneWorldObjects.AddScene(sceneData.sceneNumber);

            metricsCounter.Configure(sceneData.sceneNumber, sceneData.basePosition, sceneData.parcels.Length);
            metricsCounter.Enable();

            OnSetData?.Invoke(data);
        }

        private async UniTask<Asset_SceneAB> FetchSceneAssetBundles(string sceneId, string dataBaseUrlBundles)
        {
            AssetPromise_SceneAB promiseSceneAb = new AssetPromise_SceneAB(dataBaseUrlBundles, sceneId);
            AssetPromiseKeeper_SceneAB.i.Keep(promiseSceneAb);
            await promiseSceneAb.ToUniTask();
            return promiseSceneAb.asset;
        }

        void SetupPositionAndParcels()
        {
            gameObject.transform.position = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(sceneData.basePosition.x, sceneData.basePosition.y));

            parcels.Clear();

            // The scene's gameobject position should already be in 'unityposition'
            Vector3 baseParcelWorldPos = gameObject.transform.position;

            outerBounds.SetMinMax(new Vector3(baseParcelWorldPos.x, 0f, baseParcelWorldPos.z),
                new Vector3(baseParcelWorldPos.x + ParcelSettings.PARCEL_SIZE, 0f, baseParcelWorldPos.z + ParcelSettings.PARCEL_SIZE));

            for (int i = 0; i < sceneData.parcels.Length; i++)
            {
                // 1. Update outer bounds with parcel's size
                var parcel = sceneData.parcels[i];

                Vector3 parcelWorldPos = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(parcel.x, parcel.y));
                outerBounds.Encapsulate(new Vector3(parcelWorldPos.x, 0, parcelWorldPos.z));
                outerBounds.Encapsulate(new Vector3(parcelWorldPos.x + ParcelSettings.PARCEL_SIZE, 0, parcelWorldPos.z + ParcelSettings.PARCEL_SIZE));

                // 2. add parcel to collection
                parcels.Add(parcel);
            }

            // Apply outer bounds extra threshold
            outerBounds.SetMinMax(new Vector3(outerBounds.min.x - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD, 0f, outerBounds.min.z - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD),
                new Vector3(outerBounds.max.x + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD, 0f, outerBounds.max.z + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD));
        }

        void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            Vector3 currentSceneWorldPos = Utils.GridToWorldPosition(sceneData.basePosition.x, sceneData.basePosition.y);
            Vector3 oldSceneUnityPos = gameObject.transform.position;
            Vector3 newSceneUnityPos = PositionUtils.WorldToUnityPosition(currentSceneWorldPos);

            gameObject.transform.position = newSceneUnityPos;
            outerBounds.center += newSceneUnityPos - oldSceneUnityPos;
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
            if (EnvironmentSettings.DEBUG && sceneData.parcels != null && sceneDebugPlane == null) { sceneDebugPlane = new SceneDebugPlane(sceneData, gameObject.transform); }
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
            if (isReleased || gameObject == null)
                return;

            if (sceneDebugPlane != null)
            {
                sceneDebugPlane.Dispose();
                sceneDebugPlane = null;
            }

            componentsManagerLegacy.DisposeAllSceneComponents();

            if (immediate) //!CommonScriptableObjects.rendererState.Get())
            {
                RemoveAllEntitiesImmediate();
                PoolManager.i.Cleanup(true, true);
                DataStore.i.sceneWorldObjects.RemoveScene(sceneData.sceneNumber);
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
                    DataStore.i.sceneWorldObjects.RemoveScene(sceneData.sceneNumber);
                }
            }

            isReleased = true;
        }

        public override string ToString()
        {
            return "Parcel Scene: " + base.ToString() + "\n" + sceneData;
        }

        public string GetSceneName()
        {
            return string.IsNullOrEmpty(sceneName) ? "Unnamed" : sceneName;
        }

        public HashSet<Vector2Int> GetParcels() =>
            parcels;

        public bool IsInsideSceneBoundaries(Bounds objectBounds)
        {
            if (isPersistent)
                return true;

            if (!IsInsideSceneBoundaries(objectBounds.min + CommonScriptableObjects.worldOffset, objectBounds.max.y))
                return false;

            if (!IsInsideSceneBoundaries(objectBounds.max + CommonScriptableObjects.worldOffset, objectBounds.max.y))
                return false;

            return true;
        }

        public virtual bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0f)
        {
            if (isPersistent)
                return true;

            if (parcels.Count == 0)
                return false;

            float heightLimit = metricsCounter.maxCount.sceneHeight;

            if (height > heightLimit)
                return false;

            return parcels.Contains(gridPosition);
        }

        public virtual bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f)
        {
            if (isPersistent)
                return true;

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

        public bool IsInsideSceneOuterBoundaries(Bounds objectBounds)
        {
            if (isPersistent)
                return true;

            Vector3 objectBoundsMin = new Vector3(objectBounds.min.x, 0f, objectBounds.min.z);
            Vector3 objectBoundsMax = new Vector3(objectBounds.max.x, 0f, objectBounds.max.z);
            bool isInsideOuterBoundaries = outerBounds.Contains(objectBoundsMin) && outerBounds.Contains(objectBoundsMax);

            return isInsideOuterBoundaries;
        }

        public bool IsInsideSceneOuterBoundaries(Vector3 objectUnityPosition)
        {
            if (isPersistent)
                return true;

            objectUnityPosition.y = 0f;
            return outerBounds.Contains(objectUnityPosition);
        }

        private void OnDrawGizmosSelected()
        {
            if (!renderOuterBoundsGizmo) return;

            Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f);
            Gizmos.DrawCube(outerBounds.center, outerBounds.size + Vector3.up);

            Gizmos.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f);
            Bounds parcelBounds = new Bounds();

            foreach (Vector2Int parcel in parcels)
            {
                Vector3 parcelSceneUnityPos = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(parcel.x, parcel.y));
                parcelBounds.center = parcelSceneUnityPos + new Vector3(8f, 0f, 8f);
                parcelBounds.size = new Vector3(16f, 0.1f, 16f);
                Gizmos.DrawCube(parcelBounds.center, parcelBounds.size);
            }
        }

        public IDCLEntity GetEntityById(string entityId)
        {
            throw new System.NotImplementedException();
        }

        public Transform GetSceneTransform()
        {
            return transform;
        }

        public IDCLEntity CreateEntity(long id)
        {
            if (entities.ContainsKey(id)) { return entities[id]; }

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
                newEntity.OnShapeUpdated += OnEntityShapeUpdated;

            entities.Add(id, newEntity);

            DataStore.i.sceneWorldObjects.sceneData[sceneData.sceneNumber].owners.Add(id);

            OnEntityAdded?.Invoke(newEntity);

            Environment.i.world.sceneBoundsChecker.AddEntityToBeChecked(newEntity, runPreliminaryEvaluation: true);

            return newEntity;
        }

        void OnEntityShapeUpdated(IDCLEntity entity)
        {
            Environment.i.world.sceneBoundsChecker.AddEntityToBeChecked(entity, runPreliminaryEvaluation: true);
        }

        public void RemoveEntity(long id, bool removeImmediatelyFromEntitiesList = true)
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

                if (data.ContainsKey(sceneData.sceneNumber)) { data[sceneData.sceneNumber].owners.Remove(id); }
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
            using (var iterator = entity.children.GetEnumerator())
            {
                while (iterator.MoveNext()) { CleanUpEntityRecursively(iterator.Current.Value, removeImmediatelyFromEntitiesList); }
            }

            OnEntityRemoved?.Invoke(entity);

            if (Environment.i.world.sceneBoundsChecker.enabled)
            {
                entity.OnShapeUpdated -= OnEntityShapeUpdated;
                Environment.i.world.sceneBoundsChecker.RemoveEntity(entity, removeIfPersistent: true, resetState: true);
            }

            if (removeImmediatelyFromEntitiesList)
            {
                // Every entity ends up being removed through here
                entity.Cleanup();
                entities.Remove(entity.entityId);
            }
            else { Environment.i.platform.parcelScenesCleaner.MarkForCleanup(entity); }
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

                // TODO: Does it make sense that 'RemoveAllEntities()' destroys the whole scene GameObject?
                if (gameObject != null)
                    Destroy(gameObject);
            }
        }

        private void RemoveAllEntitiesImmediate()
        {
            RemoveAllEntities(instant: true);
        }

        public void SetEntityParent(long entityId, long parentId)
        {
            if (entityId == parentId) { return; }

            IDCLEntity me = GetEntityById(entityId);

            if (me == null)
                return;

            Environment.i.platform.cullingController.MarkDirty();
            Environment.i.platform.physicsSyncController.MarkDirty();

            DataStore_World worldData = DataStore.i.Get<DataStore_World>();
            Transform avatarTransform = worldData.avatarTransform.Get();
            Transform firstPersonCameraTransform = worldData.fpsTransform.Get();

            // CONST_THIRD_PERSON_CAMERA_ENTITY_REFERENCE is for compatibility purposes
            if (parentId == (long)SpecialEntityId.FIRST_PERSON_CAMERA_ENTITY_REFERENCE ||
                parentId == (long)SpecialEntityId.THIRD_PERSON_CAMERA_ENTITY_REFERENCE)
            {
                if (firstPersonCameraTransform == null)
                {
                    Debug.LogError("FPS transform is null when trying to set parent! " + sceneData.sceneNumber);
                    return;
                }

                // In this case, the entity will attached to the first person camera
                // On first person mode, the entity will rotate with the camera. On third person mode, the entity will rotate with the avatar
                me.SetParent(null);
                me.gameObject.transform.SetParent(firstPersonCameraTransform, false);
                Environment.i.world.sceneBoundsChecker.RemoveEntity(me, removeIfPersistent: true, resetState: true);
                Environment.i.world.sceneBoundsChecker.AddEntityToBeChecked(me, isPersistent: true, runPreliminaryEvaluation: true);
                return;
            }

            if (parentId == (long)SpecialEntityId.AVATAR_ENTITY_REFERENCE ||
                parentId == (long)SpecialEntityId
                   .AVATAR_POSITION_REFERENCE) // AvatarPositionEntityReference is for compatibility purposes
            {
                if (avatarTransform == null)
                {
                    Debug.LogError("Avatar transform is null when trying to set parent! " + sceneData.sceneNumber);
                    return;
                }

                // In this case, the entity will be attached to the avatar
                // It will simply rotate with the avatar, regardless of where the camera is pointing
                me.SetParent(null);
                me.gameObject.transform.SetParent(avatarTransform, false);
                Environment.i.world.sceneBoundsChecker.RemoveEntity(me, removeIfPersistent: true, resetState: true);
                Environment.i.world.sceneBoundsChecker.AddEntityToBeChecked(me, isPersistent: true, runPreliminaryEvaluation: true);
                return;
            }

            // Remove from persistent checks if it was formerly added as child of avatarTransform or fpsTransform
            if (me.gameObject.transform.parent == avatarTransform ||
                me.gameObject.transform.parent == firstPersonCameraTransform)
            {
                if (Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(me))
                    Environment.i.world.sceneBoundsChecker.RemoveEntity(me, removeIfPersistent: true);
            }

            if (parentId == (long)SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                // The entity will be child of the scene directly
                me.SetParent(null);
                me.gameObject.transform.SetParent(gameObject.transform, false);
            }
            else
            {
                IDCLEntity myParent = GetEntityById(parentId);

                if (myParent != null) { me.SetParent(myParent); }
            }

            // After reparenting the Entity may end up outside the scene boundaries
            Environment.i.world.sceneBoundsChecker?.AddEntityToBeChecked(me, runPreliminaryEvaluation: true);
        }

        protected virtual void SendMetricsEvent()
        {
            if (Time.frameCount % 10 == 0)
                metricsCounter.SendEvent();
        }

        public IDCLEntity GetEntityById(long entityId)
        {
            if (!entities.TryGetValue(entityId, out IDCLEntity entity)) { return null; }

            //NOTE(Brian): This is for removing stray null references? This should never happen.
            //             Maybe move to a different 'clean-up' method to make this method have a single responsibility?.
            if (entity == null || entity.gameObject == null)
            {
                entities.Remove(entityId);
                return null;
            }

            return entity;
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
                    return $"{baseState}:{prettyName} - {sceneLifecycleHandler.sceneResourcesLoadTracker.GetStateString()}";
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
                    sceneLifecycleHandler.sceneResourcesLoadTracker.PrintWaitingResourcesDebugInfo();
                    break;

                default:
                    Debug.Log($"The scene {sceneData.sceneNumber} is not waiting for any components. Its current state is " + sceneLifecycleHandler.state);
                    break;
            }
        }

        [ContextMenu("Get Scene Info")]
        public void GetSceneDebugInfo()
        {
            Debug.Log("-----------------");
            Debug.Log("SCENE DEBUG INFO:");
            Debug.Log($"Scene Id: {sceneData.id}");
            Debug.Log($"Scene Number: {sceneData.sceneNumber}");
            Debug.Log($"Scene Coords: {sceneData.basePosition.ToString()}");
            Debug.Log($"Scene State: {sceneLifecycleHandler.state.ToString()}");
            Debug.Log("-----------------");
        }

        /// <summary>
        /// Calculates the current loading progress of the scene and raise the event OnLoadingStateUpdated with the percentage.
        /// </summary>
        public void CalculateSceneLoadingState()
        {
            loadingProgress = 0f;

            if (sceneLifecycleHandler.state == SceneLifecycleHandler.State.WAITING_FOR_COMPONENTS ||
                sceneLifecycleHandler.state == SceneLifecycleHandler.State.READY) { loadingProgress = sceneLifecycleHandler.loadingProgress; }

            OnLoadingStateUpdated?.Invoke(loadingProgress);
        }

        public bool IsInitMessageDone()
        {
            return sceneLifecycleHandler.state == SceneLifecycleHandler.State.READY
                   || sceneLifecycleHandler.state == SceneLifecycleHandler.State.WAITING_FOR_COMPONENTS;
        }
    }
}
