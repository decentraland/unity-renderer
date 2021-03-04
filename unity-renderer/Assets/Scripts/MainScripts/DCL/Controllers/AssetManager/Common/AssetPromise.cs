using System;
using UnityEngine;

namespace DCL
{
    public enum AssetPromiseState
    {
        IDLE_AND_EMPTY,
        WAITING,
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
        internal bool isDirty = false;
        internal AssetLibrary<AssetType> library;
        public AssetType asset { get; protected set; }

        public AssetPromiseState state { get; protected set; }

        internal event Action<AssetPromise<AssetType>> OnPreFinishEvent;
        public event Action<AssetType> OnSuccessEvent;
        public event Action<AssetType> OnFailEvent;

        public override bool keepWaiting
        {
            get { return state == AssetPromiseState.LOADING || state == AssetPromiseState.WAITING; }
        }

        public void ClearEvents()
        {
            OnSuccessEvent = null;
            OnFailEvent = null;
        }

        internal void ForceFail()
        {
            OnPreFinishEvent = null;
            CallAndClearEvents(false);
            state = AssetPromiseState.IDLE_AND_EMPTY;
        }

        internal void SetWaitingState()
        {
            //TODO(Brian): This is made to make the promises yielding not return automatically when the promise is blocked.
            //
            //             Managing the blocked promises handling entirely in the AssetPromiseKeeper is coupling the code too much.
            //             It's better to have a "WaitForPromise" method here and lighten the APK logic a bit. For now this makes the trick.
            state = AssetPromiseState.WAITING;
        }

        protected void CallAndClearEvents(bool isSuccess = true)
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
            if (state == AssetPromiseState.LOADING || state == AssetPromiseState.FINISHED)
                return;

            state = AssetPromiseState.LOADING;

            // NOTE(Brian): Get existent library element
            object libraryAssetCheckId = GetLibraryAssetCheckId();
            if (library.Contains(libraryAssetCheckId))
            {
                asset = GetAsset(libraryAssetCheckId);

                if (asset != null)
                {
                    OnBeforeLoadOrReuse();
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
            OnBeforeLoadOrReuse();
            asset.id = GetId();

            OnLoad(OnLoadSuccess, OnLoadFailure);
        }

        protected virtual object GetLibraryAssetCheckId()
        {
            return GetId();
        }

        protected virtual AssetType GetAsset(object id)
        {
            return library.Get(id);
        }

        protected virtual void OnReuse(Action OnFinish)
        {
            OnFinish?.Invoke();
        }

        protected void OnReuseFinished()
        {
            OnAfterLoadOrReuse();
            state = AssetPromiseState.FINISHED;
            CallAndClearEvents(isSuccess: true);
        }

        protected void OnLoadSuccess()
        {
            if (AddToLibrary())
            {
                OnAfterLoadOrReuse();
                state = AssetPromiseState.FINISHED;
                CallAndClearEvents(isSuccess: true);
            }
            else
            {
                OnLoadFailure();
            }
        }

        protected void OnLoadFailure()
        {
            CallAndClearEvents(isSuccess: false);
            Cleanup();
        }

        protected virtual bool AddToLibrary()
        {
            return library.Add(asset);
        }

        internal void Unload()
        {
            if (state == AssetPromiseState.IDLE_AND_EMPTY)
                return;

            Cleanup();
        }

        public void Cleanup()
        {
            if (state == AssetPromiseState.LOADING)
            {
                OnCancelLoading();
                ClearEvents();
            }

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
        protected abstract void OnBeforeLoadOrReuse();
        protected abstract void OnAfterLoadOrReuse();
        public abstract object GetId();
    }
}