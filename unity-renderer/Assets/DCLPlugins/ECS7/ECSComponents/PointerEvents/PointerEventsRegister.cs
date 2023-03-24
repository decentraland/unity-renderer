using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using System;

namespace DCL.ECSComponents
{
    public class PointerEventsRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public PointerEventsRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalPointerEvents> internalPointerEvents)
        {
            var handler = new PointerEventsHandler(internalPointerEvents);

            factory.AddOrReplaceComponent(componentId,
                ProtoSerialization.Deserialize<PBPointerEvents>,
                () => handler);

            componentWriter.AddOrReplaceComponentSerializer<PBPointerEvents>(componentId, ProtoSerialization.Serialize);

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
