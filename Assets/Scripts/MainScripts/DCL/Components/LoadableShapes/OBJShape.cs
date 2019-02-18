using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class OBJLoader : LoadableMonoBehavior
    {
        DynamicOBJLoaderController objLoaderComponent;

        void Awake()
        {
            objLoaderComponent = Helpers.Utils.GetOrCreateComponent<DynamicOBJLoaderController>(gameObject);
            objLoaderComponent.OnFinishedLoadingAsset += CallOnComponentUpdated;
        }

        public override void Load(string src, bool useVisualFeedback=false)
        {
            if (!string.IsNullOrEmpty(src))
            {
                objLoaderComponent.LoadAsset(src, true);

                if (objLoaderComponent.loadingPlaceholder == null)
                {
                    objLoaderComponent.loadingPlaceholder = Helpers.Utils.AttachPlaceholderRendererGameObject(gameObject.transform);
                }
                else
                {
                    objLoaderComponent.loadingPlaceholder.SetActive(true);
                }
            }
        }

        void CallOnComponentUpdated()
        {
            if (entity.OnComponentUpdated != null)
                entity.OnComponentUpdated.Invoke(this);

            if (entity.OnShapeUpdated != null)
                entity.OnShapeUpdated.Invoke(entity);

            BaseShape.ConfigureColliders(entity, true, true);
        }

        void OnDestroy()
        {
            objLoaderComponent.OnFinishedLoadingAsset -= CallOnComponentUpdated;

            Destroy(objLoaderComponent);
        }
    }


    public class OBJShape : BaseLoadableShape<OBJLoader>
    {
        public override string componentName => "OBJ Shape";

        public OBJShape(ParcelScene scene) : base(scene)
        {
        }
    }
}
