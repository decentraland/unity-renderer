using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class MeshRendererRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public MeshRendererRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents, SystemsContext systemsContext = null) // FD:: change this optional
        {
            DataStore_ECS7 dataStoreEcs7 = DataStore.i.ecs7;

            // FD:: wrapping
            var meshRendererPoolWrapper = new ECSReferenceTypeIecsComponentPool<PBMeshRenderer>(systemsContext.MeshRendererPool);

            factory.AddOrReplaceComponent(componentId,
                () => new MeshRendererHandler(dataStoreEcs7, internalComponents.texturizableComponent, internalComponents.renderersComponent));
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
