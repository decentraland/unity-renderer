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

        internal Label uiElement;
        private AssetPromise_Font fontPromise;
        private int lastFontId = -1;

        public UiTextHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer, AssetPromiseKeeper_Font fontPromiseKeeper)
        {
            this.internalUiContainer = internalUiContainer;
            this.fontPromiseKeeper = fontPromiseKeeper;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            uiElement = new Label() { text = string.Empty };

            var containerModel = internalUiContainer.GetFor(scene, entity)?.model ?? new InternalUiContainer();
            containerModel.rootElement.Add(uiElement);

            internalUiContainer.PutFor(scene, entity, containerModel);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            var containerData = internalUiContainer.GetFor(scene, entity);
            if (containerData != null)
            {
                var containerModel = containerData.model;
                containerModel.rootElement.Remove(uiElement);
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

                fontPromise = new AssetPromise_Font(GetFontName(model.GetFont()));
                fontPromise.OnSuccessEvent += font =>
                {
                    uiElement.style.unityFont = font.font.sourceFontFile;
                };
                fontPromiseKeeper.Keep(fontPromise);
                fontPromiseKeeper.Forget(prevPromise);
            }
        }

        private static TextAnchor ToUnityTextAlign(TextAlign align)
        {
            switch (align)
            {
                case TextAlign.Center:
                    return TextAnchor.MiddleCenter;
                case TextAlign.Left:
                    return TextAnchor.MiddleLeft;
                case TextAlign.Right:
                    return TextAnchor.MiddleRight;
                default:
                    return TextAnchor.MiddleCenter;
            }
        }

        private static string GetFontName(Font font)
        {
            // TODO: add support for the rest of the fonts and discuss old font deprecation
            const string SANS_SERIF = "SansSerif";
            const string LIBERATION_SANS = "builtin:LiberationSans SDF";

            switch (font)
            {
                case Font.LiberationSans:
                    return LIBERATION_SANS;
                case Font.SansSerif:
                    return SANS_SERIF;
                default:
                    return SANS_SERIF;
            }
        }
    }
}