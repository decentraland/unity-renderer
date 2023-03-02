using System;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using DCL.SettingsCommon;
using UnityEngine;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCL.ECSComponents
{
    public class ECSAudioSourceComponentHandler : IECSComponentHandler<PBAudioSource>
    {
        internal AudioSource audioSource;
        internal AssetPromise_AudioClip promiseAudioClip;

        private bool isAudioClipReady = false;

        private PBAudioSource model;
        private IParcelScene scene;
        private AudioClip audioClip;

        private readonly DataStore dataStore;
        private readonly Settings settings;
        private readonly AssetPromiseKeeper_AudioClip keeperAudioClip;
        private readonly IntVariable sceneNumber;
        private readonly IInternalECSComponent<InternalAudioSource> audioSourceInternalComponent;

        public ECSAudioSourceComponentHandler(DataStore dataStoreInstance, Settings settingsInstance, AssetPromiseKeeper_AudioClip keeperInstance, IntVariable sceneNumber, IInternalECSComponent<InternalAudioSource> audioSourceInternalComponent)
        {
            dataStore = dataStoreInstance;
            settings = settingsInstance;
            keeperAudioClip = keeperInstance;
            this.sceneNumber = sceneNumber;
            this.audioSourceInternalComponent = audioSourceInternalComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            audioSource = entity.gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
            audioSource.dopplerLevel = 0.1f;

            if (settings != null)
                settings.audioSettings.OnChanged += OnAudioSettingsChanged;

            dataStore.virtualAudioMixer.sceneSFXVolume.OnChange += OnVirtualAudioMixerChangedValue;
            sceneNumber.OnChange += OnCurrentSceneChanged;

            audioSourceInternalComponent.PutFor(scene, entity, new InternalAudioSource()
            {
                audioSource = this.audioSource
            });
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            audioSourceInternalComponent.RemoveFor(scene, entity, new InternalAudioSource() { audioSource = null });
            Dispose(entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAudioSource model)
        {
            bool isSameClip = model.AudioClipUrl == this.model?.AudioClipUrl;
            this.model = model;

            // If the clip has changed, we need to forget the old clip
            if (!isSameClip && promiseAudioClip != null)
            {
                isAudioClipReady = false;
                if (audioClip != null)
                    dataStore.sceneWorldObjects.RemoveAudioClip(scene.sceneData.sceneNumber, audioClip);
                DisposePromise();
            }

            ApplyCurrentModel();

            if (!isAudioClipReady && !isSameClip)
            {
                promiseAudioClip = new AssetPromise_AudioClip(model.AudioClipUrl, scene.contentProvider);
                promiseAudioClip.OnSuccessEvent += OnAudioClipLoadComplete;
                promiseAudioClip.OnFailEvent += OnAudioClipLoadFail;

                keeperAudioClip.Keep(promiseAudioClip);
            }
        }

        private void DisposePromise()
        {
            if (promiseAudioClip == null)
                return;

            promiseAudioClip.OnSuccessEvent += OnAudioClipLoadComplete;
            promiseAudioClip.OnFailEvent += OnAudioClipLoadFail;

            keeperAudioClip.Forget(promiseAudioClip);
        }
        private void Dispose(IDCLEntity entity)
        {
            DisposePromise();

            if (audioClip != null)
                dataStore.sceneWorldObjects.RemoveAudioClip(scene.sceneData.sceneNumber, audioClip);

            sceneNumber.OnChange -= OnCurrentSceneChanged;

            if (settings != null)
                settings.audioSettings.OnChanged -= OnAudioSettingsChanged;

            dataStore.virtualAudioMixer.sceneSFXVolume.OnChange -= OnVirtualAudioMixerChangedValue;
            if (audioSource != null)
            {
                GameObject.Destroy(audioSource);
                audioSource = null;
            }
        }

        private void ApplyCurrentModel()
        {
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource is null!.");
                return;
            }

            UpdateAudioSourceVolume();
            audioSource.loop = model.Loop;
            audioSource.pitch = model.GetPitch();
            audioSource.spatialBlend = 1;
            audioSource.dopplerLevel = 0.1f;

            if (!model.Playing)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
            else if(isAudioClipReady)
            {
                audioSource.Play();
            }
        }

        private void ApplyLoadedAudioClip(AudioClip clip)
        {
            isAudioClipReady = true;
            if (audioSource.clip != clip)
                audioSource.clip = clip;

            bool shouldPlay = model.Playing && !audioSource.isPlaying;

            if (audioSource.enabled && model.Playing && shouldPlay)
                audioSource.Play();
        }

        private void OnAudioClipLoadComplete(Asset_AudioClip assetAudioClip)
        {
            if (assetAudioClip.audioClip == null)
                return;

            audioClip = assetAudioClip.audioClip;

            dataStore.sceneWorldObjects.AddAudioClip(scene.sceneData.sceneNumber, audioClip);

            ApplyLoadedAudioClip(assetAudioClip.audioClip);
        }

        private void OnAudioClipLoadFail(Asset_AudioClip assetAudioClip, Exception exception)
        {
            Debug.LogError("Audio clip couldn't be loaded. Url: " +model.AudioClipUrl + "     error: " + exception.Message);
            DisposePromise();
        }

        private void OnAudioSettingsChanged(AudioSettings settings)
        {
            UpdateAudioSourceVolume();
        }

        private void OnVirtualAudioMixerChangedValue(float currentValue, float previousValue)
        {
            UpdateAudioSourceVolume();
        }

        private void UpdateAudioSourceVolume()
        {
            bool isCurrentScene = scene.isPersistent || scene.sceneData.sceneNumber == sceneNumber.Get();
            if (!isCurrentScene)
            {
                audioSource.volume = 0;
                return;
            }

            AudioSettings audioSettingsData =
                settings != null ? settings.audioSettings.Data : new AudioSettings();
            float newVolume = model.GetVolume() * Utils.ToVolumeCurve(
                dataStore.virtualAudioMixer.sceneSFXVolume.Get() * audioSettingsData.sceneSFXVolume *
                audioSettingsData.masterVolume);
            
            audioSource.volume = newVolume;
        }

        private void OnCurrentSceneChanged(int currentSceneNumber, int previousSceneNumber)
        {
            if (audioSource == null || model == null)
                return;

            float volume = 0;

            if (scene.isPersistent || scene.sceneData.sceneNumber == currentSceneNumber)
                volume = model.GetVolume();

            audioSource.volume = volume;
        }
    }
}
