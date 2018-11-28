
using DCL.Models;
using UnityEngine;
using UnityGLTF;

namespace DCL.Helpers {
  public class ShapeComponentHelpers {

    public static void IntializeDecentralandEntityRenderer(DecentralandEntity currentEntity, DecentralandEntity.EntityShape newShape) {
      if (!newShape.Equals(currentEntity.components.shape)) {
        var material = Resources.Load<Material>("Materials/Default");

        MeshFilter meshFilter = null;
        MeshRenderer meshRenderer = null;

        switch (newShape.tag) {
          case "box":
          case "sphere":
          case "plane":
          case "cone":
          case "cylinder":
            meshFilter = currentEntity.gameObject.GetComponent<MeshFilter>();
            if (!meshFilter) {
              meshFilter = currentEntity.gameObject.AddComponent<MeshFilter>();
            }

            meshRenderer = currentEntity.gameObject.GetComponent<MeshRenderer>();
            if (!meshRenderer) {
              meshRenderer = currentEntity.gameObject.AddComponent<MeshRenderer>();
            }
            break;
          default:
            // stub
            break;
        }

        if (meshFilter.mesh) {
          Object.Destroy(meshFilter.mesh);
        }

        switch (newShape.tag) {
          case "box":
            meshFilter.mesh = PrimitiveMeshBuilder.BuildCube(1f);
            meshRenderer.material = material;
            break;
          case "sphere":
            meshFilter.mesh = PrimitiveMeshBuilder.BuildSphere(1f);
            meshRenderer.material = material;
            break;
          case "plane":
            meshFilter.mesh = PrimitiveMeshBuilder.BuildPlane(1f);
            meshRenderer.material = material;
            break;
          case "cone":
            meshFilter.mesh = PrimitiveMeshBuilder.BuildCone(50, 0f, 1f, 2f, 0f, true, false);
            meshRenderer.material = material;
            break;
          case "cylinder":
            meshFilter.mesh = PrimitiveMeshBuilder.BuildCylinder(50, 1f, 1f, 2f, 0f, true, false);
            meshRenderer.material = material;
            break;
          case "gltf-model":
            if (!string.IsNullOrEmpty(newShape.src)) {
              if (meshFilter) {
                Object.Destroy(meshFilter);
              }
              if (meshRenderer) {
                Object.Destroy(meshRenderer);
              }
              var gltfShapeComponent = currentEntity.gameObject.AddComponent<GLTFComponent>();
              gltfShapeComponent.Multithreaded = false;
              gltfShapeComponent.LoadAsset(newShape.src);

              gltfShapeComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(currentEntity.gameObject.transform);
            }
            break;
          case "obj-model":
            if (!string.IsNullOrEmpty(newShape.src)) {
              var objShapeComponent = currentEntity.gameObject.AddComponent<DynamicOBJLoaderController>();
              objShapeComponent.LoadAsset(newShape.src);

              objShapeComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(currentEntity.gameObject.transform);
            }
            break;
        }

        currentEntity.components.shape = newShape;
      }
    }

    static GameObject AttachPlaceholderRendererGameObject(Transform targetTransform) {
      var placeholderRenderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>();
      placeholderRenderer.material = Resources.Load<Material>("Materials/AssetLoading");
      placeholderRenderer.transform.SetParent(targetTransform);
      placeholderRenderer.transform.localPosition = Vector3.zero;
      placeholderRenderer.name = "PlaceholderRenderer";
      return placeholderRenderer.gameObject;
    }
  }
}
