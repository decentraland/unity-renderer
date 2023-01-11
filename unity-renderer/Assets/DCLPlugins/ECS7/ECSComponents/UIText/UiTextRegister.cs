using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSRuntime;

namespace DCL.ECSComponents.UIText
{
    public class UiTextRegister : UIElementRegisterBase<PBUiText, UiTextHandler>
    {
        public UiTextRegister(int componentId,
            ECSComponentsFactory factory,
            IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalUiContainer> internalUiContainer)
            : base(componentId,
                factory,
                componentWriter,
                internalUiContainer,
                (container, cId) =>
                    new UiTextHandler(container, AssetPromiseKeeper_Font.i, cId)) { }
    }
}
