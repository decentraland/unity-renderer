using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class DCLAudioSource : BaseComponent
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
        AudioSource audioSource;
        DCLAudioClip lastDCLAudioClip;

        public void InitDCLAudioClip(DCLAudioClip dclAudioClip)
        {
            if (lastDCLAudioClip != null)
            {
                lastDCLAudioClip.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
            }

            lastDCLAudioClip = dclAudioClip;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            audioSource = gameObject.GetOrCreateComponent<AudioSource>();
            model = SceneController.i.SafeFromJson<Model>(newJson);

            audioSource.volume = model.volume;
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

            yield return null;
        }

        private void OnDestroy()
        {
            //NOTE(Brian): Unsuscribe events.
            InitDCLAudioClip(null);
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
