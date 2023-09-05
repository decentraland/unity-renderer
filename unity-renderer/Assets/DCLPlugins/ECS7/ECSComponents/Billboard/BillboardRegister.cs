using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
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
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBBillboard>(
                new WrappedComponentPool<IWrappedComponent<PBBillboard>>(10,
                    () => new ProtobufWrappedComponent<PBBillboard>(new PBBillboard()))
            );

            factory.AddOrReplaceComponent(
                componentId,
                null,
                BillboardSerializer.Deserialize, // FD::
                iecsComponentPool: poolWrapper
                );

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
