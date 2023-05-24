using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCL.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Components;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DCL
{
    public class SceneController : ISceneController
    {
        public static bool VERBOSE = false;
        const int SCENE_MESSAGES_PREWARM_COUNT = 100000;

        public bool enabled { get; set; } = true;
        internal BaseVariable<Transform> isPexViewerInitialized => DataStore.i.experiencesViewer.isInitialized;

        //TODO(Brian): Move to WorldRuntimePlugin later
        private Coroutine deferredDecodingCoroutine;

        private CancellationTokenSource tokenSource;
        private IMessagingControllersManager messagingControllersManager => Environment.i.messaging.manager;

        public EntityIdHelper entityIdHelper { get; } = new EntityIdHelper();

        public void Initialize()
        {
            tokenSource = new CancellationTokenSource();
            sceneSortDirty = true;
            positionDirty = true;
            lastSortFrame = 0;
            enabled = true;

            DataStore.i.debugConfig.isDebugMode.OnChange += OnDebugModeSet;

            SetupDeferredRunners();

            DataStore.i.player.playerGridPosition.OnChange += SetPositionDirty;
            CommonScriptableObjects.sceneNumber.OnChange += OnCurrentSceneNumberChange;

            // TODO(Brian): Move this later to Main.cs
            if ( !EnvironmentSettings.RUNNING_TESTS )
            {
                PrewarmSceneMessagesPool();
            }

            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        }
        private void SetupDeferredRunners()
        {
#if UNITY_WEBGL
            deferredDecodingCoroutine = CoroutineStarter.Start(DeferredDecodingAndEnqueue());
#else
            CancellationToken tokenSourceToken = tokenSource.Token;
            TaskUtils.Run(async () => await WatchForNewChunksToDecode(tokenSourceToken), cancellationToken: tokenSourceToken).Forget();
#endif
        }

        private void PrewarmSceneMessagesPool()
        {
            if (prewarmSceneMessagesPool)
            {
                for (int i = 0; i < SCENE_MESSAGES_PREWARM_COUNT; i++)
                {
                    sceneMessagesPool.Enqueue(new QueuedSceneMessage_Scene());
                }
            }

            if (prewarmEntitiesPool)
            {
                PoolManagerFactory.EnsureEntityPool(prewarmEntitiesPool);
            }
        }

        private void OnDebugModeSet(bool current, bool previous)
        {
            if (current == previous)
                return;

            if (current)
            {
                Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_RedBox());
            }
            else
            {
                Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple());
            }
        }

        public void Dispose()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();

            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

            PoolManager.i.OnGet -= Environment.i.platform.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet -= Environment.i.platform.cullingController.objectsTracker.MarkDirty;

            DataStore.i.player.playerGridPosition.OnChange -= SetPositionDirty;
            DataStore.i.debugConfig.isDebugMode.OnChange -= OnDebugModeSet;

            CommonScriptableObjects.sceneNumber.OnChange -= OnCurrentSceneNumberChange;

            UnloadAllScenes(includePersistent: true);

            if (deferredDecodingCoroutine != null)
                CoroutineStarter.Stop(deferredDecodingCoroutine);
        }

        public void Update()
        {
            if (!enabled)
                return;

            if (lastSortFrame != Time.frameCount && sceneSortDirty)
            {
                lastSortFrame = Time.frameCount;
                sceneSortDirty = false;
                SortScenesByDistance();
            }
        }

        public void LateUpdate()
        {
            if (!enabled)
                return;

            Environment.i.platform.physicsSyncController.Sync();
        }

        //======================================================================

        #region MESSAGES_HANDLING

        //======================================================================

#if UNITY_EDITOR
        public delegate void ProcessDelegate(int sceneNumber, string method);

        public event ProcessDelegate OnMessageProcessInfoStart;
        public event ProcessDelegate OnMessageProcessInfoEnds;
