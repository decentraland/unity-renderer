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
            factory.AddOrReplaceComponent(componentId,
                () => new MeshColliderHandler(internalComponents.onPointerColliderComponent, internalComponents.physicColliderComponent, internalComponents.customLayerColliderComponent));
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
