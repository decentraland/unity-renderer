using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class NFTShape : LoadableShape<LoadWrapper_NFT, NFTShape.Model>
    {
        [System.Serializable]
        public new class Model : LoadableShape.Model
        {
            public Color color = new Color(0.6404918f, 0.611472f, 0.8584906f); // "light purple" default, same as in explorer
            public int style = 0;

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
        }

        public override string componentName => "NFT Shape";

        public NFTShape() { model = new Model(); }

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

            entity.OnShapeUpdated += UpdateBackgroundColor;

            var loadableShape = GetOrAddLoaderForEntity<LoadWrapper_NFT>(entity);

            loadableShape.entity = entity;
            loadableShape.initialVisibility = model.visible;

            loadableShape.withCollisions = model.withCollisions;
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

            var loadableShape = GetLoaderForEntity(entity) as LoadWrapper_NFT;
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