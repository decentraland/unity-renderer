using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCL.SettingsCommon;
using System;
using UnityEngine;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCL.ECSComponents
{
    public class ECSAudioStreamComponentHandler : IECSComponentHandler<PBAudioStream>
    {
        private float settingsVolume = 0;

        internal float currentVolume = -1;
        internal bool isPlaying = false;
        internal PBAudioStream model;
        internal IParcelScene scene;
        internal string url;

        // Flags to check if we can activate the AudioStream
        internal bool isInsideScene = false;
        internal bool isRendererActive = false;
        internal bool hadUserInteraction = false;
        internal bool isValidUrl = false;

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            if (!scene.isPersistent)
                CommonScriptableObjects.sceneNumber.OnChange += OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
            Settings.i.audioSettings.OnChanged += OnSettingsChanged;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange += SceneSFXVolume_OnChange;
            settingsVolume = GetCalculatedSettingsVolume(Settings.i.audioSettings.Data);

            isRendererActive = CommonScriptableObjects.rendererState.Get();
            isInsideScene = scene.isPersistent || scene.sceneData.sceneNumber == CommonScriptableObjects.sceneNumber.Get();

            // Browsers don't allow streaming if the user didn't interact first, ending up in a fake 'playing' state.
            hadUserInteraction = Utils.IsCursorLocked;
            if (!hadUserInteraction)
            {
                Utils.OnCursorLockChanged += OnCursorLockChanged;
            }
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Utils.OnCursorLockChanged -= OnCursorLockChanged;
            CommonScriptableObjects.sceneNumber.OnChange -= OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            Settings.i.audioSettings.OnChanged -= OnSettingsChanged;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.OnChange -= SceneSFXVolume_OnChange;

            StopStreaming();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAudioStream model)
        {
            // Nothing has change so we do an early return
            if (!StateHasChange(model))
                return;

            // We update the model and the volume
            UpdateModel(model);

            isValidUrl = UtilsScene.TryGetMediaUrl(model.Url, scene.contentProvider,
                scene.sceneData.requiredPermissions, scene.sceneData.allowedMediaHostnames, out string newUrl);

            if (!isValidUrl)
                StopStreaming();

            // In case that the audio stream can't be played we do an early return
            if (!CanAudioStreamBePlayed() || !isValidUrl)
                return;

            url = newUrl;

            // If everything went ok, we update the state
            SendUpdateAudioStreamEvent(model.Playing);
        }

        private void UpdateModel(PBAudioStream model)
        {
            this.model = model;
            currentVolume = model.GetVolume() * settingsVolume;
        }

        private bool StateHasChange(PBAudioStream model)
        {
            // First time that the model come so the state has change
            if (this.model == null)
                return true;

            bool shouldChangeState = isPlaying && !model.Playing;
            bool shouldUpdateVolume = Mathf.Approximately(currentVolume, model.GetVolume());
            bool shouldUpdateUrl = this.model.Url == model.Url;

            return shouldChangeState || shouldUpdateVolume || shouldUpdateUrl;
        }

        private void ConditionsToPlayChanged()
        {
            bool canBePlayed = CanAudioStreamBePlayed();

            if (isPlaying && !canBePlayed)
                StopStreaming();

            if (!isPlaying && canBePlayed && model.Playing && isValidUrl)
                StartStreaming();
        }

        private bool CanAudioStreamBePlayed()
        {
            return isInsideScene && isRendererActive && hadUserInteraction;
        }

        private void OnSceneChanged(int sceneNumber, int prevSceneNumber)
        {
            isInsideScene = sceneNumber == scene.sceneData.sceneNumber;
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

                if (isValidUrl)
                    SendUpdateAudioStreamEvent(isPlaying);
            }
        }

        private float GetCalculatedSettingsVolume(AudioSettings audioSettings)
        {
            return Utils.ToVolumeCurve(DataStore.i.virtualAudioMixer.sceneSFXVolume.Get() * audioSettings.sceneSFXVolume * audioSettings.masterVolume);
        }

        private void SceneSFXVolume_OnChange(float current, float previous)
        {
            OnSettingsChanged(Settings.i.audioSettings.Data);
        }

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
            WebInterface.SendAudioStreamEvent(url, isPlaying, currentVolume);
        }

        private void OnCursorLockChanged(bool isLocked)
        {
            if (!isLocked) return;

            hadUserInteraction = true;
            Utils.OnCursorLockChanged -= OnCursorLockChanged;
            ConditionsToPlayChanged();
        }
    }
}
