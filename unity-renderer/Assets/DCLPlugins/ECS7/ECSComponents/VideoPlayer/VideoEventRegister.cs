using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCLPlugins.ECSComponents
{
    public class VideoEventRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;

        public VideoEventRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBVideoEvent>(
                new WrappedComponentPool<IWrappedComponent<PBVideoEvent>>(10,
                    () => new ProtobufWrappedComponent<PBVideoEvent>(new PBVideoEvent()))
            );

            factory.AddOrReplaceComponent(
                componentId,
                null,
                ProtoSerialization.Deserialize<PBVideoEvent>, // FD::
                iecsComponentPool: poolWrapper // FD:: changed
                );

            componentWriter.AddOrReplaceComponentSerializer<PBVideoEvent>(componentId, ProtoSerialization.Serialize);

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
