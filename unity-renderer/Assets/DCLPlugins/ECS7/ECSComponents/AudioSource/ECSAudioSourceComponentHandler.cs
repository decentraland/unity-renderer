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
    public class ECSAudioSourceComponentHandler : IECSComponentHandler<ECSAudioSource>, IOutOfSceneBoundariesHandler
    {
        public float playTime => audioSource.time;
        internal AudioSource audioSource;
        internal AssetPromise_AudioClip promiseAudioClip;
        
        public long playedAtTimestamp = 0;
        
        // TODO: We should figure out how to change this value
        private bool isOutOfBoundaries = false;
        
        private ECSAudioSource model;
        private IParcelScene scene;
        private AudioClip audioClip;

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            audioSource = entity.gameObject.AddComponent<AudioSource>();

            if (Settings.i != null)
                Settings.i.audioSettings.OnChanged += OnAudioSettingsChanged;
    
            DataStore.i.sceneBoundariesChecker.Add(entity,this);
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange += OnVirtualAudioMixerChangedValue;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Dispose();
            DataStore.i.sceneBoundariesChecker.Remove(entity,this);
        }

        private void Dispose()
        {
            if(promiseAudioClip != null)
                AssetPromiseKeeper_AudioClip.i.Forget(promiseAudioClip);
            
            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneChanged;

            if (Settings.i != null)
                Settings.i.audioSettings.OnChanged -= OnAudioSettingsChanged;
            
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange -= OnVirtualAudioMixerChangedValue;
            if (audioSource != null)
            {
                GameObject.Destroy(audioSource);
                audioSource = null;
            }
        }
        
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSAudioSource model)
        {
            bool isSameClip = model.audioClipUrl == this.model?.audioClipUrl;
            this.scene = scene;
            this.model = model;
            
            ApplyCurrentModel();
            
            // If the clip has changed, we need to forget the old clip
            if(!isSameClip && promiseAudioClip != null)
                AssetPromiseKeeper_AudioClip.i.Forget(promiseAudioClip);
            
            promiseAudioClip = new AssetPromise_AudioClip(model.audioClipUrl, scene.contentProvider);
            promiseAudioClip.OnSuccessEvent += OnComplete;
            promiseAudioClip.OnFailEvent += OnFail;

            AssetPromiseKeeper_AudioClip.i.Keep(promiseAudioClip);
            
            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneChanged;
            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneChanged;
        }
        
        private void ApplyCurrentModel()
        {
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource is null!.");
                return;
            }
            
            UpdateAudioSourceVolume();
            audioSource.loop = model.loop;
            audioSource.pitch = model.pitch;
            audioSource.spatialBlend = 1;
            audioSource.dopplerLevel = 0.1f;

            if (!model.playing)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
        
        private void ApplyLoadedAudioClip(AudioClip clip)
        {
            if (audioSource.clip != clip)
                audioSource.clip = clip;
            
            bool shouldPlay = playedAtTimestamp != model.playedAtTimestamp ||
                              (model.playing && !audioSource.isPlaying);

            if (audioSource.enabled && model.playing && shouldPlay)
            {
                //To remove a pesky and quite unlikely warning when the audiosource is out of scenebounds
                audioSource.Play();
            }

            playedAtTimestamp = model.playedAtTimestamp;
        }
        
        public void UpdateOutOfBoundariesState(bool isInsideBoundaries)
        {
            if (scene.isPersistent)
                isInsideBoundaries = true;

            isOutOfBoundaries = !isInsideBoundaries;
            UpdateAudioSourceVolume();
        }

        private void OnComplete(Asset_AudioClip assetAudioClip)
        {
            if (assetAudioClip.audioClip == null)
                return;

            if (audioClip != null)
                DataStore.i.sceneWorldObjects.RemoveAudioClip(scene.sceneData.id, audioClip);

            audioClip = assetAudioClip.audioClip;

            DataStore.i.sceneWorldObjects.AddAudioClip(scene.sceneData.id, audioClip);
            
            ApplyLoadedAudioClip(assetAudioClip.audioClip);
        }

        private void OnFail(Asset_AudioClip assetAudioClip, Exception exception)
        {
            Debug.LogError("Audio clip couldn't be loaded. Url: " +model.audioClipUrl + "     error: " + exception.Message);
            AssetPromiseKeeper_AudioClip.i.Forget(promiseAudioClip);
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
                    Settings.i != null ? Settings.i.audioSettings.Data : new AudioSettings();
                newVolume = model.volume * Utils.ToVolumeCurve(
                    DataStore.i.virtualAudioMixer.sceneSFXVolume.Get() * audioSettingsData.sceneSFXVolume *
                    audioSettingsData.masterVolume);
            }

            bool isCurrentScene = scene.isPersistent || scene.sceneData.id == CommonScriptableObjects.sceneID.Get();

            audioSource.volume = isCurrentScene ? newVolume : 0f;
        }
        
        private void OnCurrentSceneChanged(string currentSceneId, string previousSceneId)
        {
            if (audioSource == null)
                return;
            
            float volume = 0;

            if (scene.isPersistent || scene.sceneData.id == currentSceneId)
                volume = model.volume;
            

            audioSource.volume = volume;
        }
    }
}
