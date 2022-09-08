using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class PlaneShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public PlaneShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalTexturizable> texturizableInternalComponent)
        {
            factory.AddOrReplaceComponent(componentId, PlaneShapeSerializer.Deserialize, () => new ECSPlaneShapeComponentHandler(DataStore.i.ecs7, texturizableInternalComponent));
            componentWriter.AddOrReplaceComponentSerializer<PBPlaneShape>(componentId, PlaneShapeSerializer.Serialize);

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