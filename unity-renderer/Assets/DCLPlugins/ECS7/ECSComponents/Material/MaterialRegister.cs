using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class MaterialRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public MaterialRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBMaterial>(
                new WrappedComponentPool<IWrappedComponent<PBMaterial>>(10,
                    () => new ProtobufWrappedComponent<PBMaterial>(new PBMaterial()))
            );

            factory.AddOrReplaceComponent(componentId,
                () => new MaterialHandler(internalComponents.materialComponent, internalComponents.videoMaterialComponent),
                // ProtoSerialization.Deserialize<PBMaterial> // FD::
                iecsComponentPool: poolWrapper
                );
            componentWriter.AddOrReplaceComponentSerializer<PBMaterial>(componentId, ProtoSerialization.Serialize);

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
