using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine.UIElements;

namespace DCL.ECSComponents
{
    public class UiTextHandler : IECSComponentHandler<PBUiText>
    {
        private readonly IInternalECSComponent<InternalUiContainer> internalUiContainer;
        private readonly AssetPromiseKeeper_Font fontPromiseKeeper;

        private Label uiElement;
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
            fontPromiseKeeper.Forget(fontPromise);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiText model)
        {
            uiElement.text = model.Value;
            uiElement.style.color = model.GetColor().ToUnityColor();
            uiElement.style.fontSize = model.GetFontSize();

            int fontId = (int)model.GetFont();
            if (lastFontId != fontId)
            {
                lastFontId = fontId;
                var prevPromise = fontPromise;

                fontPromise = new AssetPromise_Font("SansSerif");
                fontPromise.OnSuccessEvent += font =>
                {
                    uiElement.style.unityFont = font.font.sourceFontFile;
                };
                fontPromiseKeeper.Keep(fontPromise);
                fontPromiseKeeper.Forget(prevPromise);
            }
        }
    }
}