using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class CylinderShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public CylinderShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalTexturizable> texturizableInternalComponent)
        {
            factory.AddOrReplaceComponent(componentId, CylinderShapeSerializer.Deserialize, () => new ECSCylinderShapeComponentHandler(DataStore.i.ecs7, texturizableInternalComponent));
            componentWriter.AddOrReplaceComponentSerializer<PBCylinderShape>(componentId, CylinderShapeSerializer.Serialize);

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