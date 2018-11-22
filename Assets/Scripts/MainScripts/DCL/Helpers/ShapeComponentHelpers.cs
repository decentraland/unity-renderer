
using DCL.Models;
using UnityEngine;
using UnityGLTF;

namespace DCL.Helpers {
  public class ShapeComponentHelpers {
    public static GameObject IntializeDecentralandEntityRenderer(DecentralandEntity currentEntity, DecentralandEntity parsedEntity) {
      GameObject rendererGameObject = null;

      switch (parsedEntity.components.shape.tag) {
        case "box":
          rendererGameObject = new GameObject();
          rendererGameObject.AddComponent<MeshFilter>().mesh = PrimitiveMeshBuilder.BuildCube(1f);
          rendererGameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Default");
          break;
        case "sphere":
          rendererGameObject = new GameObject();
          rendererGameObject.AddComponent<MeshFilter>().mesh = PrimitiveMeshBuilder.BuildSphere(1f);
          rendererGameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Default");
          break;
        case "plane":
          rendererGameObject = new GameObject();
          rendererGameObject.AddComponent<MeshFilter>().mesh = PrimitiveMeshBuilder.BuildPlane(1f);
          rendererGameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Default");
          break;
        case "cone":
          rendererGameObject = new GameObject();
          rendererGameObject.AddComponent<MeshFilter>().mesh = PrimitiveMeshBuilder.BuildCone(50, 0f, 1f, 2f, 0f, true, false);
          rendererGameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Default");
          break;
        case "cylinder":
          rendererGameObject = new GameObject();
          rendererGameObject.AddComponent<MeshFilter>().mesh = PrimitiveMeshBuilder.BuildCylinder(50, 1f, 1f, 2f, 0f, true, false);
          rendererGameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Default");
          break;
        case "gltf-model":
          if (!string.IsNullOrEmpty(parsedEntity.components.shape.src)) {
            rendererGameObject = new GameObject("GLTFRenderer");

            var gltfShapeComponent = rendererGameObject.AddComponent<GLTFComponent>();
            gltfShapeComponent.Multithreaded = false;
            gltfShapeComponent.LoadAsset(parsedEntity.components.shape.src);

            gltfShapeComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(rendererGameObject.transform);
          }
          break;
        case "obj-model":
          if (!string.IsNullOrEmpty(parsedEntity.components.shape.src)) {
            rendererGameObject = new GameObject("OBJRenderer");

            var objShapeComponent = rendererGameObject.AddComponent<DynamicOBJLoaderController>();
            objShapeComponent.LoadAsset(parsedEntity.components.shape.src);

            objShapeComponent.loadingPlaceholder = AttachPlaceholderRendererGameObject(rendererGameObject.transform);
          }
          break;
      }

      if (rendererGameObject != null) {
        rendererGameObject.transform.SetParent(currentEntity.gameObjectReference.transform);
      }

      return rendererGameObject;
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
