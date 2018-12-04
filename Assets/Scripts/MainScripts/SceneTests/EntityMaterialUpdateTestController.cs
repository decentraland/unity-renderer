using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;

public class EntityMaterialUpdateTestController : MonoBehaviour {
  void Start() {
    var sceneController = FindObjectOfType<SceneController>();
    var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

    sceneController.UnloadAllScenes();
    sceneController.LoadParcelScenes(scenesToLoad);

    var scene = sceneController.loadedScenes["0,0"];

    MaterialComponentHelpers.InstantiateEntityWithMaterial(scene, "1", new Vector3(0, 1, 0), JsonConvert.SerializeObject(new {
      entityId = "1",
      name = "material",
      json = JsonConvert.SerializeObject(new {
        tag = "basic-material",
        texture = "http://127.0.0.1:9991/Images/atlas.png",
        alphaTest = 0.5f,
        samplingMode = 0,
        wrap = 0
      })
    }));

    MaterialComponentHelpers.InstantiateEntityWithMaterial(scene, "2", new Vector3(3, 1, 0), JsonConvert.SerializeObject(new {
      entityId = "2",
      name = "material",
      json = JsonConvert.SerializeObject(new {
        tag = "material",
        albedoTexture = "http://127.0.0.1:9991/Images/avatar.png",
        metallic = 0,
        roughness = 1,
        hasAlpha = true
      })
    }));

    MaterialComponentHelpers.InstantiateEntityWithMaterial(scene, "3", new Vector3(5, 1, 0), JsonConvert.SerializeObject(new {
      entityId = "3",
      name = "material",
      json = JsonConvert.SerializeObject(new {
        tag = "material",
        albedoTexture = "http://127.0.0.1:9991/Images/avatar.png",
        metallic = 1,
        roughness = 1,
        alphaTexture = "http://127.0.0.1:9991/Images/avatar.png",
        albedoColor = "#FF9292"
      })
    }));
  }
}
