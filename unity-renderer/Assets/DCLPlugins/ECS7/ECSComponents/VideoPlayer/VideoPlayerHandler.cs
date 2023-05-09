using DCL.Components;
using DCL.Components.Video.Plugin;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCL.ECSComponents
{
    public class VideoPlayerHandler : IECSComponentHandler<PBVideoPlayer>
    {
        private static readonly string[] NO_STREAM_EXTENSIONS = new[] { ".mp4", ".ogg", ".mov", ".webm" };

        internal DataStore_LoadingScreen.DecoupledLoadingScreen loadingScreen;
        internal PBVideoPlayer lastModel = null;
        internal WebVideoPlayer videoPlayer;

        // Flags to check if we can activate the video
        internal bool isRendererActive = false;
        internal bool hadUserInteraction = false;
        internal bool isValidUrl = false;

        private readonly IInternalECSComponent<InternalVideoPlayer> videoPlayerInternalComponent;
        private bool canVideoBePlayed => isRendererActive && hadUserInteraction && isValidUrl;

        public VideoPlayerHandler(
            IInternalECSComponent<InternalVideoPlayer> videoPlayerInternalComponent,
            DataStore_LoadingScreen.DecoupledLoadingScreen loadingScreen)
        {
            this.videoPlayerInternalComponent = videoPlayerInternalComponent;
            this.loadingScreen = loadingScreen;
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
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Helpers.Utils.OnCursorLockChanged -= OnCursorLockChanged;
            loadingScreen.visible.OnChange -= OnLoadingScreenStateChanged;

            // ECSVideoPlayerSystem.Update() will run a video events check before the component is removed
            videoPlayerInternalComponent.RemoveFor(scene, entity, new InternalVideoPlayer()
            {
                removed = true
            });

            videoPlayer?.Dispose();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBVideoPlayer model)
        {
            // Setup video player
            if (lastModel == null || lastModel.Src != model.Src)
            {
                videoPlayer?.Dispose();

                var id = entity.entityId.ToString();
                bool isStream = !NO_STREAM_EXTENSIONS.Any(x => model.Src.EndsWith(x));
                string videoUrl = model.GetVideoUrl(scene.contentProvider, scene.sceneData.requiredPermissions, scene.sceneData.allowedMediaHostnames);

                isValidUrl = !string.IsNullOrEmpty(videoUrl);
                if (!isValidUrl)
                    return;

                videoPlayer = new WebVideoPlayer(id, videoUrl, isStream, DCLVideoTexture.videoPluginWrapperBuilder.Invoke());
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
            videoPlayer.SetVolume(model.GetVolume());
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
    }
}
