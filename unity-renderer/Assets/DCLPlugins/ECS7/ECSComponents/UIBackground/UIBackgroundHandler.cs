using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.UIElements.Image;

namespace DCL.ECSComponents
{
    public class UIBackgroundHandler : UIElementHandlerBase, IECSComponentHandler<PBUiBackground>
    {
        private readonly AssetPromiseKeeper_Texture fontPromiseKeeper;
        private UITextureUpdater textureUpdater;

        internal DCLImage image { get; private set; }

        public UIBackgroundHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer, int componentId, AssetPromiseKeeper_Texture fontPromiseKeeper)
            : base(internalUiContainer, componentId)
        {
            this.fontPromiseKeeper = fontPromiseKeeper;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            image = new DCLImage();
            image.style.flexGrow = 1;

            textureUpdater = new UITextureUpdater(image, fontPromiseKeeper);

            var internalUiContainer = AddComponentToEntity(scene, entity);
            internalUiContainer.rootElement.Insert(0, image);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            RemoveElementFromRoot(scene, entity, image);
            image = null;
            textureUpdater.Dispose();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiBackground model)
        {
            textureUpdater.Update(model.Texture);
            image.Color = model.GetColor().ToUnityColor();
            image.Slices = model.GetBorder().ToUnityBorder();
            image.UVs = model.Uvs.ToDCLUVs();
            image.ScaleMode = model.TextureMode.ToDCLImageScaleMode();
        }
    }
}
