using DCL.ECSRuntime;
using System;

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
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBGltfContainer>,
                () => new GltfContainerHandler(
                    internalComponents.onPointerColliderComponent,
                    internalComponents.physicColliderComponent,
                    internalComponents.customLayerColliderComponent,
                    internalComponents.renderersComponent,
                    internalComponents.GltfContainerLoadingStateComponent,
                    internalComponents.Animation,
                    DataStore.i.ecs7,
                    DataStore.i.featureFlags,
                    DataStore.i.sceneWorldObjects,
                    DataStore.i.debugConfig));

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
