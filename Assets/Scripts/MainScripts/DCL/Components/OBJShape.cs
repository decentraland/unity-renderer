using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components {
  [Serializable]
  public class OBJShapeModel {
    public string src;
  }

  public class OBJShape : BaseShape<OBJShapeModel> {
    DynamicOBJLoaderController objLoaderComponent;

    protected void Awake() {
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

    public override IEnumerator ApplyChanges() {
      if (!string.IsNullOrEmpty(data.src)) {
        objLoaderComponent.LoadAsset(data.src, true);

        if (objLoaderComponent.loadingPlaceholder == null) {
          objLoaderComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(gameObject.transform);
        } else {
          objLoaderComponent.loadingPlaceholder.SetActive(true);
        }
      }

      return null;
    }

    public override IEnumerator UpdateComponent() {
      yield return ApplyChanges();
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
