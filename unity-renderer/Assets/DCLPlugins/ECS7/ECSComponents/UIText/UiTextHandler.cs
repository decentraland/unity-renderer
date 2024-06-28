using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIText
{
    public class UiTextHandler : UIElementHandlerBase, IECSComponentHandler<PBUiText>
    {
        private readonly AssetPromiseKeeper_Font fontPromiseKeeper;
        private UIFontUpdater fontUpdater;

        internal Label uiElement { get; private set; }

        public UiTextHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer,
            AssetPromiseKeeper_Font fontPromiseKeeper, int componentId) : base(internalUiContainer, componentId)
        {
            this.fontPromiseKeeper = fontPromiseKeeper;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            uiElement = new Label { text = string.Empty };
            fontUpdater = new UIFontUpdater(uiElement, fontPromiseKeeper);
            UiElementUtils.SetElementDefaultStyle(uiElement.style);
            AddElementToRoot(scene, entity, uiElement);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            RemoveElementFromRoot(scene, entity, uiElement);
            uiElement = null;
            fontUpdater.Dispose();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiText model)
        {
            var wrapMode = WhiteSpace.NoWrap; // Default mode is No Wrap enabled

            if (model.HasTextWrap && model.TextWrap == TextWrap.TwWrap)
                wrapMode = WhiteSpace.Normal;

            uiElement.text = model.Value;
            uiElement.style.whiteSpace = wrapMode;
            uiElement.style.color = model.GetColor().ToUnityColor();
            uiElement.style.fontSize = model.GetFontSize();
            uiElement.style.unityTextAlign = model.GetTextAlign().ToUnityTextAlign();
            fontUpdater.Update(model.GetFont());
        }
    }
}
