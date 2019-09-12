using System;
using UnityEngine;

namespace DCL
{
    public enum AssetPromiseState
    {
        IDLE_AND_EMPTY,
        LOADING,
        FINISHED
    }

    /// <summary>
    /// AssetPromise is in charge of handling all the logic related to loading the specified AssetType. 
    /// It also should return the cached asset if applicable.
    /// 
    /// If we need settings related to how the asset should be loaded/handled, we add them here.
    /// 
    /// If we need many ways of loading the same Asset type, we can create different AssetPromise
    /// subclasses to do so. This can be useful to attach the GLTFSceneImporter loading logic for
    /// materials/textures to system.
    /// </summary>
    /// <typeparam name="AssetType">The Asset type to be loaded.</typeparam>
    public abstract class AssetPromise<AssetType> : CustomYieldInstruction
        where AssetType : Asset, new()
    {
        internal AssetLibrary<AssetType> library;
        public AssetType asset { get; protected set; }

        public AssetPromiseState state { get; protected set; }

        internal event Action<AssetPromise<AssetType>> OnPreFinishEvent;
        public event Action<AssetType> OnSuccessEvent;
        public event Action<AssetType> OnFailEvent;

        public override bool keepWaiting { get { return state == AssetPromiseState.LOADING; } }

        public void ClearEvents()
        {
            OnSuccessEvent = null;
            OnFailEvent = null;
        }

        internal void ForceFail()
        {
            OnPreFinishEvent = null;
            CallAndClearEvents(false);
        }

        void CallAndClearEvents(bool isSuccess = true)
        {
            if (asset == null)
            {
                isSuccess = false;
            }

            OnPreFinishEvent?.Invoke(this);
            OnPreFinishEvent = null;

            Action<AssetType> finalEvent = isSuccess ? OnSuccessEvent : OnFailEvent;

            finalEvent?.Invoke(asset);

            ClearEvents();
        }

        internal virtual void Load()
        {
            if (state != AssetPromiseState.IDLE_AND_EMPTY)
                return;

            object id = GetId();
            state = AssetPromiseState.LOADING;

            // NOTE(Brian): Get existent library element
            if (library.Contains(id))
            {
                asset = library.Get(id);

                if (asset != null)
                {
                    ApplySettings_LoadStart();
                    OnReuse(OnReuseFinished);
                }
                else
                {
                    CallAndClearEvents(false);
                }

                return;
            }

            // NOTE(Brian): Get new library element
            asset = new AssetType();
            ApplySettings_LoadStart();
            asset.id = id;

            OnLoad(OnLoadSuccess, OnLoadFailure);
        }

        protected virtual void OnReuse(Action OnFinish)
        {
            OnFinish?.Invoke();
        }

        protected void OnReuseFinished()
        {
            ApplySettings_LoadFinished();
            state = AssetPromiseState.FINISHED;
            CallAndClearEvents();
        }


        private void OnLoadSuccess()
        {
            AddToLibrary();
            ApplySettings_LoadFinished();
            state = AssetPromiseState.FINISHED;
            CallAndClearEvents(isSuccess: true);
        }

        private void OnLoadFailure()
        {
            CallAndClearEvents(isSuccess: false);
            Cleanup();
        }

        protected virtual void AddToLibrary()
        {
            library.Add(asset);
        }

        internal void Unload()
        {
            if (state == AssetPromiseState.IDLE_AND_EMPTY)
                return;

            if (state == AssetPromiseState.LOADING)
            {
                OnCancelLoading();
                OnLoadFailure();
            }
            else if (state == AssetPromiseState.FINISHED)
            {
                Cleanup();
            }
        }

        protected void Cleanup()
        {
            state = AssetPromiseState.IDLE_AND_EMPTY;

            if (asset != null)
            {
                if (library.Contains(asset))
                    library.Release(asset);
                else
                    asset.Cleanup();

                asset = null;
            }
        }

        protected abstract void OnCancelLoading();
        protected abstract void OnLoad(Action OnSuccess, Action OnFail);
        protected abstract void ApplySettings_LoadStart();
        protected abstract void ApplySettings_LoadFinished();
        internal abstract object GetId();
    }
}
