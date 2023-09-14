using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
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
            var dataStoreEcs7 = DataStore.i.ecs7;
            var featureFlags = DataStore.i.featureFlags;

            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBGltfContainer>(
                new WrappedComponentPool<IWrappedComponent<PBGltfContainer>>(10,
                    () => new ProtobufWrappedComponent<PBGltfContainer>(new PBGltfContainer()))
            );

            factory.AddOrReplaceComponent(componentId,
                () => new GltfContainerHandler(
                    internalComponents.onPointerColliderComponent,
                    internalComponents.physicColliderComponent,
                    internalComponents.customLayerColliderComponent,
                    internalComponents.renderersComponent,
                    internalComponents.GltfContainerLoadingStateComponent,
                    dataStoreEcs7,
                    featureFlags),
                // ProtoSerialization.Deserialize<PBGltfContainer>, // FD::
                iecsComponentPool: poolWrapper
                );

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
