using System.Collections.Generic;
using DCL.Components;
using DCL.Models;
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

        public int disposableNotReadyCount => disposableNotReady.Count;
        public bool isReady => state == State.READY;

        public List<string> disposableNotReady = new List<string>();
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

        public event System.Action<ParcelScene> OnSceneReady;
        public event System.Action<ParcelScene> OnStateRefreshed;

        private ParcelScene owner;

        public SceneLifecycleHandler(ParcelScene ownerScene)
        {
            state = State.NOT_READY;
            this.owner = ownerScene;
            owner.OnSetData += OnSceneSetData;
            owner.OnAddSharedComponent += OnAddSharedComponent;
        }

        private void OnAddSharedComponent(string id, ISharedComponent component)
        {
            if (state != State.READY)
            {
                disposableNotReady.Add(id);
            }
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

        private void OnDisposableReady(ISharedComponent component)
        {
            if (owner.isReleased)
                return;

            disposableNotReady.Remove(component.id);

            if (VERBOSE)
            {
                Debug.Log($"{owner.sceneData.basePosition} Disposable objects left... {disposableNotReady.Count}");
            }

            if (disposableNotReady.Count == 0)
            {
                SetSceneReady();
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

            if (disposableNotReadyCount > 0)
            {
                //NOTE(Brian): Here, we have to split the iterations. If not, we will get repeated calls of
                //             SetSceneReady(), as the disposableNotReady count is 1 and gets to 0
                //             in each OnDisposableReady() call.

                using (var iterator = owner.disposableComponents.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        owner.disposableComponents[iterator.Current.Value.id].CallWhenReady(OnDisposableReady);
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
                Debug.Log($"{owner.sceneData.basePosition} Scene Ready!");

            state = State.READY;

            Environment.i.world.sceneController.SendSceneReady(owner.sceneData.id);
            owner.RefreshLoadingState();

            OnSceneReady?.Invoke(owner);
        }
    }
}