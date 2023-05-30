using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ECSSystems.VideoPlayerSystem
{
    public class ECSVideoPlayerSystem
    {
        private struct VideoEventData
        {
            public VideoState videoState;
            public uint timeStamp;
        }

        private readonly IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent;
        private readonly IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent;
        private readonly IECSComponentWriter componentWriter;
        private Dictionary<IDCLEntity, VideoEventData> videoEvents = new Dictionary<IDCLEntity, VideoEventData>();

        private static readonly Vector2 VIDEO_TEXTURE_SCALE = new Vector2(1, -1);

        public ECSVideoPlayerSystem(
            IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent,
            IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent,
            IECSComponentWriter componentWriter)
        {
            this.videoPlayerComponent = videoPlayerComponent;
            this.videoMaterialComponent = videoMaterialComponent;
            this.componentWriter = componentWriter;
        }

        public void Update()
        {
            var playerComponents = videoPlayerComponent.GetForAll();

            for (int i = 0; i < playerComponents.Count; ++i)
            {
                var playerComponentData = playerComponents[i].value;
                var playerModel = playerComponentData.model;

                playerModel.videoPlayer?.Update();

                UpdateVideoEvent(playerModel, playerComponentData);
            }

            var allMaterialComponent = videoMaterialComponent.GetForAll();
            for (int i = allMaterialComponent.Count - 1; i >= 0; --i)
            {
                var materialComponentData = allMaterialComponent[i].value;
                UpdateVideoMaterial(materialComponentData, materialComponentData.model, materialComponentData.model.videoTextureDatas);
            }
        }

        private void UpdateVideoEvent(InternalVideoPlayer videoPlayerModel, ECSComponentData<InternalVideoPlayer> videoPlayerComponentData)
        {
            if (videoPlayerModel.removed)
            {
                componentWriter.RemoveComponent(videoPlayerComponentData.scene.sceneData.sceneNumber, videoPlayerComponentData.entity.entityId, ComponentID.VIDEO_EVENT);
                videoEvents.Remove(videoPlayerComponentData.entity);
                return;
            }

            bool videoEventExists = videoEvents.TryGetValue(videoPlayerComponentData.entity, out VideoEventData videoEvent);
            if (!videoEventExists)
            {
                videoEvent = new VideoEventData()
                {
                    videoState = VideoState.VsNone,
                    timeStamp = 0
                };
            }

            int previousVideoState = videoEventExists ? (int)videoEvent.videoState : -1;
            int currentVideoState = (int)videoPlayerModel.videoPlayer.GetState();
            if (previousVideoState != currentVideoState)
            {
                videoEvent.timeStamp++;
                videoEvent.videoState = (VideoState)currentVideoState;

                // save video event updated data
                videoEvents[videoPlayerComponentData.entity] = videoEvent;

                // Update GrowOnlyValueSet VideoEvent component for the video player entity
                componentWriter.AppendComponent(
                    videoPlayerComponentData.scene.sceneData.sceneNumber,
                    videoPlayerComponentData.entity.entityId,
                    ComponentID.VIDEO_EVENT,
                    new PBVideoEvent()
                    {
                        State = videoEvent.videoState,
                        CurrentOffset = videoPlayerModel.videoPlayer.GetTime(),
                        VideoLength = videoPlayerModel.videoPlayer.GetDuration(),
                        Timestamp = videoEvent.timeStamp
                    },
                    ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY
                );
            }
        }

        private void UpdateVideoMaterial( ECSComponentData<InternalVideoMaterial> materialComponentData, InternalVideoMaterial model,  IList<InternalVideoMaterial.VideoTextureData> texturesData)
        {
            for (int j = texturesData.Count - 1; j >= 0; --j)
            {
                var textureData = texturesData[j];
                var playerComponent = videoPlayerComponent.GetFor(materialComponentData.scene, textureData.videoId);

                if (playerComponent != null)
                {
                    var playerModel = playerComponent.model;
                    var videoTexture = playerModel.videoPlayer.texture;

                    if (!IsMaterialAssigned(playerModel, model, textureData.textureType) && videoTexture != null)
                    {
                        model.material.SetTexture(textureData.textureType, videoTexture);

                        // Video textures are vertically flipped since natively the buffer is read from
                        // bottom to top, we are scaling the texture in the material to fix that and avoiding the performance cost
                        // of flipping the texture by code
                        model.material.SetTextureScale(textureData.textureType, VIDEO_TEXTURE_SCALE);

                        playerModel.assignedMaterials.Add(
                            new InternalVideoPlayer.MaterialAssigned(model.material, textureData.textureType));
                    }
                }
            }
        }

        private static bool IsMaterialAssigned(InternalVideoPlayer player, InternalVideoMaterial videoMaterial, int textureType)
        {
            return player.assignedMaterials.Any(data => data.material == videoMaterial.material && data.textureType == textureType);
        }
    }
}
