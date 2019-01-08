using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityGLTF;

namespace DCL.Components {
  [Serializable]
  public class GLTFShapeModel {
    public string src;
  }

  public class GLTFShape : BaseShape<GLTFShapeModel> {
    GLTFComponent gltfLoaderComponent;

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
    }

    public override IEnumerator ApplyChanges() {
      if (!string.IsNullOrEmpty(data.src)) {

        // GLTF Loader can't be reused "out of the box", so we re-instantiate it when needed
        if (gltfLoaderComponent != null) {
          Destroy(gltfLoaderComponent);
        }
        gltfLoaderComponent = meshGameObject.AddComponent<GLTFComponent>();
        gltfLoaderComponent.OnFinishedLoadingAsset += CallOnComponentUpdatedEvent;

        gltfLoaderComponent.Multithreaded = false;
        gltfLoaderComponent.LoadAsset(data.src, true);

        if (gltfLoaderComponent.loadingPlaceholder == null) {
          gltfLoaderComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(gameObject.transform);
        } else {
          gltfLoaderComponent.loadingPlaceholder.SetActive(true);
        }
      }

      return null;
    }

    public override IEnumerator UpdateComponent() {
      yield return ApplyChanges();
    }

    void CallOnComponentUpdatedEvent() {
      if (entity.OnComponentUpdated != null)
        entity.OnComponentUpdated.Invoke(this);
    }

    protected override void OnDestroy() {
      if (gltfLoaderComponent != null) {
        gltfLoaderComponent.OnFinishedLoadingAsset -= CallOnComponentUpdatedEvent;
      }

      base.OnDestroy();

      Destroy(gltfLoaderComponent);
    }
  }
}
