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
        var gltfShapeComponent = LandHelpers.GetOrCreateComponent<GLTFComponent>(gameObject);
        gltfShapeComponent.Multithreaded = false;
        gltfShapeComponent.LoadAsset(data.src);

        gltfShapeComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(gameObject.transform);
      }
      return null;
    }

    public static GameObject AttachPlaceholderRendererGameObject(UnityEngine.Transform targetTransform) {
      var placeholderRenderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>();
      placeholderRenderer.material = Resources.Load<Material>("Materials/AssetLoading");
      placeholderRenderer.transform.SetParent(targetTransform);
      placeholderRenderer.transform.localPosition = Vector3.zero;
      placeholderRenderer.name = "PlaceholderRenderer";
      return placeholderRenderer.gameObject;
    }
  }
}
