using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.UIElements;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIInput
{
    public class UIInputHandler : UIElementHandlerBase, IECSComponentHandler<PBUiInput>
    {
        private FontUpdater fontUpdater;
        private readonly AssetPromiseKeeper_Font fontPromiseKeeper;

        internal TextField uiElement { get; private set; }

        internal TextFieldPlaceholder placeholder { get; private set; }

        public UIInputHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer,
            AssetPromiseKeeper_Font fontPromiseKeeper, int componentId) : base(internalUiContainer, componentId)
        {
            this.fontPromiseKeeper = fontPromiseKeeper;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            // `TextField` contains a label as well but
            // passing a null string will actually make it invisible
            uiElement = new TextField();
            placeholder = new TextFieldPlaceholder(uiElement);

            AddElementToRoot(scene, entity, uiElement);
            fontUpdater = new FontUpdater(uiElement, fontPromiseKeeper);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            RemoveElementFromRoot(scene, entity, uiElement);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiInput model)
        {
            placeholder.SetPlaceholder(model.Placeholder);
            placeholder.SetPlaceholderColor(model.GetPlaceholderColor());
            placeholder.SetNormalColor(model.GetColor());

            uiElement.isReadOnly = model.IsInteractable();
            uiElement.style.fontSize = model.GetFontSize();

            fontUpdater.Update(model.GetFont());
        }
    }
}
