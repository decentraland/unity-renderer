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

        public int pendingResourcesCount => SceneResourcesLoadTracker.pendingResourcesCount;
        public float loadingProgress => SceneResourcesLoadTracker.loadingProgress;
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

        public SceneResourcesLoadTracker SceneResourcesLoadTracker { get; }

        public event Action<ParcelScene> OnSceneReady;
        public event Action<ParcelScene> OnStateRefreshed;

        private ParcelScene owner;

        public SceneLifecycleHandler(ParcelScene ownerScene)
        {
            state = State.NOT_READY;
            this.owner = ownerScene;
            owner.OnSetData += OnSceneSetData;

            SceneResourcesLoadTracker = new SceneResourcesLoadTracker();
            SceneResourcesLoadTracker.Track(owner.componentsManagerLegacy, Environment.i.world.state);
            SceneResourcesLoadTracker.OnResourcesStatusUpdate += OnResourcesStatusUpdated;
   
            // This is done while the two ECS are living together, if we detect that a component from the new ECS has incremented a 
            // resource for the scene, we changed the track since that means that this scene is from the new ECS.
            // This should disappear when the old ecs is removed from the project. This should be the default track 
            DataStore.i.ecs7.pendingSceneResources.OnRefCountUpdated += ( sceneId,  amount) =>
            {
                if (sceneId.sceneId != ownerScene.sceneData.id)
                    return;
                
                SceneResourcesLoadTracker.Dispose();
                SceneResourcesLoadTracker.Track(sceneId.sceneId);
                SceneResourcesLoadTracker.OnResourcesStatusUpdate += OnResourcesStatusUpdated;
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
                Debug.Log($"{owner.sceneData.basePosition} Disposable objects left... {SceneResourcesLoadTracker.pendingResourcesCount}");
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

            if (SceneResourcesLoadTracker.ShouldWaitForPendingResources())
            {
                SceneResourcesLoadTracker.OnResourcesLoaded -= SetSceneReady;
                SceneResourcesLoadTracker.OnResourcesLoaded += SetSceneReady;
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

            SceneResourcesLoadTracker.OnResourcesLoaded -= SetSceneReady;
            SceneResourcesLoadTracker.OnResourcesStatusUpdate -= OnResourcesStatusUpdated;
            SceneResourcesLoadTracker.Dispose();

            OnSceneReady?.Invoke(owner);
        }
    }
}