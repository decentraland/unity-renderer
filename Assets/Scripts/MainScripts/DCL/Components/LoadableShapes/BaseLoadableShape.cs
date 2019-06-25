using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public abstract class LoadableMonoBehavior : MonoBehaviour
    {
        public bool useVisualFeedback = true;
        public bool initialVisibility = true;
        public bool alreadyLoaded = false;

        public DecentralandEntity entity;
        public ContentProvider contentProvider;

        public abstract void Load(string url, System.Action<LoadableMonoBehavior> OnSuccess, System.Action<LoadableMonoBehavior> OnFail);
        public abstract void Unload();
    }

    public class BaseLoadableShape<Loadable> : BaseShape
        where Loadable : LoadableMonoBehavior
    {
        [System.Serializable]
        public new class Model : BaseShape.Model
        {
            public string src;
        }

        protected string currentSrc = "";

        public new Model model { get { return base.model as Model; } set { base.model = value; } }

        public BaseLoadableShape(ParcelScene scene) : base(scene)
        {
            OnDetach += DetachShape;
            OnAttach += AttachShape;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            if (model == null)
                model = new Model();

            bool currentVisible = model.visible;
            model = SceneController.i.SafeFromJson<Model>(newJson);

            // TODO: changing src is not allowed in loadableShapes
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
                    var loadable = entity.meshGameObject.GetComponentInChildren<ILoadable>();
                    if (loadable != null)
                    {
                        loadable.InitialVisibility = model.visible;
                    }

                    ConfigureVisibility(entity.meshGameObject, model.visible);
                }
            }

            return null;
        }

        protected virtual void AttachShape(DecentralandEntity entity)
        {
            if (!string.IsNullOrEmpty(currentSrc))
            {
                string finalUrl;

                if (scene.contentProvider.TryGetContentsUrl(currentSrc, out finalUrl))
                {
                    entity.EnsureMeshGameObject(componentName + " mesh");
                    Loadable loadableShape = entity.meshGameObject.GetOrCreateComponent<Loadable>();
                    loadableShape.entity = entity;
                    loadableShape.contentProvider = entity.scene.contentProvider;
                    loadableShape.initialVisibility = model.visible;
                    loadableShape.useVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;
                    loadableShape.Load(finalUrl, OnLoadCompleted, OnLoadFailed);
                }
            }
        }

        protected void OnLoadFailed(LoadableMonoBehavior loadable)
        {
            loadable.gameObject.name += " - Failed loading";

            MaterialTransitionController[] transitionController =
                loadable.GetComponentsInChildren<MaterialTransitionController>(true);

            foreach (MaterialTransitionController material in transitionController)
            {
                Object.Destroy(material);
            }
        }

        protected void OnLoadCompleted(LoadableMonoBehavior loadable)
        {
            DecentralandEntity entity = loadable.entity;

            if (entity.currentShape == null)
            {
                return;
            }

            BaseShape.ConfigureVisibility(loadable.entity.meshGameObject, entity.currentShape.model.visible);

            if (entity.OnComponentUpdated != null)
            {
                entity.OnComponentUpdated.Invoke(loadable);
            }

            if (entity.OnShapeUpdated != null)
            {
                entity.OnShapeUpdated.Invoke(entity);
            }
        }

        private void DetachShape(DecentralandEntity entity)
        {
            if (entity == null || entity.meshGameObject == null)
            {
                return;
            }

            Loadable loadableShape = entity.meshGameObject.GetComponent<Loadable>();

            if (loadableShape != null)
            {
                loadableShape.Unload();
            }

            Utils.SafeDestroy(entity.meshGameObject);
            entity.meshGameObject = null;
        }
    }
}
