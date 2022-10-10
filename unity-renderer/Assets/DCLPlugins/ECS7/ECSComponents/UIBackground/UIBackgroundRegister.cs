using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class UIBackgroundRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public UIBackgroundRegister(int componentId, ECSComponentsFactory factory,
            IECSComponentWriter componentWriter, IInternalECSComponent<InternalUiContainer> internalUiContainer)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBUiBackground>,
                () => new UIBackgroundHandler(internalUiContainer, componentId));
            componentWriter.AddOrReplaceComponentSerializer<PBUiBackground>(componentId, ProtoSerialization.Serialize);

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