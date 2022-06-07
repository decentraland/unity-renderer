using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class PlaneShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public PlaneShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            ECSPlaneShapeComponentHandler handler = new ECSPlaneShapeComponentHandler();
            factory.AddOrReplaceComponent(componentId, PlaneShapeSerializer.Deserialize, () => handler);
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