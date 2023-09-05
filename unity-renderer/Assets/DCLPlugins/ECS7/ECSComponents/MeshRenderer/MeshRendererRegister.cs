using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class MeshRendererRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public MeshRendererRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            DataStore_ECS7 dataStoreEcs7 = DataStore.i.ecs7;
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBMeshRenderer>(
                new WrappedComponentPool<IWrappedComponent<PBMeshRenderer>>(10,
                    () => new ProtobufWrappedComponent<PBMeshRenderer>(new PBMeshRenderer()))
                );

            factory.AddOrReplaceComponent(componentId,
                () => new MeshRendererHandler(dataStoreEcs7, internalComponents.texturizableComponent, internalComponents.renderersComponent),
                ProtoSerialization.Deserialize<PBMeshRenderer>, // FD::
                iecsComponentPool: poolWrapper);
            componentWriter.AddOrReplaceComponentSerializer<PBMeshRenderer>(componentId, ProtoSerialization.Serialize);

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
