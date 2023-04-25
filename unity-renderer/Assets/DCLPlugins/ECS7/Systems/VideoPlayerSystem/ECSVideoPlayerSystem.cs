using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using System.Linq;
using UnityEngine;

namespace ECSSystems.VideoPlayerSystem
{
    public class ECSVideoPlayerSystem
    {
        public readonly IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent;
        public readonly IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent;
        public readonly ECSComponent<PBVideoEvent> videoEventComponent;

        private static readonly Vector2 VIDEO_TEXTURE_SCALE = new Vector2(1, -1);

        public ECSVideoPlayerSystem(
            IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent,
            IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent,
            ECSComponent<PBVideoEvent> videoEventComponent)
        {
            this.videoPlayerComponent = videoPlayerComponent;
            this.videoMaterialComponent = videoMaterialComponent;
            this.videoEventComponent = videoEventComponent;
        }

        public void Update()
        {
            var playerComponents = videoPlayerComponent.GetForAll();

            for (int i = 0; i < playerComponents.Count; ++i)
            {
                var playerComponentData = playerComponents[i].value;
                var playerModel = playerComponentData.model;

                var previousVideoState = playerModel.videoPlayer.GetState();
                playerModel.videoPlayer.Update();

                if (videoEventComponent.HasComponent(playerComponentData.scene, playerComponentData.entity))
                {
                    var currentVideoState = playerModel.videoPlayer.GetState();
                    if (previousVideoState != currentVideoState)
                    {
                        videoEventComponent.SetModel(playerComponentData.scene,
                            playerComponentData.entity,
                            new PBVideoEvent()
                            {
                                State = (VideoState)currentVideoState,
                                CurrentOffset = playerModel.videoPlayer.GetTime(),
                                VideoLength = playerModel.videoPlayer.GetDuration()
                            });
                    }
                }
            }

            var allMaterialComponent = videoMaterialComponent.GetForAll();

            for (int i = allMaterialComponent.Count - 1; i >= 0; --i)
            {
                var materialComponentData = allMaterialComponent[i].value;
                var model = materialComponentData.model;
                var texturesData = model.videoTextureDatas;

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
        }

        private static bool IsMaterialAssigned(InternalVideoPlayer player, InternalVideoMaterial videoMaterial, int textureType)
        {
            return player.assignedMaterials.Any(data => data.material == videoMaterial.material && data.textureType == textureType);
        }
    }
}