#endif
        public bool deferredMessagesDecoding { get; set; } = false;

        readonly ConcurrentQueue<string> chunksToDecode = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<QueuedSceneMessage_Scene> messagesToProcess = new ConcurrentQueue<QueuedSceneMessage_Scene>();

        const float MAX_TIME_FOR_DECODE = 0.005f;

        public bool ProcessMessage(QueuedSceneMessage_Scene msgObject, out CustomYieldInstruction yieldInstruction)
        {
            int sceneNumber = msgObject.sceneNumber;
            string method = msgObject.method;

            yieldInstruction = null;

            IParcelScene scene;
            bool res = false;
            IWorldState worldState = Environment.i.world.state;
            DebugConfig debugConfig = DataStore.i.debugConfig;

            if (worldState.TryGetScene(sceneNumber, out scene))
            {
#if UNITY_EDITOR
                if (debugConfig.soloScene && scene is GlobalScene && debugConfig.ignoreGlobalScenes)
                {
                    return false;
                }
#endif
                if (!scene.GetSceneTransform().gameObject.activeInHierarchy)
                {
                    return true;
                }

#if UNITY_EDITOR
                OnMessageProcessInfoStart?.Invoke(sceneNumber, method);
#endif
                ProfilingEvents.OnMessageProcessStart?.Invoke(method);

                ProcessMessage(scene as ParcelScene, method, msgObject.payload, out yieldInstruction);

                ProfilingEvents.OnMessageProcessEnds?.Invoke(method);

#if UNITY_EDITOR
                OnMessageProcessInfoEnds?.Invoke(sceneNumber, method);
#endif

                res = true;
            }

            else
            {
                res = false;
            }

            sceneMessagesPool.Enqueue(msgObject);

            return res;
        }

        private void ProcessMessage(ParcelScene scene, string method, object msgPayload, out CustomYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;
            IDelayedComponent delayedComponent = null;

            try
            {
                switch (method)
                {
                    case MessagingTypes.ENTITY_CREATE:
                        {
                            if (msgPayload is Protocol.CreateEntity payload)
                                scene.CreateEntity(entityIdHelper.EntityFromLegacyEntityString(payload.entityId));
                            break;
                        }
                    case MessagingTypes.ENTITY_REPARENT:
                        {
                            if (msgPayload is Protocol.SetEntityParent payload)
                                scene.SetEntityParent(entityIdHelper.EntityFromLegacyEntityString(payload.entityId),entityIdHelper.EntityFromLegacyEntityString(payload.parentId));
                            break;
                        }
                    case MessagingTypes.PB_SHARED_COMPONENT_UPDATE:
                    {
                        if (msgPayload is Decentraland.Sdk.Ecs6.ComponentUpdatedBody payload){
                            if (payload.ComponentData != null) {
                                delayedComponent = scene.componentsManagerLegacy.SceneSharedComponentUpdate(payload.Id, payload.ComponentData) as IDelayedComponent;
                            }
                        }
                        break;
                    }
                    case MessagingTypes.PB_ENTITY_COMPONENT_CREATE_OR_UPDATE:
                        {
                            if (msgPayload is Decentraland.Sdk.Ecs6.UpdateEntityComponentBody payload) {

                                if (payload.ComponentData != null) {
                                    delayedComponent = scene.componentsManagerLegacy.EntityComponentCreateOrUpdate(entityIdHelper.EntityFromLegacyEntityString(payload.EntityId),
                                    (CLASS_ID_COMPONENT) payload.ClassId, payload.ComponentData) as IDelayedComponent;
                                }
                            }
                            break;
                        }
                    case MessagingTypes.ENTITY_COMPONENT_DESTROY:
                        {
                            if (msgPayload is Protocol.EntityComponentDestroy payload)
                                scene.componentsManagerLegacy.EntityComponentRemove(entityIdHelper.EntityFromLegacyEntityString(payload.entityId), payload.name);
                            break;
                        }
                    case MessagingTypes.SHARED_COMPONENT_ATTACH:
                        {
                            if (msgPayload is Protocol.SharedComponentAttach payload)
                                scene.componentsManagerLegacy.SceneSharedComponentAttach(entityIdHelper.EntityFromLegacyEntityString(payload.entityId), payload.id);
                            break;
                        }
                    case MessagingTypes.SHARED_COMPONENT_CREATE:
                        {
                            if (msgPayload is Protocol.SharedComponentCreate payload)
                                scene.componentsManagerLegacy.SceneSharedComponentCreate(payload.id, payload.classId);
                            break;
                        }
                    case MessagingTypes.SHARED_COMPONENT_DISPOSE:
                        {
                            if (msgPayload is Protocol.SharedComponentDispose payload)
                                scene.componentsManagerLegacy.SceneSharedComponentDispose(payload.id);
                            break;
                        }
                    case MessagingTypes.ENTITY_DESTROY:
                        {
                            if (msgPayload is Protocol.RemoveEntity payload)
                                scene.RemoveEntity(entityIdHelper.EntityFromLegacyEntityString(payload.entityId));
                            break;
                        }
                    case MessagingTypes.INIT_DONE:
                        {
                            if (!scene.IsInitMessageDone())
                                scene.sceneLifecycleHandler.SetInitMessagesDone();
                            break;
                        }
                    case MessagingTypes.QUERY:
                        {
                            if (msgPayload is QueryMessage queryMessage)
                                ParseQuery(queryMessage.payload, scene.sceneData.sceneNumber);
                            break;
                        }
                    case MessagingTypes.OPEN_EXTERNAL_URL:
                        {
                            if (msgPayload is Protocol.OpenExternalUrl payload)
                                OnOpenExternalUrlRequest?.Invoke(scene, payload.url);
                            break;
                        }
                    case MessagingTypes.OPEN_NFT_DIALOG:
                        {
                            if (msgPayload is Protocol.OpenNftDialog payload)
                                DataStore.i.common.onOpenNFTPrompt.Set(new NFTPromptModel(payload.contactAddress, payload.tokenId, payload.comment), true);
                            break;
                        }

                    default:
                        Debug.LogError($"Unknown method {method}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError($"Scene message error. scene: {scene.sceneData.sceneNumber} method: {method} payload: {JsonUtility.ToJson(msgPayload)}");
            }

            if (delayedComponent != null)
            {
                if (delayedComponent.isRoutineRunning)
                {
                    yieldInstruction = delayedComponent.yieldInstruction;
                }
            }
        }

        public void ParseQuery(object payload, int sceneNumber)
        {
            if (!Environment.i.world.state.TryGetScene(sceneNumber, out var scene)) return;

            if (!(payload is RaycastQuery raycastQuery))
                return;

            Vector3 worldOrigin = raycastQuery.ray.origin + Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);

            raycastQuery.ray.unityOrigin = PositionUtils.WorldToUnityPosition(worldOrigin);
            raycastQuery.sceneNumber = sceneNumber;
            PhysicsCast.i.Query(raycastQuery, entityIdHelper);
        }

        public void SendSceneMessage(string chunk)
        {
            var renderer = CommonScriptableObjects.rendererState.Get();

            if (!renderer)
            {
                EnqueueChunk(chunk);
            }
            else
            {
                chunksToDecode.Enqueue(chunk);
            }
        }

        private QueuedSceneMessage_Scene Decode(string payload, QueuedSceneMessage_Scene queuedMessage)
        {
            ProfilingEvents.OnMessageDecodeStart?.Invoke("Misc");

            if (!MessageDecoder.DecodePayloadChunk(payload,
                    out int sceneNumber,
                    out string message,
                    out string messageTag,
                    out PB_SendSceneMessage sendSceneMessage))
            {
                return null;
            }

            MessageDecoder.DecodeSceneMessage(sceneNumber, message, messageTag, sendSceneMessage, ref queuedMessage);

            ProfilingEvents.OnMessageDecodeEnds?.Invoke("Misc");

            return queuedMessage;
        }

        private IEnumerator DeferredDecodingAndEnqueue()
        {
            float start = Time.realtimeSinceStartup;
            float maxTimeForDecode;

            while (true)
            {
                maxTimeForDecode = CommonScriptableObjects.rendererState.Get() ? MAX_TIME_FOR_DECODE : float.MaxValue;

                if (chunksToDecode.TryDequeue(out string chunk))
                {
                    EnqueueChunk(chunk);

                    if (Time.realtimeSinceStartup - start < maxTimeForDecode)
                        continue;
                }

                yield return null;

                start = Time.unscaledTime;
            }
        }
        private void EnqueueChunk(string chunk)
        {
            string[] payloads = chunk.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var count = payloads.Length;

            for (int i = 0; i < count; i++)
            {
                bool availableMessage = sceneMessagesPool.TryDequeue(out QueuedSceneMessage_Scene freeMessage);

                if (availableMessage)
                {
                    EnqueueSceneMessage(Decode(payloads[i], freeMessage));
                }
                else
                {
                    EnqueueSceneMessage(Decode(payloads[i], new QueuedSceneMessage_Scene()));
                }
            }
        }
        private async UniTask WatchForNewChunksToDecode(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (chunksToDecode.Count > 0)
                    {
                        ThreadedDecodeAndEnqueue(cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                await UniTask.Yield();
            }
        }

        private void ThreadedDecodeAndEnqueue(CancellationToken cancellationToken)
        {
            while (chunksToDecode.TryDequeue(out string chunk))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string[] payloads = chunk.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var count = payloads.Length;

                for (int i = 0; i < count; i++)
                {
                    var payload = payloads[i];
                    bool availableMessage = sceneMessagesPool.TryDequeue(out QueuedSceneMessage_Scene freeMessage);

                    if (availableMessage)
                    {
                        EnqueueSceneMessage(Decode(payload, freeMessage));
                    }
                    else
                    {
                        EnqueueSceneMessage(Decode(payload, new QueuedSceneMessage_Scene()));
                    }
                }
            }
        }

        public void EnqueueSceneMessage(QueuedSceneMessage_Scene message)
        {
            bool isGlobalScene = WorldStateUtils.IsGlobalScene(message.sceneNumber);
            messagingControllersManager.AddControllerIfNotExists(this, message.sceneNumber);
            messagingControllersManager.Enqueue(isGlobalScene, message);
        }

        //======================================================================

        #endregion

        //======================================================================

        //======================================================================

        #region SCENES_MANAGEMENT

        //======================================================================
        public event Action<int> OnReadyScene;

        public void SendSceneReady(int sceneNumber)
        {
            messagingControllersManager.SetSceneReady(sceneNumber);

            WebInterface.ReportControlEvent(new WebInterface.SceneReady(sceneNumber));
            WebInterface.ReportCameraChanged(CommonScriptableObjects.cameraMode.Get(), sceneNumber);

            Environment.i.world.blockersController.SetupWorldBlockers();

            OnReadyScene?.Invoke(sceneNumber);
        }

        private void SetPositionDirty(Vector2Int gridPosition, Vector2Int previous)
        {
            positionDirty = gridPosition.x != currentGridSceneCoordinate.x || gridPosition.y != currentGridSceneCoordinate.y;

            if (positionDirty)
            {
                sceneSortDirty = true;
                currentGridSceneCoordinate = gridPosition;

                // Since the first position for the character is not sent from Kernel until just-before calling
                // the rendering activation from Kernel, we need to sort the scenes to get the current scene id
                // to lock the rendering accordingly...
                if (!CommonScriptableObjects.rendererState.Get())
                {
                    SortScenesByDistance();
                }
            }
        }

        public void SortScenesByDistance()
        {
            IWorldState worldState = Environment.i.world.state;

            worldState.SortScenesByDistance(currentGridSceneCoordinate);

            int currentSceneNumber = worldState.GetCurrentSceneNumber();

            if (!DataStore.i.debugConfig.isDebugMode.Get() && currentSceneNumber <= 0)
            {
                // When we don't know the current scene yet, we must lock the rendering from enabling until it is set
                CommonScriptableObjects.rendererState.AddLock(this);
            }
            else
            {
                // 1. Set current scene id
                CommonScriptableObjects.sceneNumber.Set(currentSceneNumber);

                // 2. Attempt to remove SceneController's lock on rendering
                CommonScriptableObjects.rendererState.RemoveLock(this);
            }

            OnSortScenes?.Invoke();
        }

        private void OnCurrentSceneNumberChange(int newSceneNumber, int previousSceneNumber)
        {
            if (Environment.i.world.state.TryGetScene(newSceneNumber, out IParcelScene newCurrentScene)
                && !(newCurrentScene as ParcelScene).sceneLifecycleHandler.isReady)
            {
                CommonScriptableObjects.rendererState.AddLock(newCurrentScene);

                (newCurrentScene as ParcelScene).sceneLifecycleHandler.OnSceneReady += (readyScene) => { CommonScriptableObjects.rendererState.RemoveLock(readyScene); };
            }
        }

        public void LoadParcelScenesExecute(string scenePayload)
        {
            LoadParcelScenesMessage.UnityParcelScene sceneToLoad;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_LOAD);
            sceneToLoad = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(scenePayload);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_LOAD);

            LoadUnityParcelScene(sceneToLoad).Forget();
        }

        public async UniTaskVoid LoadUnityParcelScene(LoadParcelScenesMessage.UnityParcelScene sceneToLoad)
        {
            if (sceneToLoad == null || sceneToLoad.sceneNumber <= 0)
                return;

            if (VERBOSE)
                Debug.Log($"{Time.frameCount}: Trying to load scene: id: {sceneToLoad.id}; number: {sceneToLoad.sceneNumber}");

            DebugConfig debugConfig = DataStore.i.debugConfig;
#if UNITY_EDITOR
            if (debugConfig.soloScene && sceneToLoad.basePosition.ToString() != debugConfig.soloSceneCoords.ToString())
            {
                SendSceneReady(sceneToLoad.sceneNumber);

                return;
            }
#endif

            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_LOAD);

            IWorldState worldState = Environment.i.world.state;

            if (!worldState.ContainsScene(sceneToLoad.sceneNumber))
            {
                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                await newScene.SetData(sceneToLoad);

                if (debugConfig.isDebugMode.Get())
                {
                    newScene.InitializeDebugPlane();
                }

                worldState.AddScene(newScene);

                sceneSortDirty = true;

                OnNewSceneAdded?.Invoke(newScene);

                messagingControllersManager.AddControllerIfNotExists(this, newScene.sceneData.sceneNumber);

                if (VERBOSE)
                    Debug.Log($"{Time.frameCount}: Load parcel scene (id: {newScene.sceneData.sceneNumber})");
            }

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_LOAD);
        }

        public void UpdateParcelScenesExecute(string scenePayload)
        {
            LoadParcelScenesMessage.UnityParcelScene sceneData;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            sceneData = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(scenePayload);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_UPDATE);

            IWorldState worldState = Environment.i.world.state;

            if (worldState.TryGetScene(sceneData.sceneNumber, out IParcelScene sceneInterface))
            {
                ParcelScene scene = sceneInterface as ParcelScene;
                scene.SetUpdateData(sceneData);
            }
            else
            {
                LoadParcelScenesExecute(scenePayload);
            }
        }

        public void UpdateParcelScenesExecute(LoadParcelScenesMessage.UnityParcelScene scene)
        {
            if (scene == null || scene.sceneNumber <= 0)
                return;

            var sceneToLoad = scene;

            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_UPDATE);

            ParcelScene parcelScene = Environment.i.world.state.GetScene(sceneToLoad.sceneNumber) as ParcelScene;

            if (parcelScene != null)
                parcelScene.SetUpdateData(sceneToLoad);

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_UPDATE);
        }

        public void UnloadScene(int sceneNumber)
        {
            var queuedMessage = new QueuedSceneMessage()
                { type = QueuedSceneMessage.Type.UNLOAD_PARCEL, sceneNumber = sceneNumber };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            messagingControllersManager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
            messagingControllersManager.RemoveController(sceneNumber);
        }

        public void UnloadParcelSceneExecute(int sceneNumber)
        {
            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_DESTROY);

            IWorldState worldState = Environment.i.world.state;

            if (!worldState.TryGetScene(sceneNumber, out ParcelScene scene))
                return;

            CommonScriptableObjects.rendererState.RemoveLock(scene);
            worldState.RemoveScene(sceneNumber);

            DataStore.i.world.portableExperienceIds.Remove(scene.sceneData.id);

            // Remove messaging controller for unloaded scene
            messagingControllersManager.RemoveController(sceneNumber);

            scene.Cleanup(!CommonScriptableObjects.rendererState.Get());

            if (VERBOSE)
            {
                Debug.Log($"{Time.frameCount} : Destroying scene {scene.sceneData.basePosition}");
            }

            Environment.i.world.blockersController.SetupWorldBlockers();

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_DESTROY);
            OnSceneRemoved?.Invoke(scene);
        }

        public void UnloadAllScenes(bool includePersistent = false)
        {
            var worldState = Environment.i.world.state;

            // since the list was changing by this foreach, we make a copy
            var list = worldState.GetLoadedScenes().ToArray();

            foreach (var kvp in list)
            {
                if (kvp.Value.isPersistent && !includePersistent)
                    continue;

                UnloadParcelSceneExecute(kvp.Key);
            }
        }

        public void LoadParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new QueuedSceneMessage()
            {
                type = QueuedSceneMessage.Type.LOAD_PARCEL,
                message = decentralandSceneJSON
            };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_LOAD);

            messagingControllersManager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

            if (VERBOSE)
                Debug.Log($"{Time.frameCount} : Load parcel scene queue {decentralandSceneJSON}");
        }

        public void UpdateParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new QueuedSceneMessage()
                { type = QueuedSceneMessage.Type.UPDATE_PARCEL, message = decentralandSceneJSON };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_UPDATE);

            messagingControllersManager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new QueuedSceneMessage() { type = QueuedSceneMessage.Type.UNLOAD_SCENES };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public void CreateGlobalScene(CreateGlobalSceneMessage globalScene)
        {
#if UNITY_EDITOR
            DebugConfig debugConfig = DataStore.i.debugConfig;

            if (debugConfig.soloScene && debugConfig.ignoreGlobalScenes)
                return;
#endif

            // NOTE(Brian): We should remove this line. SceneController is a runtime core class.
            //              It should never have references to UI systems or higher level systems.
            if (globalScene.isPortableExperience && !isPexViewerInitialized.Get())
            {
                Debug.LogError(
                    "Portable experiences are trying to be added before the system is initialized!. scene number: " +
                    globalScene.sceneNumber);
                return;
            }

            int newGlobalSceneNumber = globalScene.sceneNumber;
            IWorldState worldState = Environment.i.world.state;

            if (worldState.ContainsScene(newGlobalSceneNumber))
                return;

            var newGameObject = new GameObject("Global Scene - " + newGlobalSceneNumber);

            var newScene = newGameObject.AddComponent<GlobalScene>();
            newScene.unloadWithDistance = false;
            newScene.isPersistent = true;
            newScene.sceneName = globalScene.name;
            newScene.isPortableExperience = globalScene.isPortableExperience;

            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene
            {
                id = globalScene.id,
                sceneNumber = newGlobalSceneNumber,
                basePosition = new Vector2Int(0, 0),
                baseUrl = globalScene.baseUrl,
                contents = globalScene.contents,
                sdk7 = globalScene.sdk7,
                requiredPermissions = globalScene.requiredPermissions,
                allowedMediaHostnames = globalScene.allowedMediaHostnames
            };

            newScene.SetData(sceneData);

            if (!string.IsNullOrEmpty(globalScene.icon))
            {
                newScene.iconUrl = newScene.contentProvider.GetContentsUrl(globalScene.icon);
            }

            worldState.AddScene(newScene);

            OnNewSceneAdded?.Invoke(newScene);

            if (newScene.isPortableExperience)
            {
                DataStore.i.world.portableExperienceIds.Add(sceneData.id);
            }

            messagingControllersManager.AddControllerIfNotExists(this, newGlobalSceneNumber, isGlobal: true);

            if (VERBOSE)
                Debug.Log($"Creating Global scene {newGlobalSceneNumber}");
        }

        public void IsolateScene(IParcelScene sceneToActive)
        {
            foreach (IParcelScene scene in Environment.i.world.state.GetScenesSortedByDistance())
            {
                if (scene != sceneToActive)
                    scene.GetSceneTransform().gameObject.SetActive(false);
            }
        }

        public void ReIntegrateIsolatedScene()
        {
            foreach (IParcelScene scene in Environment.i.world.state.GetScenesSortedByDistance())
            {
                scene.GetSceneTransform().gameObject.SetActive(true);
            }
        }

        //======================================================================

        #endregion

        //======================================================================

        public ConcurrentQueue<QueuedSceneMessage_Scene> sceneMessagesPool { get; } = new ConcurrentQueue<QueuedSceneMessage_Scene>();

        public bool prewarmSceneMessagesPool { get; set; } = true;
        public bool prewarmEntitiesPool { get; set; } = true;

        private bool sceneSortDirty = false;
        private bool positionDirty = true;
        private int lastSortFrame = 0;

        public event Action OnSortScenes;
        public event Action<IParcelScene, string> OnOpenExternalUrlRequest;
        public event Action<IParcelScene> OnNewSceneAdded;
        public event Action<IParcelScene> OnSceneRemoved;

        private Vector2Int currentGridSceneCoordinate = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
    }
}
