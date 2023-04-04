using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Models;
using System.Linq;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIDropdown
{
    public class UIDropdownHandler : UIElementHandlerBase, IECSComponentHandler<PBUiDropdown>
    {
        // The 'DCL.UIDropdown.uss' stylesheet loaded in DCLDefaultRuntimeTheme scriptable object uses these classes
        private const string READONLY_CLASS = "dcl-dropdown-readonly";
        private const string CLASS = "dcl-dropdown";
        private const string TEXT_ELEMENT_CLASS = "unity-base-popup-field__text";

        private UIFontUpdater fontUpdater;
        private readonly int resultComponentId;
        private readonly IInternalECSComponent<InternalUIInputResults> inputResults;
        private readonly AssetPromiseKeeper_Font fontPromiseKeeper;

        private EventCallback<ChangeEvent<string>> onValueChanged;

        // In the future iteration we may want to create our own DropdownField to be able to fully customize
        // it according to the user provided styles
        internal DropdownField uiElement { get; private set; }

        internal TextElement textField { get; private set; }

        public UIDropdownHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer,
            int resultComponentId,
            IInternalECSComponent<InternalUIInputResults> inputResults,
            AssetPromiseKeeper_Font fontPromiseKeeper,
            int componentId)
            : base(internalUiContainer, componentId)
        {
            this.resultComponentId = resultComponentId;
            this.inputResults = inputResults;
            this.fontPromiseKeeper = fontPromiseKeeper;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            // `DropdownField` contains a label as well but
            // passing a null string will actually make it invisible
            uiElement = new DropdownField(null);
            textField = uiElement.Q<TextElement>(className: TEXT_ELEMENT_CLASS);
            uiElement.AddToClassList(CLASS);

            AddElementToRoot(scene, entity, uiElement);
            fontUpdater = new UIFontUpdater(uiElement, fontPromiseKeeper);

            // it seems strange but `DropdownField` notifies with `string`, not `int`
            onValueChanged = UIPointerEventsUtils
               .RegisterFeedback<ChangeEvent<string>, PBUiDropdownResult>
                (inputResults,
                    CreateInputResult,
                    scene,
                    entity,
                    uiElement,
                    resultComponentId);
        }

        private PBUiDropdownResult CreateInputResult(ChangeEvent<string> onValueChange) =>
            new() { Value = uiElement.index };

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            uiElement.UnregisterFeedback(onValueChanged);
            RemoveElementFromRoot(scene, entity, uiElement);
            uiElement = null;
            fontUpdater.Dispose();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiDropdown model)
        {
            fontUpdater.Update(model.GetFont());

            uiElement.style.fontSize = model.GetFontSize();
            uiElement.style.color = model.GetColor().ToUnityColor();

            uiElement.choices.Clear();
            uiElement.choices.AddRange(model.Options);

            int selectedIndex = model.GetSelectedIndex();

            // Don't report the value back to the scene
            // there is no `SetIndexWithoutNotify`
            uiElement.SetValueWithoutNotify(uiElement.choices.ElementAtOrDefault(selectedIndex) ?? model.EmptyLabel);

            // it is controlled from the style sheet
            uiElement.EnableInClassList(READONLY_CLASS, model.Disabled);
            // TODO it's not fully correct
            uiElement.pickingMode = model.Disabled ? PickingMode.Ignore : PickingMode.Position;

            ApplyTextElementStyle(model);
        }

        private void ApplyTextElementStyle(PBUiDropdown dropdown)
        {
            textField.style.unityTextAlign = dropdown.GetTextAlign().ToUnityTextAlign();
        }
    }
}
