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
    public class ECSAudioStreamComponentHandler : IECSComponentHandler<ECSAudioStream>, IOutOfSceneBoundariesHandler
    {
        public float playTime => audioSource.time;
        public long playedAtTimestamp = 0;
        
        internal AudioSource audioSource;
        internal AssetPromise_AudioClip promiseAudioClip;
        


        private float settingsVolume = 0;
        private bool isOutOfBoundaries = false;
        
        private ECSAudioStream model;
        private IParcelScene scene;
        private AudioClip audioClip;

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            audioSource = entity.gameObject.AddComponent<AudioSource>();
            
            CommonScriptableObjects.sceneID.OnChange += OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
            Settings.i.audioSettings.OnChanged += OnSettingsChanged;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange += SceneSFXVolume_OnChange;
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
            
            CommonScriptableObjects.sceneID.OnChange -= OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            Settings.i.audioSettings.OnChanged -= OnSettingsChanged;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange -= SceneSFXVolume_OnChange;
            StopStreaming();
            
            if (audioSource != null)
            {
                GameObject.Destroy(audioSource);
                audioSource = null;
            }
        }
        
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSAudioStream model)
        {
            settingsVolume = GetCalculatedSettingsVolume(Settings.i.audioSettings.Data);

            UpdatePlayingState(model);
        }
        
         private void UpdatePlayingState(ECSAudioStream model)
        {
            bool canPlayStream = scene.isPersistent || scene.sceneData.id == CommonScriptableObjects.sceneID.Get();
            canPlayStream &= CommonScriptableObjects.rendererState;
            
            bool shouldStopStream = (isPlaying && !model.playing) || (isPlaying && !canPlayStream);
            bool shouldStartStream = !isPlaying && canPlayStream && model.playing;

            if (shouldStopStream)
            {
                StopStreaming();
                return;
            }

            if (shouldStartStream)
            {
                StartStreaming();
                return;
            }

            if (forceStateUpdate)
            {
                if (isPlaying)
                    StartStreaming();
                else
                    StopStreaming();
            }
        }

        private void OnSceneChanged(string sceneId, string prevSceneId) { UpdatePlayingState(false); }

        private void OnRendererStateChanged(bool isEnable, bool prevState)
        {
            if (isEnable)
            {
                UpdatePlayingState(false);
            }
        }

        private void OnSettingsChanged(AudioSettings settings)
        {
            float newSettingsVolume = GetCalculatedSettingsVolume(settings);
            if (Math.Abs(settingsVolume - newSettingsVolume) > Mathf.Epsilon)
            {
                settingsVolume = newSettingsVolume;
                UpdatePlayingState(true);
            }
        }

        private float GetCalculatedSettingsVolume(AudioSettings audioSettings) { return Utils.ToVolumeCurve(DataStore.i.virtualAudioMixer.sceneSFXVolume.Get() * audioSettings.sceneSFXVolume * audioSettings.masterVolume); }

        private void SceneSFXVolume_OnChange(float current, float previous) { OnSettingsChanged(Settings.i.audioSettings.Data); }

        private void StopStreaming()
        {
            Model model = (Model) this.model;
            isPlaying = false;
            Interface.WebInterface.SendAudioStreamEvent(model.url, false, model.volume * settingsVolume);
        }

        private void StartStreaming()
        {
            Model model = (Model) this.model;
            isPlaying = true;
            Interface.WebInterface.SendAudioStreamEvent(model.url, true, model.volume * settingsVolume);
        }

        public void UpdateOutOfBoundariesState(bool isInsideBoundaries)
        {
            if (!isPlaying)
                return;

            if (isInsideBoundaries)
            {
                StartStreaming();
            }
            else
            {
                Model model = (Model) this.model;
                //Set volume to 0 (temporary solution until the refactor in #1421)
                Interface.WebInterface.SendAudioStreamEvent(model.url, true, 0);
            }
        }

    }
}
