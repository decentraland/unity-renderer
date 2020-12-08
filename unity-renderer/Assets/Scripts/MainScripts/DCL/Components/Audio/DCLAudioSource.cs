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

            model = Utils.SafeFromJson<Model>(newJson);

            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneChanged;
            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneChanged;

            ApplyCurrentModel();

            yield return null;
        }

        private void ApplyCurrentModel()
        {
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
                        audioSource.clip = dclAudioClip.audioClip;

                        if(audioSource.enabled) //To remove a pesky and quite unlikely warning when the audiosource is out of scenebounds
                            audioSource.Play();
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
            audioSource.volume = (scene.sceneData.id == currentSceneId) ? model.volume : 0f;
        }

        private void OnDestroy()
        {
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
                audioSource.PlayOneShot(obj.audioClip);
            }

            obj.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
        }
    }
}