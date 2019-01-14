using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityGLTF;

namespace DCL.Components {

  public class GLTFShape : BaseShape {

    [System.Serializable]
    public class Model
    {
      public string src;
    }

    Model model = new Model();
    GLTFComponent gltfLoaderComponent;
    public bool alreadyLoaded { get; private set; }

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
    }

    public override IEnumerator ApplyChanges(string newJson) {

      LandHelpers.SafeFromJsonOverwrite(newJson, model);

      if (!string.IsNullOrEmpty(model.src)) {

        // GLTF Loader can't be reused "out of the box", so we re-instantiate it when needed
        if (gltfLoaderComponent != null) {
          Destroy(gltfLoaderComponent);
        }

        alreadyLoaded = false;
        gltfLoaderComponent = meshGameObject.AddComponent<GLTFComponent>();
        gltfLoaderComponent.OnFinishedLoadingAsset += CallOnComponentUpdatedEvent;

        gltfLoaderComponent.Multithreaded = false;
        gltfLoaderComponent.LoadAsset(model.src, true);

        if (gltfLoaderComponent.loadingPlaceholder == null) {
          gltfLoaderComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(gameObject.transform);
        } else {
          gltfLoaderComponent.loadingPlaceholder.SetActive(true);
        }
      }

      return null;
    }

    public override IEnumerator UpdateComponent(string newJson) {
      yield return ApplyChanges(newJson);
    }

    void CallOnComponentUpdatedEvent() {
      alreadyLoaded = true;
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
