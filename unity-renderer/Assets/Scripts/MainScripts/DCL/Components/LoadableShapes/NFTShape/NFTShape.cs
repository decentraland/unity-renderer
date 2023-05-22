using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;

namespace DCL.Components
{
    public class NFTShape : LoadableShape<LoadWrapper_NFT, NFTShape.Model>
    {
        [System.Serializable]
        public new class Model : LoadableShape.Model
        {
            public Color color = new (0.6404918f, 0.611472f, 0.8584906f); // "light purple" default, same as in explorer
            public int style;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.NftShape)
                    return Utils.SafeUnimplemented<NFTShape, Model>(expected: ComponentBodyPayload.PayloadOneofCase.NftShape, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.NftShape.Color != null) pb.color = pbModel.NftShape.Color.AsUnityColor();
                if (pbModel.NftShape.HasStyle) pb.style = (int)pbModel.NftShape.Style;
                if (pbModel.NftShape.HasSrc) pb.src = pbModel.NftShape.Src;
                if (pbModel.NftShape.HasVisible) pb.visible = pbModel.NftShape.Visible;
                if (pbModel.NftShape.HasWithCollisions) pb.withCollisions = pbModel.NftShape.WithCollisions;
                if (pbModel.NftShape.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.NftShape.IsPointerBlocker;

                return pb;
            }
        }

        public override string componentName => "NFT Shape";

        private INFTInfoRetriever infoRetriever;
        private INFTAssetRetriever assetRetriever;

        public NFTShape(INFTInfoRetriever infoRetriever, INFTAssetRetriever assetRetriever)
        {
            model = new Model();
            this.infoRetriever = infoRetriever;
            this.assetRetriever = assetRetriever;
        }

        public override int GetClassId() { return (int) CLASS_ID.NFT_SHAPE; }

        protected override void AttachShape(IDCLEntity entity)
        {
            if (string.IsNullOrEmpty(model.src))
            {
#if UNITY_EDITOR
                Debug.LogError($"NFT SHAPE with url '{model.src}' couldn't be loaded.");
#endif
                return;
            }

            entity.meshesInfo.meshRootGameObject = NFTShapeFactory.InstantiateLoaderController(model.style);
            entity.meshesInfo.currentShape = this;

            entity.meshRootGameObject.name = componentName + " mesh";
            entity.meshRootGameObject.transform.SetParent(entity.gameObject.transform);
            entity.meshRootGameObject.transform.ResetLocalTRS();

            var loaderController = entity.meshRootGameObject.GetComponent<NFTShapeLoaderController>();

            if (loaderController)
                loaderController.Initialize(infoRetriever, assetRetriever);

            entity.OnShapeUpdated += UpdateBackgroundColor;

            var loadableShape = Environment.i.world.state.GetOrAddLoaderForEntity<LoadWrapper_NFT>(entity);

            loadableShape.entity = entity;

            bool initialVisibility = model.visible;
            if (!DataStore.i.debugConfig.isDebugMode.Get())
                initialVisibility &= entity.isInsideSceneBoundaries;
            loadableShape.initialVisibility = initialVisibility;

            loadableShape.withCollisions = model.withCollisions && entity.isInsideSceneBoundaries;
            loadableShape.backgroundColor = model.color;

            loadableShape.Load(model.src, OnLoadCompleted, OnLoadFailed);
        }

        protected override void DetachShape(IDCLEntity entity)
        {
            if (entity == null || entity.meshRootGameObject == null)
                return;

            entity.OnShapeUpdated -= UpdateBackgroundColor;

            base.DetachShape(entity);
        }

        protected override void ConfigureColliders(IDCLEntity entity) { CollidersManager.i.ConfigureColliders(entity.meshRootGameObject, model.withCollisions, false, entity); }

        void UpdateBackgroundColor(IDCLEntity entity)
        {
            if (previousModel is NFTShape.Model && model.color == previousModel.color)
                return;

            var loadableShape = Environment.i.world.state.GetLoaderForEntity(entity) as LoadWrapper_NFT;
            loadableShape?.loaderController.UpdateBackgroundColor(model.color);
        }

        public override string ToString()
        {
            if (model == null)
                return base.ToString();

            return $"{componentName} (src = {model.src})";
        }
    }
}
