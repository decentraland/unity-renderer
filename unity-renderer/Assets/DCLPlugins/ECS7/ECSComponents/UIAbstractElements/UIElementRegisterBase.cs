using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using Google.Protobuf;
using System;

namespace DCL.ECSComponents.UIAbstractElements
{
    /// <summary>
    /// Base class to register deserializer and serializer of UI Component,
    /// and create a builder for handler.
    /// </summary>
    /// <typeparam name="T">Type of component</typeparam>
    /// <typeparam name="THandler">Type of handler</typeparam>
    /// <typeparam name="TFeedbackResult">Type of feedback component (such as 'text value' for TextField)</typeparam>
    public abstract class UIElementRegisterBase<T, THandler, TFeedbackResult> : UIElementRegisterBase<T, THandler>
        where T: IMessage<T>, new()
        where THandler: class, IECSComponentHandler<T>
        where TFeedbackResult : IMessage<TFeedbackResult>, new()
    {
        private readonly int feedbackResultComponentId;

        protected UIElementRegisterBase(int componentId, int feedbackResultComponentId,
            ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalUiContainer> internalUiContainer,
            HandlerBuilder handlerBuilder)
            : base(componentId, factory, componentWriter, internalUiContainer, handlerBuilder)
        {
            this.feedbackResultComponentId = feedbackResultComponentId;

            factory.AddOrReplaceComponent(feedbackResultComponentId, ProtoSerialization.Deserialize<TFeedbackResult>,
                () => null);

            componentWriter.AddOrReplaceComponentSerializer<TFeedbackResult>(feedbackResultComponentId, ProtoSerialization.Serialize);
        }

        public sealed override void Dispose()
        {
            factory.RemoveComponent(feedbackResultComponentId);
            componentWriter.RemoveComponentSerializer(feedbackResultComponentId);

            base.Dispose();
        }
    }

    /// <summary>
    /// Base class to register deserializer and serializer of UI Component,
    /// and create a builder for handler.
    /// </summary>
    /// <typeparam name="T">Type of component</typeparam>
    /// <typeparam name="THandler">Type of handler</typeparam>
    public abstract class UIElementRegisterBase<T, THandler> : IDisposable
        where T: IMessage<T>, new()
        where THandler: class, IECSComponentHandler<T>
    {
        protected internal delegate THandler HandlerBuilder(IInternalECSComponent<InternalUiContainer> container, int componentId);

        protected readonly ECSComponentsFactory factory;
        protected readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        protected UIElementRegisterBase(int componentId, ECSComponentsFactory factory,
            IECSComponentWriter componentWriter, IInternalECSComponent<InternalUiContainer> internalUiContainer,
            HandlerBuilder handlerBuilder)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<T>,
                () => handlerBuilder(internalUiContainer, componentId));

            componentWriter.AddOrReplaceComponentSerializer<T>(componentId, ProtoSerialization.Serialize);

            this.factory = factory;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        protected virtual void DisposeImpl() { }

        public virtual void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
            DisposeImpl();
        }
    }
}
