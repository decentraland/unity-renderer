using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSRuntime;

namespace DCL.ECSComponents.UIInput
{
    public class UIInputRegister : UIElementRegisterBase<PBUiInput, UIInputHandler>
    {
        public UIInputRegister(int componentId,
            ECSComponentsFactory factory,
            IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalUiContainer> internalUiContainer)
            : base(
                componentId,
                factory,
                componentWriter,
                internalUiContainer,
                (container, id) => new UIInputHandler(container, AssetPromiseKeeper_Font.i, id)) { }
    }
}
