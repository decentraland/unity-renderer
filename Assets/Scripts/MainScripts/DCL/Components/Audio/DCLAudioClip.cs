using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    public class DCLAudioClip : BaseDisposable
    {
        public override string componentName => "AudioClip";

        [System.Serializable]
        public class Model
        {
            public string url;
            public bool distanceModel;
            public bool loop;
            public bool loading;
            public string loadingCompleteEventId;
            public double volume;
        }

        Model model;
        public AudioClip audioClip;

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
                OnLoadingFinished.Invoke(this);
        }

        void OnFail(string error)
        {
            loadingState = LoadState.LOADING_FAILED;

            if (OnLoadingFinished != null)
                OnLoadingFinished.Invoke(this);
        }

        IEnumerator TryToLoad()
        {
            if (loadingState != LoadState.LOADING_IN_PROGRESS
                && loadingState != LoadState.LOADING_COMPLETED)
            {
                loadingState = LoadState.LOADING_IN_PROGRESS;
                yield return Utils.FetchAudioClip(model.url, Utils.GetAudioTypeFromUrlName(model.url), OnComplete, OnFail);
                
            }
        }

        void Unload()
        {
            if (audioClip != null && loadingState != LoadState.IDLE)
            {
                audioClip.UnloadAudioData();
                Resources.UnloadUnusedAssets();
                audioClip = null;
                loadingState = LoadState.IDLE;
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);

            if (model.loading && audioClip == null)
            {
                yield return TryToLoad();
            }
            else if (!model.loading && audioClip != null)
            {
                Unload();
            }

            yield return null;
        }
    }
}
