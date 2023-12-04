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
            factory.AddOrReplaceComponent(componentId,
                ProtoSerialization.Deserialize<PBMeshRenderer>,
                () => new MeshRendererHandler(
                    DataStore.i.ecs7,
                    internalComponents.texturizableComponent,
                    internalComponents.renderersComponent,
                    DataStore.i.sceneWorldObjects,
                    DataStore.i.debugConfig));
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
