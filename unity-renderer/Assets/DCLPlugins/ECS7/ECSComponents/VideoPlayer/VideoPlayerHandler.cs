using DCL.Components;
using DCL.Components.Video.Plugin;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.SettingsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCL.ECSComponents
{
    public class VideoPlayerHandler : IECSComponentHandler<PBVideoPlayer>
    {
        private static readonly string[] NO_STREAM_EXTENSIONS = new[] { ".mp4", ".ogg", ".mov", ".webm" };

        internal DataStore_LoadingScreen.DecoupledLoadingScreen loadingScreen;
        private readonly ISettingsRepository<AudioSettings> audioSettings;
        private readonly DataStore_VirtualAudioMixer audioMixerDataStore;
        private readonly IntVariable currentPlayerSceneNumber;

        internal PBVideoPlayer lastModel = null;
        internal WebVideoPlayer videoPlayer;

        // Flags to check if we can activate the video
        internal bool isRendererActive = false;
        internal bool hadUserInteraction = false;
        internal bool isValidUrl = false;

        private readonly IInternalECSComponent<InternalVideoPlayer> videoPlayerInternalComponent;
        private IParcelScene currentScene;
        private bool canVideoBePlayed => isRendererActive && hadUserInteraction && isValidUrl;

        public VideoPlayerHandler(
            IInternalECSComponent<InternalVideoPlayer> videoPlayerInternalComponent,
            DataStore_LoadingScreen.DecoupledLoadingScreen loadingScreen,
            ISettingsRepository<AudioSettings> audioSettings,
            DataStore_VirtualAudioMixer audioMixerDataStore,
            IntVariable currentPlayerSceneNumber)
        {
            this.videoPlayerInternalComponent = videoPlayerInternalComponent;
            this.loadingScreen = loadingScreen;
            this.audioSettings = audioSettings;
            this.audioMixerDataStore = audioMixerDataStore;
            this.currentPlayerSceneNumber = currentPlayerSceneNumber;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            isRendererActive = !loadingScreen.visible.Get();

            // We need to check if the user interacted with the application before playing the video,
            // otherwise browsers won't play the video, ending up in a fake 'playing' state.
            hadUserInteraction = Helpers.Utils.IsCursorLocked;

            if (!hadUserInteraction)
                Helpers.Utils.OnCursorLockChanged += OnCursorLockChanged;
            loadingScreen.visible.OnChange += OnLoadingScreenStateChanged;
            audioSettings.OnChanged += OnAudioSettingsChanged;
            audioMixerDataStore.sceneSFXVolume.OnChange += OnSceneSfxVolumeChanged;
            currentPlayerSceneNumber.OnChange += OnPlayerSceneChanged;
            currentScene = scene;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Helpers.Utils.OnCursorLockChanged -= OnCursorLockChanged;
            loadingScreen.visible.OnChange -= OnLoadingScreenStateChanged;
            audioSettings.OnChanged -= OnAudioSettingsChanged;
            audioMixerDataStore.sceneSFXVolume.OnChange -= OnSceneSfxVolumeChanged;
            currentPlayerSceneNumber.OnChange -= OnPlayerSceneChanged;

            // ECSVideoPlayerSystem.Update() will run a video events check before the component is removed
            videoPlayerInternalComponent.RemoveFor(scene, entity, new InternalVideoPlayer()
            {
                removed = true
            });

            videoPlayer?.Dispose();
            currentScene = null;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBVideoPlayer model)
        {
            // Setup video player
            if (lastModel == null || lastModel.Src != model.Src)
            {
                videoPlayer?.Dispose();

                var id = entity.entityId.ToString();

                VideoType videoType = VideoType.Common;
                if (model.Src.StartsWith("livekit-video://"))
                    videoType = VideoType.LiveKit;
                else if (!NO_STREAM_EXTENSIONS.Any(x => model.Src.EndsWith(x)))
                    videoType = VideoType.Hls;

                string videoUrl = videoType != VideoType.LiveKit
                    ? model.GetVideoUrl(scene.contentProvider, scene.sceneData.requiredPermissions, scene.sceneData.allowedMediaHostnames)
                    : model.Src;

                isValidUrl = !string.IsNullOrEmpty(videoUrl);
                if (!isValidUrl)
                    return;

                videoPlayer = new WebVideoPlayer(id, videoUrl, videoType, DCLVideoTexture.videoPluginWrapperBuilder.Invoke());
                videoPlayerInternalComponent.PutFor(scene, entity, new InternalVideoPlayer()
                {
                    videoPlayer = videoPlayer,
                    assignedMaterials = new List<InternalVideoPlayer.MaterialAssigned>(),
                });
            }

            // Apply model values except 'Playing'
            float lastPosition = lastModel?.GetPosition() ?? 0.0f;
            if (Math.Abs(lastPosition - model.GetPosition()) > 0.01f) // 0.01s of tolerance
                videoPlayer.SetTime(model.GetPosition());

            UpdateVolume(model, audioSettings.Data, audioMixerDataStore.sceneSFXVolume.Get(), currentPlayerSceneNumber);

            videoPlayer.SetPlaybackRate(model.GetPlaybackRate());
            videoPlayer.SetLoop(model.GetLoop());

            lastModel = model;

            ConditionsToPlayVideoChanged();
        }

        private void ConditionsToPlayVideoChanged()
        {
            if (lastModel == null) return;

            bool shouldBePlaying = lastModel.IsPlaying() && canVideoBePlayed;
            if (shouldBePlaying != videoPlayer.playing)
            {
                if (shouldBePlaying)
                    videoPlayer.Play();
                else
                    videoPlayer.Pause();
            }
        }

        private void OnCursorLockChanged(bool isLocked)
        {
            if (!isLocked) return;

            hadUserInteraction = true;
            Helpers.Utils.OnCursorLockChanged -= OnCursorLockChanged;
            ConditionsToPlayVideoChanged();
        }

        private void OnLoadingScreenStateChanged(bool isScreenEnabled, bool prevState)
        {
            isRendererActive = !isScreenEnabled;
            ConditionsToPlayVideoChanged();
        }

        private void OnSceneSfxVolumeChanged(float current, float previous) =>
            UpdateVolume(lastModel, audioSettings.Data, current, currentPlayerSceneNumber);

        private void OnAudioSettingsChanged(AudioSettings settings) =>
            UpdateVolume(lastModel, settings, audioMixerDataStore.sceneSFXVolume.Get(), currentPlayerSceneNumber);

        private void OnPlayerSceneChanged(int current, int previous) =>
            UpdateVolume(lastModel, audioSettings.Data, audioMixerDataStore.sceneSFXVolume.Get(), current);

        private void UpdateVolume(PBVideoPlayer model, AudioSettings settings, float sceneMixerSfxVolume, int currentSceneNumber)
        {
            if (model == null) return;
            if (videoPlayer == null) return;

            float volume = 0;

            if (IsPlayerInSameSceneAsComponent(currentSceneNumber))
            {
                float sceneSFXSetting = settings.sceneSFXVolume;
                float masterSetting = settings.masterVolume;
                volume = model.GetVolume() * sceneMixerSfxVolume * sceneSFXSetting * masterSetting;
            }

            videoPlayer.SetVolume(volume);
        }

        private bool IsPlayerInSameSceneAsComponent(int currentSceneNumber)
        {
            if (currentScene == null)
                return false;

            if (currentSceneNumber <= 0)
                return false;

            return currentScene.sceneData?.sceneNumber == currentSceneNumber || currentScene.isPersistent;
        }
    }
}
