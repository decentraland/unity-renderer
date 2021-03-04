using DCL.Helpers;
using System.Collections;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Components
{
    public class DCLAudioSource : BaseComponent, IOutOfSceneBoundariesHandler
    {
        [System.Serializable]
        public class Model
        {
            public string audioClipId;
            public bool playing;
            public float volume = 1f;
            public bool loop = false;
            public float pitch = 1f;
        }

        public float playTime => audioSource.time;
        public Model model;
        internal AudioSource audioSource;
        DCLAudioClip lastDCLAudioClip;

        private bool isDestroyed = false;

        private void Awake()
        {
            audioSource = gameObject.GetOrCreateComponent<AudioSource>();
        }

        public void InitDCLAudioClip(DCLAudioClip dclAudioClip)
        {
            if (lastDCLAudioClip != null)
            {
                lastDCLAudioClip.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
            }

            lastDCLAudioClip = dclAudioClip;
        }

        public override object GetModel()
        {
            return model;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy an audiosource before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDestroyed)
                yield break;

            model = Utils.SafeFromJson<Model>(newJson);

            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneChanged;
            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneChanged;

            ApplyCurrentModel();

            yield return null;
        }

        private void ApplyCurrentModel()
        {
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource is null!.");
                return;
            }

            audioSource.volume = (scene.sceneData.id == CommonScriptableObjects.sceneID.Get()) ? model.volume : 0f;
            audioSource.loop = model.loop;
            audioSource.pitch = model.pitch;
            audioSource.spatialBlend = 1;
            audioSource.dopplerLevel = 0.1f;

            if (model.playing)
            {
                DCLAudioClip dclAudioClip = scene.GetSharedComponent(model.audioClipId) as DCLAudioClip;

                if (dclAudioClip != null)
                {
                    InitDCLAudioClip(dclAudioClip);
                    //NOTE(Brian): Play if finished loading, otherwise will wait for the loading to complete (or fail).
                    if (dclAudioClip.loadingState == DCLAudioClip.LoadState.LOADING_COMPLETED)
                    {
                        ApplyLoadedAudioClip(dclAudioClip);
                    }
                    else
                    {
                        dclAudioClip.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
                        dclAudioClip.OnLoadingFinished += DclAudioClip_OnLoadingFinished;
                    }
                }
                else
                {
                    Debug.LogError("Wrong audio clip type when playing audiosource!!");
                }
            }
            else
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }

        private void OnCurrentSceneChanged(string currentSceneId, string previousSceneId)
        {
            if (audioSource != null)
            {
                audioSource.volume = (scene.sceneData.id == currentSceneId) ? model.volume : 0f;
            }
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneChanged;

            //NOTE(Brian): Unsuscribe events.
            InitDCLAudioClip(null);
        }

        public void UpdateOutOfBoundariesState(bool isEnabled)
        {
            bool isDirty = audioSource.enabled != isEnabled;
            audioSource.enabled = isEnabled;
            if (isDirty && isEnabled)
            {
                ApplyCurrentModel();
            }
        }

        private void DclAudioClip_OnLoadingFinished(DCLAudioClip obj)
        {
            if (obj.loadingState == DCLAudioClip.LoadState.LOADING_COMPLETED && audioSource != null)
            {
                ApplyLoadedAudioClip(obj);
            }

            obj.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
        }

        private void ApplyLoadedAudioClip(DCLAudioClip clip)
        {
            if (audioSource.clip != clip.audioClip)
            {
                audioSource.clip = clip.audioClip;
            }

            if (audioSource.enabled && model.playing && !audioSource.isPlaying)
            {
                //To remove a pesky and quite unlikely warning when the audiosource is out of scenebounds
                audioSource.Play();
            }
        }
    }
}