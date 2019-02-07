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

        public abstract void Load(string src);
    }

    public class BaseLoadableShape<LoadableShape> : BaseShape
        where LoadableShape : LoadableMonoBehavior
    {
        [System.Serializable]
        public class Model
        {
            public string src;
        }

        private string currentSrc = "";

        Model model = new Model();

        public BaseLoadableShape(ParcelScene scene) : base(scene)
        {
            OnDetach += DetachShape;
            OnAttach += AttachShape;
        }

        private IEnumerator AttachShapeCoroutine(DecentralandEntity entity)
        {
            AttachShape(entity);


            yield return null;
        }

        private void AttachShape(DecentralandEntity entity)
        {
            if (!string.IsNullOrEmpty(currentSrc))
            {
                string finalUrl;
                if (scene.sceneData.TryGetContentsUrl(currentSrc, out finalUrl))
                {
                    entity.EnsureMeshGameObject(componentName + " mesh");
                    LoadableShape loadableShape = Helpers.Utils.GetOrCreateComponent<LoadableShape>(entity.meshGameObject);
                    loadableShape.entity = entity;
                    loadableShape.Load(finalUrl);
                }
            }
        }

        private void DetachShape(DecentralandEntity entity)
        {
            if (entity.meshGameObject == null)
                return;

            LoadableShape loadableShape = entity.meshGameObject.GetComponent<LoadableShape>();

            if (loadableShape != null)
            {
                Utils.SafeDestroy(loadableShape);
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            Helpers.Utils.SafeFromJsonOverwrite(newJson, model);

            // TODO: changing src is not allowed in loadableShapes

            if (!string.IsNullOrEmpty(model.src) && currentSrc != model.src)
            {
                currentSrc = model.src;

                foreach (var entity in this.attachedEntities)
                {
                    yield return AttachShapeCoroutine(entity);
                }
            }
        }
    }
}

