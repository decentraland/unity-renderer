using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ECSSystems.VideoPlayerSystem
{
    public class ECSVideoPlayerSystem
    {
        private readonly IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent;
        private readonly IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent;
        private readonly IInternalECSComponent<InternalVideoEvent> videoEventComponent;
        private readonly IECSComponentWriter componentWriter;
        private HashSet<ECSComponentData<InternalVideoPlayer>> videoPlayersToRemove = new HashSet<ECSComponentData<InternalVideoPlayer>>();

        private static readonly Vector2 VIDEO_TEXTURE_SCALE = new Vector2(1, -1);

        public ECSVideoPlayerSystem(
            IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent,
            IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent,
            IInternalECSComponent<InternalVideoEvent> videoEventComponent,
            IECSComponentWriter componentWriter)
        {
            this.videoPlayerComponent = videoPlayerComponent;
            this.videoMaterialComponent = videoMaterialComponent;
            this.videoEventComponent = videoEventComponent;
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

                UpdateVideoEvent(videoEventComponent.GetFor(playerComponentData.scene, playerComponentData.entity)?.model,
                    playerModel, playerComponentData);

                if (playerModel.removed)
                    videoPlayersToRemove.Add(playerComponentData);
            }

            var allMaterialComponent = videoMaterialComponent.GetForAll();
            for (int i = allMaterialComponent.Count - 1; i >= 0; --i)
            {
                var materialComponentData = allMaterialComponent[i].value;
                UpdateVideoMaterial(materialComponentData, materialComponentData.model, materialComponentData.model.videoTextureDatas);
            }
        }

        private void UpdateVideoEvent(InternalVideoEvent videoEventModel, InternalVideoPlayer videoPlayerModel, ECSComponentData<InternalVideoPlayer> videoPlayerComponentData)
        {
            if (videoPlayerModel.removed)
            {
                // Clean up video event component
                componentWriter.RemoveComponent(videoPlayerComponentData.scene, videoPlayerComponentData.entity, ComponentID.VIDEO_EVENT);
                videoEventComponent.RemoveFor(videoPlayerComponentData.scene, videoPlayerComponentData.entity);
                return;
            }

            bool hasVideoEvent = videoEventModel != null;
            int previousVideoState = hasVideoEvent ? (int)videoEventModel.videoState : -1;
            int currentVideoState = (int)videoPlayerModel.videoPlayer.GetState();
            if (previousVideoState != currentVideoState)
            {
                uint updatedTimestamp = hasVideoEvent ? videoEventModel.timeStamp + 1 : 1;
                VideoState updatedVideoState = (VideoState)currentVideoState;

                // Update internal component for needed checks
                videoEventComponent.PutFor(videoPlayerComponentData.scene, videoPlayerComponentData.entity,
                new InternalVideoEvent(){
                    videoState = updatedVideoState,
                    timeStamp = updatedTimestamp
                });

                // Update GrowOnlyValueSet VideoEvent component for the video player entity
                componentWriter.AppendComponent(
                    videoPlayerComponentData.scene.sceneData.sceneNumber,
                    videoPlayerComponentData.entity.entityId,
                    ComponentID.VIDEO_EVENT,
                    new PBVideoEvent()
                    {
                        State = updatedVideoState,
                        CurrentOffset = videoPlayerModel.videoPlayer.GetTime(),
                        VideoLength = videoPlayerModel.videoPlayer.GetDuration(),
                        Timestamp = updatedTimestamp
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
