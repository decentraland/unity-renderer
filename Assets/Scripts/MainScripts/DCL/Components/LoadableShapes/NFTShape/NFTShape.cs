using DCL.Controllers;
using DCL.Models;
using UnityEngine;
using System.Collections;

namespace DCL.Components
{
    public class NFTShape : LoadableShape<LoadWrapper_NFT>
    {
        [System.Serializable]
        public new class Model : LoadableShape.Model
        {
            public Color color = new Color(0.6404918f, 0.611472f, 0.8584906f); // "light purple" default, same as in explorer
        }
        public new Model model = new Model();

        public override string componentName => "NFT Shape";
        LoadWrapper_NFT loadableShape;

        public NFTShape(ParcelScene scene) : base(scene)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            if (model == null)
                model = new Model();

            bool currentVisible = model.visible;
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (!string.IsNullOrEmpty(model.src) && currentSrc != model.src)
            {
                currentSrc = model.src;

                foreach (var entity in this.attachedEntities)
                {
                    AttachShape(entity);
                }
            }

            if (currentVisible != model.visible)
            {
                foreach (var entity in this.attachedEntities)
                {
                    var loadable = entity.meshGameObject.GetComponentInChildren<LoadWrapper>();

                    if (loadable != null)
                    {
                        loadable.initialVisibility = model.visible;
                    }

                    ConfigureVisibility(entity.meshGameObject, model.visible);
                }
            }

            loadableShape?.loaderController?.UpdateBackgroundColor(model.color);

            return null;
        }

        protected override void AttachShape(DecentralandEntity entity)
        {
            if (!string.IsNullOrEmpty(currentSrc))
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

                loadableShape.Load(currentSrc, OnLoadCompleted, OnLoadFailed);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"NFT SHAPE with url '{currentSrc}' couldn't be loaded.");
#endif
            }
        }
    }
}
