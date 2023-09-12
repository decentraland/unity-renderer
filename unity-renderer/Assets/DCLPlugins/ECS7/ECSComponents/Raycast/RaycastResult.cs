using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCLPlugins.ECSComponents
{
    public class RaycastResultRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;

        public RaycastResultRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBRaycastResult>(
                new WrappedComponentPool<IWrappedComponent<PBRaycastResult>>(10,
                    () => new ProtobufWrappedComponent<PBRaycastResult>(new PBRaycastResult()))
            );

            factory.AddOrReplaceComponent(componentId,
                null,
                ProtoSerialization.Deserialize<PBRaycastResult>, // FD::
                iecsComponentPool: poolWrapper
                );

            componentWriter.AddOrReplaceComponentSerializer<PBRaycastResult>(componentId, ProtoSerialization.Serialize);

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
