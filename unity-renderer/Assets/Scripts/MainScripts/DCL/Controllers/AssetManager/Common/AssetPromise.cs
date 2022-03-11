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
        public bool isForgotten { get; protected set; }

        internal event Action<AssetPromise<AssetType>> OnPreFinishEvent;
        public event Action<AssetType> OnSuccessEvent;
        public event Action<AssetType, Exception> OnFailEvent;

        public override bool keepWaiting { get { return state == AssetPromiseState.LOADING || state == AssetPromiseState.WAITING; } }

        public void ClearEvents()
        {
            OnSuccessEvent = null;
            OnFailEvent = null;
        }

        internal void ForceFail(Exception reason)
        {
            OnPreFinishEvent = null;
            CallAndClearEvents(false, reason);
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

        protected void CallAndClearEvents(bool isSuccess, Exception exception)
        {
            if (asset == null)
            {
                isSuccess = false;
            }

            OnPreFinishEvent?.Invoke(this);
            OnPreFinishEvent = null;

            if (isSuccess)
                OnSuccessEvent?.Invoke(asset);
            else
                OnFailEvent?.Invoke(asset, exception);

            ClearEvents();
        }

        public bool isDebug = false;
        internal virtual void Load()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.Load() - {GetId()} - 1 - state {state.ToString()}");
            if (state == AssetPromiseState.LOADING || state == AssetPromiseState.FINISHED)
                return;

            state = AssetPromiseState.LOADING;
            
            if(isDebug)
                Debug.Log($"{GetType()}.Load() - {GetId()} - 2");

            // NOTE(Brian): Get existent library element
            object libraryAssetCheckId = GetLibraryAssetCheckId();
            if (library.Contains(libraryAssetCheckId))
            {
                if(isDebug)
                    Debug.Log($"{GetType()}.Load() - {GetId()} - 3");
                asset = GetAsset(libraryAssetCheckId);

                if (asset != null)
                {
                    if(isDebug)
                        Debug.Log($"{GetType()}.Load() - {GetId()} - 4A");
                    OnBeforeLoadOrReuse();
                    OnReuse(OnReuseFinished);
                }
                else
                {
                    if(isDebug)
                        Debug.Log($"{GetType()}.Load() - {GetId()} - 4B");
                    CallAndClearEvents(false, new Exception("Asset is null"));
                }

                return;
            }
            
            if(isDebug)
                Debug.Log($"{GetType()}.Load() - {GetId()} - 5 - Not in library, creating new");

            // NOTE(Brian): Get new library element
            asset = new AssetType();
            OnBeforeLoadOrReuse();
            asset.id = GetId();
            
            if(isDebug)
                Debug.Log($"{GetType()}.Load() - {GetId()} - 6");

            OnLoad(OnLoadSuccess, OnLoadFailure);
        }

        protected virtual object GetLibraryAssetCheckId() { return GetId(); }

        protected virtual AssetType GetAsset(object id) { return library.Get(id); }

        protected virtual void OnReuse(Action OnFinish) { OnFinish?.Invoke(); }

        protected void OnReuseFinished()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.OnReuseFinished() - {GetId()}");
            
            OnAfterLoadOrReuse();
            state = AssetPromiseState.FINISHED;
            CallAndClearEvents(true, null);
        }

        protected void OnLoadSuccess()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.OnLoadSuccess() - {GetId()} - 1");
            
            if (AddToLibrary())
            {
                if(isDebug)
                    Debug.Log($"{GetType()}.OnLoadSuccess() - {GetId()} - 2");
                OnAfterLoadOrReuse();
                state = AssetPromiseState.FINISHED;
                CallAndClearEvents(true, null);
            }
            else
            {
                OnLoadFailure(new Exception("Could not add asset to library"));
            }
        }

        protected void OnLoadFailure(Exception exception)
        {
            if(isDebug)
                Debug.Log($"{GetType()}.OnLoadFailure() - {GetId()} - 1");
            
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif
            
            if(isDebug)
                Debug.Log($"{GetType()}.OnLoadFailure() - {GetId()} - 2");
            
            CallAndClearEvents(false, exception);
            Cleanup();
        }

        protected virtual bool AddToLibrary() { return library.Add(asset); }

        internal virtual void Unload()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.Unload() - {GetId()} - 1");
            
            if (state == AssetPromiseState.IDLE_AND_EMPTY)
                return;
            
            if(isDebug)
                Debug.Log($"{GetType()}.Unload() - {GetId()} - 2");

            Cleanup();
        }

        public void Cleanup()
        {
            if(isDebug)
                Debug.Log($"{GetType()}.Cleanup() - {GetId()} - 1");
            
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
            
            if(isDebug)
                Debug.Log($"{GetType()}.Cleanup() - {GetId()} - 2");
        }

        internal virtual void OnForget()
        {
            isForgotten = true;
            ClearEvents();
        }

        protected abstract void OnCancelLoading();
        protected abstract void OnLoad(Action OnSuccess, Action<Exception> OnFail);
        protected abstract void OnBeforeLoadOrReuse();
        protected abstract void OnAfterLoadOrReuse();
        public abstract object GetId();
    }
}