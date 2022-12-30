using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL.Components;
using DCL.Components.Video.Plugin;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using Decentraland.Common;
using System;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class VideoPlayerHandler : IECSComponentHandler<PBVideoPlayer>
    {
        private PBVideoPlayer lastModel = null;
        internal WebVideoPlayer videoPlayer;

        private readonly IInternalECSComponent<InternalVideoPlayer> videoPlayerInternalComponent;

        public VideoPlayerHandler(IInternalECSComponent<InternalVideoPlayer> videoPlayerInternalComponent)
        {
            this.videoPlayerInternalComponent = videoPlayerInternalComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            videoPlayerInternalComponent.RemoveFor(scene, entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBVideoPlayer model)
        {
            if (lastModel != null && lastModel.Equals(model))
                return;

            // detect change of the state
            if (lastModel == null || lastModel.Src != model.Src)
            {
                videoPlayer?.Dispose();

                var id = entity.entityId.ToString();
                videoPlayer = new WebVideoPlayer(id, model.Src, true, DCLVideoTexture.videoPluginWrapperBuilder.Invoke());
                videoPlayerInternalComponent.PutFor(scene, entity, new InternalVideoPlayer()
                {
                    videoPlayer = videoPlayer,
                    assignedMaterials = new List<InternalVideoPlayer.MaterialAssigned>(),
                });
            }

            // detect change of the state
            if (lastModel == null || lastModel.IsPlaying() != model.IsPlaying())
            {
                if (model.IsPlaying()) { videoPlayer.Play(); }
                else { videoPlayer.Pause(); }
            }

            // detect change of the state
            float lastPosition = lastModel?.GetPosition() ?? 0.0f;
            if (Math.Abs(lastPosition - model.GetPosition()) > 0.01f) // 0.01s of tolerance
            {
                videoPlayer.SetTime(model.GetPosition());
            }

            videoPlayer.SetVolume(model.GetVolume());

            videoPlayer.SetPlaybackRate(model.GetPlaybackRate());

            lastModel = model;
        }
    }
}
