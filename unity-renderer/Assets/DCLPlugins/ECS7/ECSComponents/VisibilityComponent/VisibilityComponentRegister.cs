using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class VisibilityComponentRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public VisibilityComponentRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBVisibilityComponent>(
                new WrappedComponentPool<IWrappedComponent<PBVisibilityComponent>>(10,
                    () => new ProtobufWrappedComponent<PBVisibilityComponent>(new PBVisibilityComponent()))
            );

            factory.AddOrReplaceComponent(
                componentId,
                () => new ECSVisibilityComponentHandler(internalComponents.visibilityComponent),
                ProtoSerialization.Deserialize<PBVisibilityComponent>,// FD:: changed
                iecsComponentPool: poolWrapper
                );

            componentWriter.AddOrReplaceComponentSerializer<PBVisibilityComponent>(componentId, ProtoSerialization.Serialize);

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
