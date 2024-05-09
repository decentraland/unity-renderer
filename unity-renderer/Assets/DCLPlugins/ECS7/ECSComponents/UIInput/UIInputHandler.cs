using DCL.Controllers;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.UIElements;
using UnityEngine;
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
        private EventCallback<NavigationSubmitEvent> onSubmit;

        public TextField uiElement { get; private set; }

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
            uiElement.pickingMode = PickingMode.Position; // force pointer blocking
            fontUpdater = new UIFontUpdater(uiElement, fontPromiseKeeper);

            onValueChanged = UIPointerEventsUtils
               .RegisterFeedback<ChangeEvent<string>>
                (inputResults,
                    CreateOnChangeInputResult,
                    scene,
                    entity,
                    uiElement,
                    resultComponentId);

            // We don't use <KeyDownEvent> because that one is called a lot more and is
            // triggered twice for some reason (probably a unity bug)
            onSubmit = UIPointerEventsUtils
               .RegisterFeedback<NavigationSubmitEvent>
                (inputResults,
                    CreateOnSubmitInputResult,
                    scene,
                    entity,
                    uiElement,
                    resultComponentId);
        }

        private IPooledWrappedComponent CreateOnChangeInputResult(ChangeEvent<string> evt)
        {
            evt.StopPropagation();

            var componentPooled = componentPool.Get();
            var componentModel = componentPooled.WrappedComponent.Model;
            componentModel.Value = uiElement.value;
            componentModel.IsSubmit = false;
            return componentPooled;
        }

        private IPooledWrappedComponent CreateOnSubmitInputResult(NavigationSubmitEvent evt)
        {
            evt.StopPropagation();

            // Space-bar is also detected as a navigation "submit" event
            if (evt.shiftKey || evt.altKey || evt.ctrlKey || evt.commandKey
                || (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter)))
                return null;

            var componentPooled = componentPool.Get();
            var componentModel = componentPooled.WrappedComponent.Model;
            componentModel.Value = uiElement.value;
            componentModel.IsSubmit = true;

            // Clear text field without triggering its onChange event
            uiElement.SetValueWithoutNotify(string.Empty);

            return componentPooled;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            uiElement.UnregisterFeedback(onValueChanged);
            uiElement.UnregisterFeedback(onSubmit);
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
