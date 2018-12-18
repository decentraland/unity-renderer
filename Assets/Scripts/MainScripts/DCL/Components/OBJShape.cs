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
    public override IEnumerator ApplyChanges() {
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

      if (!string.IsNullOrEmpty(data.src)) {
        var objShapeComponent = LandHelpers.GetOrCreateComponent<DynamicOBJLoaderController>(gameObject);
        objShapeComponent.LoadAsset(data.src);

        objShapeComponent.loadingPlaceholder = GLTFShape.AttachPlaceholderRendererGameObject(gameObject.transform);
      }

      return null;
    }
  }
}
