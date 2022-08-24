using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class TransformRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public TransformRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            ECSTransformHandler handler = new ECSTransformHandler(Environment.i.world.state, DataStore.i.player.lastTeleportPosition);
            factory.AddOrReplaceComponent(componentId, ECSTransformSerialization.Deserialize, () => handler);
            componentWriter.AddOrReplaceComponentSerializer<ECSTransform>(componentId, ECSTransformSerialization.Serialize);

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