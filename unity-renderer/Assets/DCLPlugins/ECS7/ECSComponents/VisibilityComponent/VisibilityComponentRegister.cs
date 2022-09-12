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
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBVisibilityComponent>, () => new ECSVisibilityComponentHandler(internalComponents.visibilityComponent));
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