using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class ECSTextShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public ECSTextShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBTextShape>(
                new WrappedComponentPool<IWrappedComponent<PBTextShape>>(10,
                    () => new ProtobufWrappedComponent<PBTextShape>(new PBTextShape()))
            );

            factory.AddOrReplaceComponent(componentId,
                () => new ECSTextShapeComponentHandler(AssetPromiseKeeper_Font.i, internalComponents.renderersComponent, internalComponents.sceneBoundsCheckComponent),
                // ProtoSerialization.Deserialize<PBTextShape>, // FD::
                iecsComponentPool: poolWrapper
                );

            componentWriter.AddOrReplaceComponentSerializer<PBTextShape>(componentId, ProtoSerialization.Serialize);

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
