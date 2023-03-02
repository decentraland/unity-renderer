using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSRuntime;

namespace DCL.ECSComponents.UIDropdown
{
    public class UIDropdownRegister : UIElementRegisterBase<PBUiDropdown, UIDropdownHandler, PBUiDropdownResult>
    {
        public UIDropdownRegister(
            int componentId,
            int feedbackResultComponentId,
            ECSComponentsFactory factory,
            IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalUiContainer> internalUiContainer,
            IInternalECSComponent<InternalUIInputResults> uiInputResults)
            : base(componentId, feedbackResultComponentId, factory, componentWriter, internalUiContainer,
                (container, componentId) =>
                    new UIDropdownHandler(
                        container,
                        feedbackResultComponentId,
                        uiInputResults,
                        AssetPromiseKeeper_Font.i,
                        componentId
                    )) { }
    }
}
