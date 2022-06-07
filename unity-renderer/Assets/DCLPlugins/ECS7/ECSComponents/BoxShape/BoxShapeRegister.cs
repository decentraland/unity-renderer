using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class BoxShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public BoxShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            ECSBoxShapeComponentHandler handler = new ECSBoxShapeComponentHandler(DataStore.i.ecs7);
            factory.AddOrReplaceComponent(componentId, BoxShapeSerializer.Deserialize, () => handler);
            componentWriter.AddOrReplaceComponentSerializer<PBBoxShape>(componentId, BoxShapeSerializer.Serialize);

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