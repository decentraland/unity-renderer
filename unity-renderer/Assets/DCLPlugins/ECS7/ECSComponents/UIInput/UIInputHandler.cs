using DCL.Controllers;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
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
        // The 'DCL.UIInput.uss' stylesheet loaded in DCLDefaultRuntimeTheme scriptable object uses this class
        private const string USS_CLASS = "dcl-input";

        private UIFontUpdater fontUpdater;
        private readonly int resultComponentId;
        private readonly IInternalECSComponent<InternalUIInputResults> inputResults;
        private readonly AssetPromiseKeeper_Font fontPromiseKeeper;
        private readonly WrappedComponentPool<IWrappedComponent<PBUiInputResult>> componentPool;

        private EventCallback<ChangeEvent<string>> onValueChanged;

        internal TextField uiElement { get; private set; }

        internal TextFieldPlaceholder placeholder { get; private set; }

        public UIInputHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer,
            int resultComponentId,
            IInternalECSComponent<InternalUIInputResults> inputResults,
            AssetPromiseKeeper_Font fontPromiseKeeper, int componentId,
            WrappedComponentPool<IWrappedComponent<PBUiInputResult>> componentPool) : base(internalUiContainer, componentId)
        {
            this.resultComponentId = resultComponentId;
            this.inputResults = inputResults;
            this.fontPromiseKeeper = fontPromiseKeeper;
            this.componentPool = componentPool;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            // `TextField` contains a label as well but
            // passing a null string will actually make it invisible
            uiElement = new TextField();
            uiElement.AddToClassList(USS_CLASS);

            placeholder = new TextFieldPlaceholder(uiElement);

            AddElementToRoot(scene, entity, uiElement);
            fontUpdater = new UIFontUpdater(uiElement, fontPromiseKeeper);

            onValueChanged = UIPointerEventsUtils
               .RegisterFeedback<ChangeEvent<string>>
                (inputResults,
                    CreateInputResult,
                    scene,
                    entity,
                    uiElement,
                    resultComponentId);
        }

        private IPooledWrappedComponent CreateInputResult(ChangeEvent<string> onValueChange)
        {
            var componentPooled = componentPool.Get();
            var componentModel = componentPooled.WrappedComponent.Model;
            componentModel.Value = onValueChange.newValue;
            return componentPooled;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            uiElement.UnregisterFeedback(onValueChanged);
            RemoveElementFromRoot(scene, entity, uiElement);
            uiElement = null;
            fontUpdater.Dispose();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiInput model)
        {
            placeholder.SetPlaceholder(model.Placeholder);
            placeholder.SetPlaceholderColor(model.GetPlaceholderColor());
            placeholder.SetNormalColor(model.GetColor());

            var isReadonly = !model.IsInteractable();

            placeholder.SetReadOnly(isReadonly);
            uiElement.isReadOnly = isReadonly;
            uiElement.style.fontSize = model.GetFontSize();
            uiElement.style.unityTextAlign = model.GetTextAlign().ToUnityTextAlign();

            if (model.HasValue)
                uiElement.SetValueWithoutNotify(model.Value);

            fontUpdater.Update(model.GetFont());
        }
    }
}
