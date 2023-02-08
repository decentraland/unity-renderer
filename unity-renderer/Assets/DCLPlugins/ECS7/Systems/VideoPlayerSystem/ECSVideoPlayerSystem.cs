using DCL.ECS7.InternalComponents;
using System.Linq;

namespace ECSSystems.VideoPlayerSystem
{
    public class ECSVideoPlayerSystem
    {
        public readonly IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent;
        public readonly IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent;


        public ECSVideoPlayerSystem(
            IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent,
            IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent)
        {
            this.videoPlayerComponent = videoPlayerComponent;
            this.videoMaterialComponent = videoMaterialComponent;
        }

        public void Update()
        {
            var playerComponents = videoPlayerComponent.GetForAll();

            for (int i = 0; i < playerComponents.Count; ++i)
            {
                var playerComponentData = playerComponents[i].value;
                var playerModel = playerComponentData.model;

                playerModel.videoPlayer.Update();
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
