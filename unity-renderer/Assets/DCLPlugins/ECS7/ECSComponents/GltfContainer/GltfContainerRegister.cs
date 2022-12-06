using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class GltfContainerRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public GltfContainerRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponents internalComponents)
        {
            var dataStoreEcs7 = DataStore.i.ecs7;
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBGltfContainer>,
                () => new GltfContainerHandler(
                    internalComponents.onPointerColliderComponent,
                    internalComponents.physicColliderComponent,
                    internalComponents.renderersComponent,
                    dataStoreEcs7));
            componentWriter.AddOrReplaceComponentSerializer<PBGltfContainer>(componentId, ProtoSerialization.Serialize);

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