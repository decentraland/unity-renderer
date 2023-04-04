using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class UIBackgroundRegister : UIElementRegisterBase<PBUiBackground, UIBackgroundHandler>
    {
        public UIBackgroundRegister(
            int componentId,
            ECSComponentsFactory factory,
            IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalUiContainer> internalUiContainer)
            : base(
                componentId,
                factory,
                componentWriter,
                internalUiContainer,
                (container, componentId) =>
                    new UIBackgroundHandler(container, componentId, AssetPromiseKeeper_Texture.i)) { }
    }
}
