using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using DCL.SettingsCommon;
using UnityEngine;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCL.ECSComponents
{
    public class ECSAudioSourceComponentHandler : IECSComponentHandler<PBAudioSource>, IOutOfSceneBoundariesHandler
    {
        internal AudioSource audioSource;
        internal AssetPromise_AudioClip promiseAudioClip;
        
        private bool isOutOfBoundaries = false;
        private bool isAudioClipReady = false;
        
        private PBAudioSource model;
        private IParcelScene scene;
        private AudioClip audioClip;

        private DataStore dataStore;
        private Settings settings;
        private AssetPromiseKeeper_AudioClip keeperAudioClip;
        private StringVariable sceneID;

        public ECSAudioSourceComponentHandler(DataStore dataStoreInstance, Settings settingsInstance, AssetPromiseKeeper_AudioClip keeperInstance, StringVariable sceneId)
        {
            dataStore = dataStoreInstance;
            settings = settingsInstance;
            keeperAudioClip = keeperInstance;
            this.sceneID = sceneId;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            audioSource = entity.gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
            audioSource.dopplerLevel = 0.1f;

            if (settings != null)
                settings.audioSettings.OnChanged += OnAudioSettingsChanged;
    
            dataStore.sceneBoundariesChecker.Add(entity,this);
            dataStore.virtualAudioMixer.sceneSFXVolume.OnChange += OnVirtualAudioMixerChangedValue;
            sceneID.OnChange += OnCurrentSceneChanged;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
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
                    dataStore.sceneWorldObjects.RemoveAudioClip(scene.sceneData.id, audioClip);
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
                
        void IOutOfSceneBoundariesHandler.UpdateOutOfBoundariesState(bool isInsideBoundaries)
        {
            if (scene.isPersistent)
                isInsideBoundaries = true;

            isOutOfBoundaries = !isInsideBoundaries;
            UpdateAudioSourceVolume();
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
                dataStore.sceneWorldObjects.RemoveAudioClip(scene.sceneData.id, audioClip);
            dataStore.sceneBoundariesChecker.Remove(entity,this);
            
            sceneID.OnChange -= OnCurrentSceneChanged;

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

            dataStore.sceneWorldObjects.AddAudioClip(scene.sceneData.id, audioClip);
            
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
            float newVolume = 0;
            
            // isOutOfBoundaries will always be false for global scenes.
            if (!isOutOfBoundaries)
            {
                AudioSettings audioSettingsData =
                    settings != null ? settings.audioSettings.Data : new AudioSettings();
                newVolume = model.GetVolume() * Utils.ToVolumeCurve(
                    dataStore.virtualAudioMixer.sceneSFXVolume.Get() * audioSettingsData.sceneSFXVolume *
                    audioSettingsData.masterVolume);
            }

            bool isCurrentScene = scene.isPersistent || scene.sceneData.id == sceneID.Get();

            audioSource.volume = isCurrentScene ? newVolume : 0f;
        }
        
        private void OnCurrentSceneChanged(string currentSceneId, string previousSceneId)
        {
            if (audioSource == null || model == null)
                return;
            
            float volume = 0;

            if (scene.isPersistent || scene.sceneData.id == currentSceneId)
                volume = model.GetVolume();
            
            audioSource.volume = volume;
        }
    }
}
