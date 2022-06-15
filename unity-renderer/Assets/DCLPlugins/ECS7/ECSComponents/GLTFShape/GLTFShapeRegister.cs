using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class GLTFShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public GLTFShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, BoxShapeSerializer.Deserialize, () => new ECSBoxShapeComponentHandler(DataStore.i.ecs7));
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