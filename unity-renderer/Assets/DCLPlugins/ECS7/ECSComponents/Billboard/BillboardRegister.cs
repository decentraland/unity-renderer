using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class BillboardRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public BillboardRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, BillboardSerializer.Deserialize, () => new BillboardComponentHandler(DataStore.i.player, Environment.i.platform.updateEventHandler));
            componentWriter.AddOrReplaceComponentSerializer<PBBillboard>(componentId, BillboardSerializer.Serialize);

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