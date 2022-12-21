using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.ECSComponents
{
    public class UiTextHandler : IECSComponentHandler<PBUiText>
    {
        private readonly IInternalECSComponent<InternalUiContainer> internalUiContainer;
        private readonly AssetPromiseKeeper_Font fontPromiseKeeper;
        private readonly int componentId;

        internal Label uiElement;
        private AssetPromise_Font fontPromise;
        private int lastFontId = -1;

        public UiTextHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer,
            AssetPromiseKeeper_Font fontPromiseKeeper, int componentId)
        {
            this.internalUiContainer = internalUiContainer;
            this.fontPromiseKeeper = fontPromiseKeeper;
            this.componentId = componentId;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            var containerModel = internalUiContainer.GetFor(scene, entity)?.model ?? new InternalUiContainer();

            uiElement = new Label()
            {
                text = string.Empty,
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    width = new Length(100f, LengthUnit.Percent),
                    height = new Length(100f, LengthUnit.Percent)
                }
            };

            UpdateStyleBoundToContainerUiElement(containerModel.rootElement);
            containerModel.OnUiElementStyleUpdated += UpdateStyleBoundToContainerUiElement;

            containerModel.rootElement.Add(uiElement);
            containerModel.components.Add(componentId);

            internalUiContainer.PutFor(scene, entity, containerModel);
        }

        private void UpdateStyleBoundToContainerUiElement(VisualElement element)
        {
            uiElement.style.left = element.style.paddingLeft;
            uiElement.style.right = element.style.paddingRight;
            uiElement.style.top = element.style.paddingTop;
            uiElement.style.bottom = element.style.paddingBottom;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            var containerData = internalUiContainer.GetFor(scene, entity);
            if (containerData != null)
            {
                var containerModel = containerData.model;
                containerModel.OnUiElementStyleUpdated -= UpdateStyleBoundToContainerUiElement;
                containerModel.rootElement.Remove(uiElement);
                containerModel.components.Remove(componentId);
                internalUiContainer.PutFor(scene, entity, containerModel);
            }
            uiElement = null;
            fontPromiseKeeper.Forget(fontPromise);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiText model)
        {
            uiElement.text = model.Value;
            uiElement.style.color = model.GetColor().ToUnityColor();
            uiElement.style.fontSize = model.GetFontSize();
            uiElement.style.unityTextAlign = ToUnityTextAlign(model.GetTextAlign());

            int fontId = (int)model.GetFont();
            if (lastFontId != fontId)
            {
                lastFontId = fontId;
                var prevPromise = fontPromise;

                fontPromise = new AssetPromise_Font(model.GetFont().ToFontName());
                fontPromise.OnSuccessEvent += font =>
                {
                    uiElement.style.unityFont = font.font.sourceFontFile;
                };
                fontPromiseKeeper.Keep(fontPromise);
                fontPromiseKeeper.Forget(prevPromise);
            }
        }

        private static TextAnchor ToUnityTextAlign(TextAlignMode align)
        {
            switch (align)
            {
                case TextAlignMode.TamTopCenter:
                    return TextAnchor.UpperCenter;
                case TextAlignMode.TamTopLeft:
                    return TextAnchor.UpperLeft;
                case TextAlignMode.TamTopRight:
                    return TextAnchor.UpperRight;

                case TextAlignMode.TamBottomCenter:
                    return TextAnchor.LowerCenter;
                case TextAlignMode.TamBottomLeft:
                    return TextAnchor.LowerLeft;
                case TextAlignMode.TamBottomRight:
                    return TextAnchor.LowerRight;

                case TextAlignMode.TamMiddleCenter:
                    return TextAnchor.MiddleCenter;
                case TextAlignMode.TamMiddleLeft:
                    return TextAnchor.MiddleLeft;
                case TextAlignMode.TamMiddleRight:
                    return TextAnchor.MiddleRight;

                default:
                    return TextAnchor.MiddleCenter;
            }
        }

    }
}
