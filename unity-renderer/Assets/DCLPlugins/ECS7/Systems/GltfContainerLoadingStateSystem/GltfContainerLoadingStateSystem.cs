using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;

namespace ECSSystems.GltfContainerLoadingStateSystem
{
    public class GltfContainerLoadingStateSystem
    {
        private readonly IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent;
        private readonly IECSComponentWriter componentWriter;

        public GltfContainerLoadingStateSystem(IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent)
        {
            this.componentWriter = componentWriter;
            this.gltfContainerLoadingStateComponent = gltfContainerLoadingStateComponent;
        }

        public void Update()
        {
            var components = gltfContainerLoadingStateComponent.GetForAll();

            for (int i = 0; i < components.Count; i++)
            {
                var model = components[i].value.model;

                if (!model.dirty)
                    continue;

                IParcelScene scene = components[i].value.scene;
                IDCLEntity entity = components[i].value.entity;

                if (model.GltfContainerRemoved)
                {
                    componentWriter.RemoveComponent(
                        scene.sceneData.sceneNumber,
                        entity.entityId,
                        ComponentID.GLTF_CONTAINER_LOADING_STATE,
                        ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY);
                }
                else
                {
                    componentWriter.PutComponent(
                        scene.sceneData.sceneNumber,
                        entity.entityId,
                        ComponentID.GLTF_CONTAINER_LOADING_STATE,
                        new PBGltfContainerLoadingState()
                        {
                            CurrentState = model.LoadingState
                        },
                        ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY);
                }
            }
        }
    }
}
