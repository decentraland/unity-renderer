using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSRuntime;

namespace DCL.ECSComponents.UIInput
{
    public class UIInputRegister : UIElementRegisterBase<PBUiInput, UIInputHandler, PBUiInputResult>
    {
        public UIInputRegister(int componentId,
            int feedbackResultComponentId,
            ECSComponentsFactory factory,
            IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalUiContainer> internalUiContainer,
            IInternalECSComponent<InternalUIInputResults> inputResults)
            : base(
                componentId,
                feedbackResultComponentId,
                factory,
                componentWriter,
                internalUiContainer,
                (container, id) =>
                    new UIInputHandler(
                        container,
                        feedbackResultComponentId,
                        inputResults,
                        AssetPromiseKeeper_Font.i,
                        id,
                        new WrappedComponentPool<IWrappedComponent<PBUiInputResult>>(
                            5,
                            () => new ProtobufWrappedComponent<PBUiInputResult>(new PBUiInputResult())))) { }
    }
}
