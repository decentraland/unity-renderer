using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public abstract class LoadableMonoBehavior : MonoBehaviour
    {
        public bool alreadyLoaded = false;
        public DecentralandEntity entity;

        public abstract void Load(string url, bool useVisualFeedback);
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

        public Model model = new Model();

        public BaseLoadableShape(ParcelScene scene) : base(scene)
        {
            OnDetach += DetachShape;
            OnAttach += AttachShape;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            bool currentVisible = model.visible;
            model = Helpers.Utils.SafeFromJson<Model>(newJson);

            // TODO: changing src is not allowed in loadableShapes
            if (!string.IsNullOrEmpty(model.src) && currentSrc != model.src)
            {
                currentSrc = model.src;

                foreach (var entity in this.attachedEntities)
                {
                    yield return AttachShapeCoroutine(entity);
                }
            }

            if (currentVisible != model.visible)
            {
                foreach (var entity in this.attachedEntities)
                {
                    ConfigureVisibility(entity.meshGameObject, model.visible);
                }
            }
        }

        private IEnumerator AttachShapeCoroutine(DecentralandEntity entity)
        {
            AttachShape(entity);
            yield return null;
        }

        protected virtual void AttachShape(DecentralandEntity entity)
        {
            if (!string.IsNullOrEmpty(currentSrc))
            {
                string finalUrl;

                if (scene.TryGetContentsUrl(currentSrc, out finalUrl))
                {
                    entity.EnsureMeshGameObject(componentName + " mesh");
                    Loadable loadableShape = entity.meshGameObject.GetOrCreateComponent<Loadable>();
                    loadableShape.entity = entity;
                    loadableShape.Load(finalUrl, Configuration.ParcelSettings.VISUAL_LOADING_ENABLED);
                }
            }
        }

        private void DetachShape(DecentralandEntity entity)
        {
            if (entity == null || entity.meshGameObject == null)
                return;

            Loadable loadableShape = entity.meshGameObject.GetOrCreateComponent<Loadable>();

            if (loadableShape != null)
                loadableShape.Unload();

            Utils.SafeDestroy(entity.meshGameObject);
            entity.meshGameObject = null;
        }
    }
}

