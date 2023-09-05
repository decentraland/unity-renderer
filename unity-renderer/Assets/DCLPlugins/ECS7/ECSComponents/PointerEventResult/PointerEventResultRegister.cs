using DCL;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCLPlugins.ECSComponents
{
    public class PointerEventResultRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;

        public PointerEventResultRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBPointerEventsResult>(
                new WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>>(10,
                    () => new ProtobufWrappedComponent<PBPointerEventsResult>(new PBPointerEventsResult()))
            );

            factory.AddOrReplaceComponent(
                componentId,
                null,
                ProtoSerialization.Deserialize<PBPointerEventsResult>, // FD::
                iecsComponentPool: poolWrapper // FD:: changed
                );

            componentWriter.AddOrReplaceComponentSerializer<PBPointerEventsResult>(componentId, ProtoSerialization.Serialize);

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
