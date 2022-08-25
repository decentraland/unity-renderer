using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class SphereShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public SphereShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, 
            IInternalECSComponent<InternalTexturizable> texturizableInternalComponent)
        {
            factory.AddOrReplaceComponent(componentId, SphereShapeSerializer.Deserialize, () => new ECSSphereShapeComponentHandler(DataStore.i.ecs7, texturizableInternalComponent));
            componentWriter.AddOrReplaceComponentSerializer<PBSphereShape>(componentId, SphereShapeSerializer.Serialize);

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