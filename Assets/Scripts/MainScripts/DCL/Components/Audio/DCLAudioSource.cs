using DCL.Components;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    public class DCLAudioSource : BaseComponent
    {
        public override string componentName => "AudioSource";

        [System.Serializable]
        public class Model
        {
            public string audioClipId;
            public bool playing;
            public float volume;
            public bool loop;
            public float pitch;
        }

        Model model;
        AudioSource audioSource;
        DCLAudioClip lastDCLAudioClip;

        public void InitDCLAudioClip(DCLAudioClip dclAudioClip)
        {
            if (lastDCLAudioClip != null)
                lastDCLAudioClip.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;

            lastDCLAudioClip = dclAudioClip;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            audioSource = Utils.GetOrCreateComponent<AudioSource>(gameObject);
            model = Utils.SafeFromJson<Model>(newJson);

            audioSource.volume = model.volume;
            audioSource.loop = model.loop;
            audioSource.pitch = model.pitch;

            if (model.playing)
            {
                DCLAudioClip dclAudioClip = scene.GetSharedComponent(model.audioClipId) as DCLAudioClip;
                
                if (dclAudioClip != null)
                {
                    InitDCLAudioClip(dclAudioClip);
                    //NOTE(Brian): Play if finished loading, otherwise will wait for the loading to complete (or fail).
                    if (dclAudioClip.loadingState == DCLAudioClip.LoadState.LOADING_COMPLETED)
                    {
                        audioSource.PlayOneShot(dclAudioClip.audioClip);
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
