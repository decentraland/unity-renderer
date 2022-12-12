using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using Google.Protobuf;
using System;

namespace DCL.ECSComponents.UIAbstractElements
{
    public abstract class UIElementRegisterBase<T, THandler> : IDisposable
        where T : IMessage<T>, new()
        where THandler : class, IECSComponentHandler<T>
    {
        protected delegate THandler HandlerBuilder(IInternalECSComponent<InternalUiContainer> container, int componentId);

        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        protected UIElementRegisterBase(int componentId, ECSComponentsFactory factory,
            IECSComponentWriter componentWriter, IInternalECSComponent<InternalUiContainer> internalUiContainer,
            HandlerBuilder handlerBuilder)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<T>,
                () => handlerBuilder(internalUiContainer, componentId));
            componentWriter.AddOrReplaceComponentSerializer<PBUiBackground>(componentId, ProtoSerialization.Serialize);

            this.factory = factory;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        protected virtual void DisposeImpl() {}

        public void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
            DisposeImpl();
        }
    }
}
