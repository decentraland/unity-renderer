using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{


  public class OBJShape : BaseShape {

    [System.Serializable]
    public class Model
    {
      public string src;
    }

    Model model = new Model();
    DynamicOBJLoaderController objLoaderComponent;

    protected new void Awake() {
      base.Awake();

      if (meshFilter) {
#if UNITY_EDITOR
        DestroyImmediate(meshFilter);
#else
        Destroy(meshFilter);
#endif
      }

      if (meshRenderer) {
#if UNITY_EDITOR
        DestroyImmediate(meshRenderer);
#else
        Destroy(meshRenderer);
#endif
      }

      objLoaderComponent = LandHelpers.GetOrCreateComponent<DynamicOBJLoaderController>(meshGameObject);
      objLoaderComponent.OnFinishedLoadingAsset += CallOnComponentUpdated;
    }

    public override IEnumerator ApplyChanges(string newJson) {
      JsonUtility.FromJsonOverwrite(newJson, model);
      if (!string.IsNullOrEmpty(model.src)) {
        objLoaderComponent.LoadAsset(model.src, true);

        if (objLoaderComponent.loadingPlaceholder == null) {
          objLoaderComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(gameObject.transform);
        } else {
          objLoaderComponent.loadingPlaceholder.SetActive(true);
        }
      }

      return null;
    }

    public override IEnumerator UpdateComponent(string newJson) {
      yield return ApplyChanges(newJson);
    }

    void CallOnComponentUpdated() {
      if (entity.OnComponentUpdated != null)
        entity.OnComponentUpdated.Invoke(this);
    }

    protected override void OnDestroy() {
      objLoaderComponent.OnFinishedLoadingAsset -= CallOnComponentUpdated;

      base.OnDestroy();

      Destroy(objLoaderComponent);
    }
  }
}
