using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class LoadableShape : BaseShape
    {
        [System.Serializable]
        public new class Model : BaseShape.Model
        {
            public string src;
        }

        public Model model = new Model();

        public LoadableShape(ParcelScene scene) : base(scene)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            return null;
        }
    }

    public class LoadableShape<LoadWrapperType> : LoadableShape
        where LoadWrapperType : LoadWrapper
    {
        protected string currentSrc = "";

        public LoadableShape(ParcelScene scene) : base(scene)
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
                    var loadable = entity.meshGameObject.GetComponentInChildren<LoadWrapper>();

                    if (loadable != null)
                    {
                        loadable.initialVisibility = model.visible;
                    }

                    ConfigureVisibility(entity.meshGameObject, model.visible);
                }
            }

            return null;
        }

        protected virtual void AttachShape(DecentralandEntity entity)
        {
            if (scene.contentProvider.HasContentsUrl(currentSrc))
            {
                entity.EnsureMeshGameObject(componentName + " mesh");
                LoadWrapperType loadableShape = entity.meshGameObject.GetOrCreateComponent<LoadWrapperType>();
                loadableShape.entity = entity;
                loadableShape.useVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;
                loadableShape.initialVisibility = model.visible;
                loadableShape.contentProvider = scene.contentProvider;
                loadableShape.Load(currentSrc, OnLoadCompleted, OnLoadFailed);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"LoadableShape '{currentSrc}' not found in scene '{scene.sceneData.id}' mappings");
#endif
            }
        }

        protected void OnLoadFailed(LoadWrapper loadWrapper)
        {
            loadWrapper.gameObject.name += " - Failed loading";

            MaterialTransitionController[] transitionController =
                loadWrapper.GetComponentsInChildren<MaterialTransitionController>(true);

            for (int i = 0; i < transitionController.Length; i++)
            {
                MaterialTransitionController material = transitionController[i];
                Object.Destroy(material);
            }
        }

        protected void OnLoadCompleted(LoadWrapper loadWrapper)
        {
            DecentralandEntity entity = loadWrapper.entity;

            if (entity.currentShape == null)
            {
                return;
            }

            BaseShape.ConfigureVisibility(loadWrapper.entity.meshGameObject, (entity.currentShape as LoadableShape).model.visible);

            entity.OnComponentUpdated?.Invoke(loadWrapper);
            entity.OnShapeUpdated?.Invoke(entity);
        }

        private void DetachShape(DecentralandEntity entity)
        {
            if (entity == null || entity.meshGameObject == null)
            {
                return;
            }

            LoadWrapper loadWrapper = entity.meshGameObject.GetComponent<LoadWrapper>();

            loadWrapper?.Unload();

            Utils.SafeDestroy(entity.meshGameObject);
            entity.meshGameObject = null;
        }
    }
}
