using Castle.Core.Logging;
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
        private EventCallback<KeyDownEvent> onSubmit;

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
            uiElement.pickingMode = PickingMode.Position; // force pointer blocking
            fontUpdater = new UIFontUpdater(uiElement, fontPromiseKeeper);

            onValueChanged = UIPointerEventsUtils
               .RegisterFeedback<ChangeEvent<string>>
                (inputResults,
                    CreateInputResult,
                    scene,
                    entity,
                    uiElement,
                    resultComponentId);

            // Can be optimized using <NavigationSubmitEvent> (a lot less calls) but somehow checking the
            // last keycode read from user input, exiting if it's not KeyCode.Return
            onSubmit = UIPointerEventsUtils
               .RegisterFeedback<KeyDownEvent>
                (inputResults,
                    CreateInputResultOnSubmit,
                    scene,
                    entity,
                    uiElement,
                    resultComponentId);

            /*uiElement.RegisterCallback<KeyDownEvent>(ev =>
            {
                if (ev.keyCode != KeyCode.Return) return;

                Debug.Log($"ENTER KEY EVENT! - submitted string: {uiElement.value}");
            });*/

            /*uiElement.RegisterCallback<NavigationSubmitEvent>(ev =>
            {
                Debug.Log($"NAV SUBMIT EVENT! - submitted string: {uiElement.value}");
            });*/
        }

        private IPooledWrappedComponent CreateInputResult(ChangeEvent<string> evt)
        {
            var componentPooled = componentPool.Get();
            var componentModel = componentPooled.WrappedComponent.Model;
            componentModel.Value = evt.newValue;
            componentModel.IsSubmit = false;
            return componentPooled;
        }

        private IPooledWrappedComponent CreateInputResultOnSubmit(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Return) return null;

            var componentPooled = componentPool.Get();
            var componentModel = componentPooled.WrappedComponent.Model;
            componentModel.Value = uiElement.value;
            componentModel.IsSubmit = true;

            uiElement.value = string.Empty;

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
