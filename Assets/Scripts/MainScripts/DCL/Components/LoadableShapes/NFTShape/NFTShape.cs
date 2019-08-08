using DCL.Controllers;
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
        }

        public override string componentName => "NFT Shape";
        LoadWrapper_NFT loadableShape;

        public NFTShape(ParcelScene scene) : base(scene)
        {
            OnEntityShapeUpdated += UpdateBackgroundColor;
        }

        protected override void AttachShape(DecentralandEntity entity)
        {
            if (!string.IsNullOrEmpty(model.src))
            {
                entity.meshGameObject = UnityEngine.Object.Instantiate(Resources.Load("NFTShapeLoader")) as GameObject;
                entity.meshGameObject.name = componentName + " mesh";
                entity.meshGameObject.transform.SetParent(entity.gameObject.transform);
                entity.meshGameObject.transform.localPosition = Vector3.zero;
                entity.meshGameObject.transform.localScale = Vector3.one;
                entity.meshGameObject.transform.localRotation = Quaternion.identity;

                loadableShape = entity.meshGameObject.GetComponent<LoadWrapper_NFT>();
                loadableShape.entity = entity;
                loadableShape.initialVisibility = model.visible;

                loadableShape.loaderController.collider.enabled = model.withCollisions;
                loadableShape.loaderController.backgroundColor = model.color;

                loadableShape.Load(model.src, OnLoadCompleted, OnLoadFailed);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"NFT SHAPE with url '{model.src}' couldn't be loaded.");
#endif
            }
        }

        protected override void ConfigureColliders(DecentralandEntity entity)
        {
            ConfigureColliders(entity.meshGameObject, model.withCollisions);
        }

        void UpdateBackgroundColor(DecentralandEntity entity)
        {
            if(model.color == previousModel.color) return;

            loadableShape = entity.meshGameObject.GetComponent<LoadWrapper_NFT>();
            loadableShape.loaderController.UpdateBackgroundColor(model.color);
        }
    }
}
