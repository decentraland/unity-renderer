using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents.Raycast;

namespace DCLPlugins.ECSComponents
{
    public class RaycastRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;

        public RaycastRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBRaycast>(
                new WrappedComponentPool<IWrappedComponent<PBRaycast>>(10,
                    () => new ProtobufWrappedComponent<PBRaycast>(new PBRaycast()))
            );

            factory.AddOrReplaceComponent(
                componentId,
                () => new RaycastComponentHandler(internalComponents.raycastComponent),
                // ProtoSerialization.Deserialize<PBRaycast>, // FD::
                iecsComponentPool: poolWrapper
                );
            componentWriter.AddOrReplaceComponentSerializer<PBRaycast>(componentId, ProtoSerialization.Serialize);

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
