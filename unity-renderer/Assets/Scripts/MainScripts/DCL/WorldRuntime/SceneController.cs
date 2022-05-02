using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCL.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
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
        private LoadingFeedbackController loadingFeedbackController;
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

            loadingFeedbackController = new LoadingFeedbackController();

            DataStore.i.debugConfig.isDebugMode.OnChange += OnDebugModeSet;

            SetupDeferredRunners();

            CommonScriptableObjects.playerWorldPosition.OnChange += SetPositionDirty;
            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneIdChange;

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
            loadingFeedbackController.Dispose();

            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

            PoolManager.i.OnGet -= Environment.i.platform.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet -= Environment.i.platform.cullingController.objectsTracker.MarkDirty;

            CommonScriptableObjects.playerWorldPosition.OnChange -= SetPositionDirty;
            DataStore.i.debugConfig.isDebugMode.OnChange -= OnDebugModeSet;

            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneIdChange;

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
        public delegate void ProcessDelegate(string sceneId, string method);

        public event ProcessDelegate OnMessageProcessInfoStart;
        public event ProcessDelegate OnMessageProcessInfoEnds;
#endif
        public bool deferredMessagesDecoding { get; set; } = false;

        readonly ConcurrentQueue<string> chunksToDecode = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<QueuedSceneMessage_Scene> messagesToProcess = new ConcurrentQueue<QueuedSceneMessage_Scene>();

        const float MAX_TIME_FOR_DECODE = 0.005f;

        public bool ProcessMessage(QueuedSceneMessage_Scene msgObject, out CustomYieldInstruction yieldInstruction)
        {
            string sceneId = msgObject.sceneId;
            string method = msgObject.method;

            yieldInstruction = null;

            IParcelScene scene;
            bool res = false;
            IWorldState worldState = Environment.i.world.state;
            DebugConfig debugConfig = DataStore.i.debugConfig;

            if (worldState.loadedScenes.TryGetValue(sceneId, out scene))
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
                OnMessageProcessInfoStart?.Invoke(sceneId, method);
#endif
                ProfilingEvents.OnMessageProcessStart?.Invoke(method);

                ProcessMessage(scene as ParcelScene, method, msgObject.payload, out yieldInstruction);

                ProfilingEvents.OnMessageProcessEnds?.Invoke(method);

#if UNITY_EDITOR
                OnMessageProcessInfoEnds?.Invoke(sceneId, method);
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

        private void ProcessMessage(ParcelScene scene, string method, object msgPayload,
            out CustomYieldInstruction yieldInstruction)
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
                                scene.SetEntityParent(entityIdHelper.EntityFromLegacyEntityString(payload.entityId),
                                    entityIdHelper.EntityFromLegacyEntityString(payload.parentId));

                            break;
                        }

                    case MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE:
                        {
                            if (msgPayload is Protocol.EntityComponentCreateOrUpdate payload)
                            {
                                delayedComponent = scene.componentsManagerLegacy.EntityComponentCreateOrUpdate(
                                    entityIdHelper.EntityFromLegacyEntityString(payload.entityId),
                                    (CLASS_ID_COMPONENT) payload.classId, payload.json) as IDelayedComponent;
                            }

                            break;
                        }

                    case MessagingTypes.ENTITY_COMPONENT_DESTROY:
                        {
                            if (msgPayload is Protocol.EntityComponentDestroy payload)
                                scene.componentsManagerLegacy.EntityComponentRemove(
                                    entityIdHelper.EntityFromLegacyEntityString(payload.entityId), payload.name);

                            break;
                        }

                    case MessagingTypes.SHARED_COMPONENT_ATTACH:
                        {
                            if (msgPayload is Protocol.SharedComponentAttach payload)
                                scene.componentsManagerLegacy.SceneSharedComponentAttach(
                                    entityIdHelper.EntityFromLegacyEntityString(payload.entityId), payload.id);

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

                    case MessagingTypes.SHARED_COMPONENT_UPDATE:
                        {
                            if (msgPayload is Protocol.SharedComponentUpdate payload)
                                delayedComponent = scene.componentsManagerLegacy.SceneSharedComponentUpdate(payload.componentId, payload.json) as IDelayedComponent;

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
                            scene.sceneLifecycleHandler.SetInitMessagesDone();

                            break;
                        }

                    case MessagingTypes.QUERY:
                        {
                            if (msgPayload is QueryMessage queryMessage)
                                ParseQuery(queryMessage.payload, scene.sceneData.id);

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
                                DataStore.i.common.onOpenNFTPrompt.Set(new NFTPromptModel(payload.contactAddress, payload.tokenId,
                                    payload.comment), true);

                            break;
                        }

                    default:
                        Debug.LogError($"Unknown method {method}");

                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Scene message error. scene: {scene.sceneData.id} method: {method} payload: {JsonUtility.ToJson(msgPayload)} {e}");
            }

            if (delayedComponent != null)
            {
                if (delayedComponent.isRoutineRunning)
                    yieldInstruction = delayedComponent.yieldInstruction;
            }
        }

        public void ParseQuery(object payload, string sceneId)
        {
            IParcelScene scene = Environment.i.world.state.loadedScenes[sceneId];

            if (!(payload is RaycastQuery raycastQuery))
                return;

            Vector3 worldOrigin = raycastQuery.ray.origin + Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);

            raycastQuery.ray.unityOrigin = PositionUtils.WorldToUnityPosition(worldOrigin);
            raycastQuery.sceneId = sceneId;
            PhysicsCast.i.Query(raycastQuery);
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
                    out string sceneId,
                    out string message,
                    out string messageTag,
                    out PB_SendSceneMessage sendSceneMessage))
            {
                return null;
            }

            MessageDecoder.DecodeSceneMessage(sceneId, message, messageTag, sendSceneMessage, ref queuedMessage);

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

                if (chunksToDecode.Count > 0)
                {
                    if (chunksToDecode.TryDequeue(out string chunk))
                    {
                        EnqueueChunk(chunk);

                        if (Time.realtimeSinceStartup - start < maxTimeForDecode)
                            continue;
                    }
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
            bool isGlobalScene = WorldStateUtils.IsGlobalScene(message.sceneId);
            messagingControllersManager.AddControllerIfNotExists(this, message.sceneId);
            messagingControllersManager.Enqueue(isGlobalScene, message);
        }

        //======================================================================

        #endregion

        //======================================================================

        //======================================================================

        #region SCENES_MANAGEMENT

        //======================================================================
        public event Action<string> OnReadyScene;
        
        public void SendSceneReady(string sceneId)
        {
            Environment.i.world.state.readyScenes.Add(sceneId);

            messagingControllersManager.SetSceneReady(sceneId);

            WebInterface.ReportControlEvent(new WebInterface.SceneReady(sceneId));
            WebInterface.ReportCameraChanged(CommonScriptableObjects.cameraMode.Get(), sceneId);

            Environment.i.world.blockersController.SetupWorldBlockers();

            OnReadyScene?.Invoke(sceneId);
        }

        public void ActivateBuilderInWorldEditScene() { Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_BIW()); }

        public void DeactivateBuilderInWorldEditScene() { Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple()); }

        private void SetPositionDirty(Vector3 worldPosition, Vector3 previous)
        {
            var currentX = (int) Math.Floor(worldPosition.x / ParcelSettings.PARCEL_SIZE);
            var currentY = (int) Math.Floor(worldPosition.z / ParcelSettings.PARCEL_SIZE);

            positionDirty = currentX != currentGridSceneCoordinate.x || currentY != currentGridSceneCoordinate.y;

            if (positionDirty)
            {
                sceneSortDirty = true;
                currentGridSceneCoordinate.x = currentX;
                currentGridSceneCoordinate.y = currentY;

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
            // if (DCLCharacterController.i == null)
            //     return;

            IWorldState worldState = Environment.i.world.state;

            worldState.currentSceneId = null;
            worldState.scenesSortedByDistance.Sort(SortScenesByDistanceMethod);

            using (var iterator = Environment.i.world.state.scenesSortedByDistance.GetEnumerator())
            {
                IParcelScene scene;
                bool characterIsInsideScene;

                while (iterator.MoveNext())
                {
                    scene = iterator.Current;

                    if (scene == null)
                        continue;

                    characterIsInsideScene = WorldStateUtils.IsCharacterInsideScene(scene);
                    bool isGlobalScene = worldState.globalSceneIds.Contains(scene.sceneData.id);

                    if (!isGlobalScene && characterIsInsideScene)
                    {
                        worldState.currentSceneId = scene.sceneData.id;

                        break;
                    }
                }
            }

            if (!DataStore.i.debugConfig.isDebugMode.Get() && string.IsNullOrEmpty(worldState.currentSceneId))
            {
                // When we don't know the current scene yet, we must lock the rendering from enabling until it is set
                CommonScriptableObjects.rendererState.AddLock(this);
            }
            else
            {
                // 1. Set current scene id
                CommonScriptableObjects.sceneID.Set(worldState.currentSceneId);

                // 2. Attempt to remove SceneController's lock on rendering
                CommonScriptableObjects.rendererState.RemoveLock(this);
            }

            OnSortScenes?.Invoke();
        }

        private int SortScenesByDistanceMethod(IParcelScene sceneA, IParcelScene sceneB)
        {
            sortAuxiliaryVector = sceneA.sceneData.basePosition - currentGridSceneCoordinate;
            int dist1 = sortAuxiliaryVector.sqrMagnitude;

            sortAuxiliaryVector = sceneB.sceneData.basePosition - currentGridSceneCoordinate;
            int dist2 = sortAuxiliaryVector.sqrMagnitude;

            return dist1 - dist2;
        }

        private void OnCurrentSceneIdChange(string newSceneId, string prevSceneId)
        {
            if (Environment.i.world.state.TryGetScene(newSceneId, out IParcelScene newCurrentScene)
                && !(newCurrentScene as ParcelScene).sceneLifecycleHandler.isReady)
            {
                CommonScriptableObjects.rendererState.AddLock(newCurrentScene);

                (newCurrentScene as ParcelScene).sceneLifecycleHandler.OnSceneReady += (readyScene) => { CommonScriptableObjects.rendererState.RemoveLock(readyScene); };
            }
        }

        public void LoadParcelScenesExecute(string scenePayload)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_LOAD);
            scene = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(scenePayload);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_LOAD);

            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;

            DebugConfig debugConfig = DataStore.i.debugConfig;
