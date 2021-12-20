using System;
using System.Collections;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class DCLAudioClip : BaseDisposable
    {
        [Serializable]
        public class Model : BaseModel
        {
            public string url;
            public bool loop = false;
            public bool shouldTryToLoad = true;

            [Range(0f, 1f)]
            public double volume = 1f;

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
        }

        public AudioClip audioClip;
        private bool isDisposed = false;
        private AssetPromise_AudioClip audioClipPromise = null;

        public enum LoadState
        {
            IDLE,
            LOADING_IN_PROGRESS,
            LOADING_FAILED,
            LOADING_COMPLETED,
        }

        public LoadState loadingState { get; private set; }
        public event Action<DCLAudioClip> OnLoadingFinished;

        public DCLAudioClip()
        {
            model = new Model();

            loadingState = LoadState.IDLE;
        }

        public double volume => ((Model) model).volume;

        public bool isLoop => ((Model) model).loop;

        public bool shouldTryLoad => ((Model) model).shouldTryToLoad;

        public override int GetClassId() { return (int) CLASS_ID.AUDIO_CLIP; }

        void OnComplete(Asset_AudioClip assetAudioClip)
        {
            if (assetAudioClip.audioClip != null)
            {
                this.audioClip = assetAudioClip.audioClip;
                loadingState = LoadState.LOADING_COMPLETED;
            }
            else
            {
                loadingState = LoadState.LOADING_FAILED;
            }

            if (OnLoadingFinished != null)
            {
                OnLoadingFinished.Invoke(this);
            }
        }

        void OnFail(Asset_AudioClip assetAudioClip, Exception exception)
        {
            loadingState = LoadState.LOADING_FAILED;
            OnLoadingFinished?.Invoke(this);
        }

        IEnumerator TryToLoad()
        {
            if (loadingState != LoadState.LOADING_IN_PROGRESS
                && loadingState != LoadState.LOADING_COMPLETED)
            {
                loadingState = LoadState.LOADING_IN_PROGRESS;
                Model model = (Model) this.model;

                audioClipPromise = new AssetPromise_AudioClip(model.url, scene.contentProvider);
                audioClipPromise.OnSuccessEvent += OnComplete;
                audioClipPromise.OnFailEvent += OnFail;

                AssetPromiseKeeper_AudioClip.i.Keep(audioClipPromise);

                yield return audioClipPromise;
            }
        }

        void Unload()
        {
            loadingState = LoadState.IDLE;
            AssetPromiseKeeper_AudioClip.i.Forget(audioClipPromise);
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDisposed)
                yield break;

            Model model = (Model) newModel;

            if (!string.IsNullOrEmpty(model.url))
            {
                if (model.shouldTryToLoad && audioClip == null)
                {
                    yield return TryToLoad();
                }
                else if (!model.shouldTryToLoad && audioClip != null)
                {
                    Unload();
                }
            }

            yield return null;
        }

        public override void Dispose()
        {
            isDisposed = true;
            AssetPromiseKeeper_AudioClip.i.Forget(audioClipPromise);
            base.Dispose();
        }
    }
}