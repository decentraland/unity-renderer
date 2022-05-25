using System;
using DCL.Models;
using DCL.WorldRuntime;
using UnityEngine;

namespace DCL.Controllers
{
    /// <summary>
    /// This handler is in charge of handling the scene lifecycle events
    /// and upkeep the scene lifecycle state.
    /// </summary>
    public class SceneLifecycleHandler
    {
        public static bool VERBOSE = false;

        public enum State
        {
            NOT_READY,
            WAITING_FOR_INIT_MESSAGES,
            WAITING_FOR_COMPONENTS,
            READY,
        }

        public int pendingResourcesCount => sceneLoadTracker.pendingResourcesCount;
        public float loadingProgress => sceneLoadTracker.loadingProgress;
        public bool isReady => state == State.READY;

        State stateValue = State.NOT_READY;

        public State state
        {
            get { return stateValue; }
            set
            {
                stateValue = value;
                OnStateRefreshed?.Invoke(owner);
            }
        }

        public SceneLoadTracker sceneLoadTracker { get; }

        public event Action<ParcelScene> OnSceneReady;
        public event Action<ParcelScene> OnStateRefreshed;

        private ParcelScene owner;

        public SceneLifecycleHandler(ParcelScene ownerScene)
        {
            state = State.NOT_READY;
            this.owner = ownerScene;
            owner.OnSetData += OnSceneSetData;

            sceneLoadTracker = new SceneLoadTracker();
            sceneLoadTracker.Track(owner.componentsManagerLegacy, Environment.i.world.state);
            sceneLoadTracker.OnResourcesStatusUpdate += OnResourcesStatusUpdated;
   
            DataStore.i.ecs7.pendingSceneResources.OnRefCountUpdated += ( sceneId,  amount) =>
            {
                if (sceneId.sceneId != ownerScene.sceneData.id)
                    return;
                
                sceneLoadTracker.Dispose();
                sceneLoadTracker.Track(sceneId.sceneId);
                sceneLoadTracker.OnResourcesStatusUpdate += OnResourcesStatusUpdated;
            };
        }

        private void OnSceneSetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            state = State.WAITING_FOR_INIT_MESSAGES;
            owner.RefreshLoadingState();

#if UNITY_EDITOR
            DebugConfig debugConfig = DataStore.i.debugConfig;
            //NOTE(Brian): Don't generate parcel blockers if debugScenes is active and is not the desired scene.
            if (debugConfig.soloScene && debugConfig.soloSceneCoords != data.basePosition)
            {
                SetSceneReady();
                return;
            }
#endif

            if (owner.isTestScene)
                SetSceneReady();
        }

        private void OnResourcesStatusUpdated()
        {
            if (owner.isReleased)
                return;

            if (VERBOSE)
            {
                Debug.Log($"{owner.sceneData.basePosition} Disposable objects left... {sceneLoadTracker.pendingResourcesCount}");
            }

            OnStateRefreshed?.Invoke(owner);
            owner.RefreshLoadingState();
        }

        public void SetInitMessagesDone()
        {
            if (owner.isReleased)
                return;

            if (state == State.READY)
            {
                Debug.LogWarning($"Init messages done after ready?! {owner.sceneData.basePosition}", owner.gameObject);
                return;
            }

            state = State.WAITING_FOR_COMPONENTS;
            owner.RefreshLoadingState();

            if (sceneLoadTracker.ShouldWaitForPendingResources())
            {
                sceneLoadTracker.OnResourcesLoaded -= SetSceneReady;
                sceneLoadTracker.OnResourcesLoaded += SetSceneReady;
            }
            else
            {
                SetSceneReady();
            }
        }

        private void SetSceneReady()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif

            if (state == State.READY)
                return;

            if (VERBOSE)
                Debug.Log($"{owner.sceneData.basePosition} Scene Ready!");

            state = State.READY;

            Environment.i.world.sceneController.SendSceneReady(owner.sceneData.id);
            owner.RefreshLoadingState();

            sceneLoadTracker.OnResourcesLoaded -= SetSceneReady;
            sceneLoadTracker.OnResourcesStatusUpdate -= OnResourcesStatusUpdated;
            sceneLoadTracker.Dispose();

            OnSceneReady?.Invoke(owner);
        }
    }
}