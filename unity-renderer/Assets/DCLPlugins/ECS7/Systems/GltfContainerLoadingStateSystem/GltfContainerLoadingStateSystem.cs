using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using System.Collections.Generic;

namespace ECSSystems.GltfContainerLoadingStateSystem
{
    public class GltfContainerLoadingStateSystem
    {
        private readonly IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<PBGltfContainerLoadingState>> componentPool;

        public GltfContainerLoadingStateSystem(IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBGltfContainerLoadingState>> componentPool,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent)
        {
            this.componentsWriter = componentsWriter;
            this.gltfContainerLoadingStateComponent = gltfContainerLoadingStateComponent;
            this.componentPool = componentPool;
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

                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                if (model.GltfContainerRemoved)
                {
                    writer.Remove(entity.entityId, ComponentID.GLTF_CONTAINER_LOADING_STATE);
                }
                else
                {
                    var componentPooled = componentPool.Get();
                    var componentModel = componentPooled.WrappedComponent.Model;
                    componentModel.CurrentState = model.LoadingState;

                    writer.Put(entity.entityId, ComponentID.GLTF_CONTAINER_LOADING_STATE, componentPooled);
                }
            }
        }
    }
}
