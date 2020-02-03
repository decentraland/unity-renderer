using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class LoadWrapper_OBJ : LoadWrapper
    {
        DynamicOBJLoaderController objLoaderComponent;
        System.Action<LoadWrapper> OnSuccessEvent;

        public override void Unload()
        {
            objLoaderComponent.OnFinishedLoadingAsset -= SuccessWrapper;
            Object.Destroy(objLoaderComponent);
            entity.Cleanup();
        }

        public override void Load(string src, System.Action<LoadWrapper> OnSuccess, System.Action<LoadWrapper> OnFail)
        {
            if (string.IsNullOrEmpty(src))
                return;

            if (objLoaderComponent == null)
                objLoaderComponent = entity.meshRootGameObject.GetOrCreateComponent<DynamicOBJLoaderController>();

            objLoaderComponent.OnFinishedLoadingAsset += SuccessWrapper;
            OnSuccessEvent = OnSuccess;

            alreadyLoaded = false;
            objLoaderComponent.LoadAsset(src, true);

            if (objLoaderComponent.loadingPlaceholder == null)
            {
                objLoaderComponent.loadingPlaceholder =
                    Helpers.Utils.AttachPlaceholderRendererGameObject(entity.gameObject.transform);
            }
            else
            {
                objLoaderComponent.loadingPlaceholder.SetActive(true);
            }
        }

        void SuccessWrapper()
        {
            CallOnComponentUpdated();
            OnSuccessEvent?.Invoke(this);
        }

        void CallOnComponentUpdated()
        {
            alreadyLoaded = true;

            if (entity.OnShapeUpdated != null)
            {
                entity.OnShapeUpdated.Invoke(entity);
            }

            CollidersManager.i.ConfigureColliders(entity);
        }

    }
}
