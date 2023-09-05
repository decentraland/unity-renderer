using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AvatarAttachRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AvatarAttachRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBAvatarAttach>(
                new WrappedComponentPool<IWrappedComponent<PBAvatarAttach>>(10,
                    () => new ProtobufWrappedComponent<PBAvatarAttach>(new PBAvatarAttach()))
            );

            factory.AddOrReplaceComponent(
                componentId,
                () => new AvatarAttachComponentHandler(Environment.i.platform.updateEventHandler, internalComponents.sceneBoundsCheckComponent),
                AvatarAttachSerializer.Deserialize, // FD::
                iecsComponentPool: poolWrapper
                );

            componentWriter.AddOrReplaceComponentSerializer<PBAvatarAttach>(componentId, AvatarAttachSerializer.Serialize);

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
