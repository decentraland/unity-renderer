﻿using System;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCL.SettingsCommon;
using UnityEngine;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCL.ECSComponents
{
    public class ECSAudioStreamComponentHandler : IECSComponentHandler<PBAudioStream>, IOutOfSceneBoundariesHandler
    {
        private float settingsVolume = 0;

        internal float currentVolume = -1;
        internal bool isPlaying = false;
        internal AudioSource audioSource;
        internal PBAudioStream model;

        // Flags to check if we can activate the AudioStream
        internal bool isInsideScene = false;
        internal bool isRendererActive = false;
        internal bool isInsideBoundaries = true;

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            audioSource = entity.gameObject.AddComponent<AudioSource>();
            
            CommonScriptableObjects.sceneID.OnChange += OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
            Settings.i.audioSettings.OnChanged += OnSettingsChanged;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange += SceneSFXVolume_OnChange;
            settingsVolume = GetCalculatedSettingsVolume(Settings.i.audioSettings.Data);
            DataStore.i.sceneBoundariesChecker.Add(entity,this);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Dispose(entity);
        }
                
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAudioStream model)
        {
            // Nothing has change so we do an early return
            if(!StateHasChange(model))
                return;
                
            // We update the model and the volume
            UpdateModel(model);
            
            // In case that the audio stream can't be played we do an early return
            if (!CanAudioStreamBePlayed())
                return;
            
            // If everything went ok, we update the state
            SendUpdateAudioStreamEvent(model.Playing);
        }
        
        public void UpdateOutOfBoundariesState(bool isInsideBoundaries)
        {
            this.isInsideBoundaries = isInsideBoundaries;
            ConditionsToPlayChanged();
        }

        private void Dispose(IDCLEntity entity)
        {
            CommonScriptableObjects.sceneID.OnChange -= OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            Settings.i.audioSettings.OnChanged -= OnSettingsChanged;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange -= SceneSFXVolume_OnChange;
            DataStore.i.sceneBoundariesChecker.Remove(entity,this);
            
            StopStreaming();
            
            if (audioSource != null)
            {
                GameObject.Destroy(audioSource);
                audioSource = null;
            }
        }

        private void UpdateModel(PBAudioStream model)
        {
            this.model = model;
            currentVolume = model.Volume * settingsVolume;
        }

        private bool StateHasChange(PBAudioStream model)
        {
            // First time that the model come so the state has change
            if (this.model == null)
                return true;
            
            bool shouldChangeState = isPlaying && !model.Playing;
            bool shouldUpdateVolume = Mathf.Approximately( currentVolume, model.Volume);
            bool shouldUpdateUrl = this.model.Url == model.Url;
                
            return shouldChangeState || shouldUpdateVolume || shouldUpdateUrl;
        }

        private void ConditionsToPlayChanged()
        {
            bool canBePlayed = CanAudioStreamBePlayed();
            
            if(isPlaying && !canBePlayed)
                StopStreaming();
            if(!isPlaying && canBePlayed && model.Playing)
                StartStreaming();
        }

        private bool CanAudioStreamBePlayed()
        {
            return isInsideScene && isRendererActive && isInsideBoundaries;
        }

        private void OnSceneChanged(string sceneId, string prevSceneId)
        {
            isInsideScene = sceneId == CommonScriptableObjects.sceneID.Get();
            ConditionsToPlayChanged();
        }

        private void OnRendererStateChanged(bool isEnable, bool prevState)
        {
            isRendererActive = isEnable;
            ConditionsToPlayChanged();
        }

        private void OnSettingsChanged(AudioSettings settings)
        {
            float newSettingsVolume = GetCalculatedSettingsVolume(settings);
            if (Math.Abs(settingsVolume - newSettingsVolume) > Mathf.Epsilon)
            {
                settingsVolume = newSettingsVolume;
                SendUpdateAudioStreamEvent(isPlaying);
            }
        }

        private float GetCalculatedSettingsVolume(AudioSettings audioSettings) { return Utils.ToVolumeCurve(DataStore.i.virtualAudioMixer.sceneSFXVolume.Get() * audioSettings.sceneSFXVolume * audioSettings.masterVolume); }

        private void SceneSFXVolume_OnChange(float current, float previous) { OnSettingsChanged(Settings.i.audioSettings.Data); }

        private void StopStreaming()
        {
            SendUpdateAudioStreamEvent(false);
        }   
        
        private void StartStreaming()
        {
            SendUpdateAudioStreamEvent(true);
        }

        private void SendUpdateAudioStreamEvent(bool play)
        {
            isPlaying = play;
            WebInterface.SendAudioStreamEvent(model.Url, isPlaying, currentVolume);
        }
    }
}