#if UNITY_EDITOR
            if (debugConfig.soloScene && sceneToLoad.basePosition.ToString() != debugConfig.soloSceneCoords.ToString())
            {
                SendSceneReady(sceneToLoad.id);

                return;
            }
#endif

            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_LOAD);

            IWorldState worldState = Environment.i.world.state;

            if (!worldState.loadedScenes.ContainsKey(sceneToLoad.id))
            {
                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                newScene.SetData(sceneToLoad);

                if (debugConfig.isDebugMode.Get())
                {
                    newScene.InitializeDebugPlane();
                }

                worldState.loadedScenes.Add(sceneToLoad.id, newScene);
                worldState.scenesSortedByDistance.Add(newScene);

                sceneSortDirty = true;

                OnNewSceneAdded?.Invoke(newScene);

                messagingControllersManager.AddControllerIfNotExists(this, newScene.sceneData.id);

                if (VERBOSE)
                    Debug.Log($"{Time.frameCount}: Load parcel scene (id: {newScene.sceneData.id})");
            }

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_LOAD);
        }

        public void UpdateParcelScenesExecute(string sceneId)
        {
            LoadParcelScenesMessage.UnityParcelScene sceneData;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            sceneData = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(sceneId);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_UPDATE);

            IWorldState worldState = Environment.i.world.state;

            if (worldState.TryGetScene(sceneData.id, out IParcelScene sceneInterface))
            {
                ParcelScene scene = sceneInterface as ParcelScene;
                scene.SetUpdateData(sceneData);
            }
            else
            {
                LoadParcelScenesExecute(sceneId);
            }
        }

        public void UpdateParcelScenesExecute(LoadParcelScenesMessage.UnityParcelScene scene)
        {
            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;

            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_UPDATE);

            ParcelScene parcelScene = Environment.i.world.state.GetScene(sceneToLoad.id) as ParcelScene;

            if (parcelScene != null)
                parcelScene.SetUpdateData(sceneToLoad);

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_UPDATE);
        }

        public void UnloadScene(string sceneKey)
        {
            var queuedMessage = new QueuedSceneMessage()
                { type = QueuedSceneMessage.Type.UNLOAD_PARCEL, message = sceneKey };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            messagingControllersManager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
            messagingControllersManager.RemoveController(sceneKey);
        }

        public void UnloadParcelSceneExecute(string sceneId)
        {
            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_DESTROY);

            IWorldState worldState = Environment.i.world.state;

            if (!worldState.Contains(sceneId))
                return;

            ParcelScene scene = (ParcelScene) worldState.loadedScenes[sceneId];
            
            worldState.loadedScenes.Remove(sceneId);
            worldState.globalSceneIds.Remove(sceneId);
            DataStore.i.world.portableExperienceIds.Remove(sceneId);
            
            // Remove the scene id from the msg. priorities list
            worldState.scenesSortedByDistance.Remove(scene);

            // Remove messaging controller for unloaded scene
            messagingControllersManager.RemoveController(scene.sceneData.id);

            scene.Cleanup(!CommonScriptableObjects.rendererState.Get());

            if (VERBOSE)
            {
                Debug.Log($"{Time.frameCount} : Destroying scene {scene.sceneData.basePosition}");
            }

            Environment.i.world.blockersController.SetupWorldBlockers();

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_DESTROY);
        }

        public void UnloadAllScenes(bool includePersistent = false)
        {
            var worldState = Environment.i.world.state;

            var list = worldState.loadedScenes.ToArray();

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Value.isPersistent && !includePersistent)
                    continue;
                
                UnloadParcelSceneExecute(list[i].Key);
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

        public void CreateGlobalScene(string json)
        {
#if UNITY_EDITOR
            DebugConfig debugConfig = DataStore.i.debugConfig;

            if (debugConfig.soloScene && debugConfig.ignoreGlobalScenes)
                return;
#endif
            CreateGlobalSceneMessage globalScene = Utils.SafeFromJson<CreateGlobalSceneMessage>(json);

            // NOTE(Brian): We should remove this line. SceneController is a runtime core class.
            //              It should never have references to UI systems or higher level systems.
            if (globalScene.isPortableExperience && !isPexViewerInitialized.Get())
            {
                Debug.LogError(
                    "Portable experiences are trying to be added before the system is initialized!. SceneID: " +
                    globalScene.id);
                return;
            }

            string newGlobalSceneId = globalScene.id;

            IWorldState worldState = Environment.i.world.state;

            if (worldState.loadedScenes.ContainsKey(newGlobalSceneId))
                return;

            var newGameObject = new GameObject("Global Scene - " + newGlobalSceneId);

            var newScene = newGameObject.AddComponent<GlobalScene>();
            newScene.unloadWithDistance = false;
            newScene.isPersistent = true;
            newScene.sceneName = globalScene.name;
            newScene.isPortableExperience = globalScene.isPortableExperience;

            LoadParcelScenesMessage.UnityParcelScene data = new LoadParcelScenesMessage.UnityParcelScene
            {
                id = newGlobalSceneId,
                basePosition = new Vector2Int(0, 0),
                baseUrl = globalScene.baseUrl,
                contents = globalScene.contents
            };

            newScene.SetData(data);

            if (!string.IsNullOrEmpty(globalScene.icon))
            {
                newScene.iconUrl = newScene.contentProvider.GetContentsUrl(globalScene.icon);
            }

            worldState.loadedScenes.Add(newGlobalSceneId, newScene);
            OnNewSceneAdded?.Invoke(newScene);

            if (newScene.isPortableExperience)
            {
                DataStore.i.world.portableExperienceIds.Add(newGlobalSceneId);
            }

            worldState.globalSceneIds.Add(newGlobalSceneId);

            messagingControllersManager.AddControllerIfNotExists(this, newGlobalSceneId, isGlobal: true);

            if (VERBOSE)
                Debug.Log($"Creating Global scene {newGlobalSceneId}");
        }

        public void IsolateScene(IParcelScene sceneToActive)
        {
            foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
            {
                if (scene != sceneToActive)
                    scene.GetSceneTransform().gameObject.SetActive(false);
            }
        }

        public void ReIntegrateIsolatedScene()
        {
            foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
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
        
        private Vector2Int currentGridSceneCoordinate = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
        private Vector2Int sortAuxiliaryVector = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
    }
}