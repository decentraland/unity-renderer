using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class GltfContainerLoadingStateRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public GltfContainerLoadingStateRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBGltfContainerLoadingState>(
                new WrappedComponentPool<IWrappedComponent<PBGltfContainerLoadingState>>(10,
                    () => new ProtobufWrappedComponent<PBGltfContainerLoadingState>(new PBGltfContainerLoadingState()))
            );

            factory.AddOrReplaceComponent(
                componentId,
                null,
                // ProtoSerialization.Deserialize<PBGltfContainerLoadingState>, // FD::
                iecsComponentPool: poolWrapper // FD:: changed
                );

            componentWriter.AddOrReplaceComponentSerializer<PBGltfContainerLoadingState>(componentId, ProtoSerialization.Serialize);

            this.factory = factory;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        public void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
        }
    }
}
