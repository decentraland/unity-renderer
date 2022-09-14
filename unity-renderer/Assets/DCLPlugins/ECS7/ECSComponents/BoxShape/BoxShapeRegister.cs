using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class BoxShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public BoxShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalTexturizable> texturizableInternalComponent)
        {
            factory.AddOrReplaceComponent(componentId, BoxShapeSerializer.Deserialize, () => new ECSBoxShapeComponentHandler(DataStore.i.ecs7, texturizableInternalComponent));
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