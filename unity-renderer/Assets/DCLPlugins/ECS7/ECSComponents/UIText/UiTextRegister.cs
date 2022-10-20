using System;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class UiTextRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public UiTextRegister(int componentId, ECSComponentsFactory factory,
            IECSComponentWriter componentWriter, IInternalECSComponent<InternalUiContainer> internalUiContainer)
        {
            AssetPromiseKeeper_Font fontPromiseKeeper = AssetPromiseKeeper_Font.i;

            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBUiText>,
                () => new UiTextHandler(internalUiContainer, fontPromiseKeeper, componentId));

            componentWriter.AddOrReplaceComponentSerializer<PBUiText>(componentId, ProtoSerialization.Serialize);

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