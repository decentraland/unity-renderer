using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCLPlugins.ECSComponents
{
    public class UiCanvasInformationRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;

        public UiCanvasInformationRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBUiCanvasInformation>(
                new WrappedComponentPool<IWrappedComponent<PBUiCanvasInformation>>(10,
                    () => new ProtobufWrappedComponent<PBUiCanvasInformation>(new PBUiCanvasInformation()))
            );

            factory.AddOrReplaceComponent(
                componentId,
                null,
                // ProtoSerialization.Deserialize<PBUiCanvasInformation>, // FD::
                iecsComponentPool: poolWrapper // FD:: changed
                );

            componentWriter.AddOrReplaceComponentSerializer<PBUiCanvasInformation>(componentId, ProtoSerialization.Serialize);

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
