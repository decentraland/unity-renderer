using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class MeshColliderRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public MeshColliderRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents) // FD:: change this optional
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBMeshCollider>(
                new WrappedComponentPool<IWrappedComponent<PBMeshCollider>>(10,
                    () => new ProtobufWrappedComponent<PBMeshCollider>(new PBMeshCollider()))
            );

            factory.AddOrReplaceComponent(componentId,
                () => new MeshColliderHandler(internalComponents.onPointerColliderComponent, internalComponents.physicColliderComponent, internalComponents.customLayerColliderComponent),
                iecsComponentPool: poolWrapper // FD:: changed
                );
            componentWriter.AddOrReplaceComponentSerializer<PBMeshCollider>(componentId, ProtoSerialization.Serialize);

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
