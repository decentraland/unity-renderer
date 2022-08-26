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

        public VisibilityComponentRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, VisibilityComponentSerializer.Deserialize, () => new ECSVisibilityComponentHandler());
            componentWriter.AddOrReplaceComponentSerializer<PBVisibilityComponent>(componentId, VisibilityComponentSerializer.Serialize);

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