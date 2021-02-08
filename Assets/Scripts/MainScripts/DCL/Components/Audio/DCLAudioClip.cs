using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class DCLAudioClip : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string url;
            public bool loop = false;
            public bool shouldTryToLoad = true;

            [Range(0f, 1f)]
            public double volume = 1f;
        }

        public Model model;
        public AudioClip audioClip;
        private bool isDisposed = false;

        public enum LoadState
        {
            IDLE,
            LOADING_IN_PROGRESS,
            LOADING_FAILED,
            LOADING_COMPLETED,
        }

        public LoadState loadingState { get; private set; }
        public event Action<DCLAudioClip> OnLoadingFinished;

        public DCLAudioClip(ParcelScene scene) : base(scene)
        {
            model = new Model();

            loadingState = LoadState.IDLE;
        }

        public override int GetClassId()
        {
            return (int)CLASS_ID.AUDIO_CLIP;
        }

        void OnComplete(AudioClip clip)
        {
            if (clip != null)
            {
                this.audioClip = clip;
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

        void OnFail(string error)
        {
            loadingState = LoadState.LOADING_FAILED;

            if (OnLoadingFinished != null)
            {
                OnLoadingFinished.Invoke(this);
            }
        }

        IEnumerator TryToLoad()
        {
            if (loadingState != LoadState.LOADING_IN_PROGRESS
                && loadingState != LoadState.LOADING_COMPLETED)
            {
                loadingState = LoadState.LOADING_IN_PROGRESS;

                if (scene.contentProvider.HasContentsUrl(model.url))
                {
                    yield return Utils.FetchAudioClip(scene.contentProvider.GetContentsUrl(model.url),
                        Utils.GetAudioTypeFromUrlName(model.url), OnComplete, OnFail);
                }
            }
        }

        void Unload()
        {
            if (audioClip != null && loadingState != LoadState.IDLE)
            {
                audioClip.UnloadAudioData();
                audioClip = null;
                loadingState = LoadState.IDLE;
            }
        }

        public override object GetModel()
        {
            return model;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if(isDisposed)
                yield break;

            model = Utils.SafeFromJson<Model>(newJson);

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
            Utils.SafeDestroy(audioClip);
            base.Dispose();
        }
    }
}