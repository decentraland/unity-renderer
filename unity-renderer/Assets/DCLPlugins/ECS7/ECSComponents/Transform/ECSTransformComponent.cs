using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class ECSTransformComponent : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public ECSTransformComponent(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            ECSTransformHandler handler = new ECSTransformHandler();
            factory.AddOrReplaceComponent(componentId, ECSTransformSerialization.Deserialize, () => handler);
            componentWriter.AddOrReplaceComponentSerializer<ECSTransform>(componentId, ECSTransformSerialization.Serialize);

            this.factory = factory;
            this.componentId = componentId;
        }

        public void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
        }
    }
}