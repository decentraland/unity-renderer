using DCL.Helpers;

namespace DCL.Components
{
    public class OBJLoader : LoadableMonoBehavior
    {
        DynamicOBJLoaderController objLoaderComponent;

        void Awake()
        {
            objLoaderComponent = gameObject.GetOrCreateComponent<DynamicOBJLoaderController>();
            objLoaderComponent.OnFinishedLoadingAsset += CallOnComponentUpdated;
        }

        public override void Unload()
        {
            throw new System.NotImplementedException();
        }

        public override void Load(string src, bool useVisualFeedback = false, bool initialVisibility = true)
        {
            if (!string.IsNullOrEmpty(src))
            {
                alreadyLoaded = false;

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
            alreadyLoaded = true;

            if (entity.OnComponentUpdated != null)
            {
                entity.OnComponentUpdated.Invoke(this);
            }

            if (entity.OnShapeUpdated != null)
            {
                entity.OnShapeUpdated.Invoke(entity);
            }

            BaseShape.ConfigureColliders(entity.meshGameObject, true, true);
        }

        void OnDestroy()
        {
            objLoaderComponent.OnFinishedLoadingAsset -= CallOnComponentUpdated;

            Destroy(objLoaderComponent);
        }
    }
}
