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
        public abstract void Load(string src);
        public bool alreadyLoaded = false;
        public DecentralandEntity entity;
    }

    public class BaseLoadableShape<LoadableShape> : BaseShape where LoadableShape : LoadableMonoBehavior
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

        private void AttachShape(DecentralandEntity entity)
        {
            if (!string.IsNullOrEmpty(currentSrc))
            {
                var loadableShape = Helpers.Utils.GetOrCreateComponent<LoadableShape>(entity.meshGameObject);
                loadableShape.entity = entity;
                loadableShape.Load(currentSrc);
            }
        }

        private void DetachShape(DecentralandEntity entity)
        {
            if (entity.meshGameObject)
            {
                var loadableShape = entity.meshGameObject.GetComponent<LoadableShape>();
                loadableShape.enabled = false;

#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(loadableShape);
#else
        UnityEngine.Object.Destroy(loadableShape);
#endif
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
                    AttachShape(entity);
                }
            }

            return null;
        }
    }
}

