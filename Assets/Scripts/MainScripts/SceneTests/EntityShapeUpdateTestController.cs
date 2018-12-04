using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;

public class EntityShapeUpdateTestController : MonoBehaviour {
  void Start() {
    var sceneController = FindObjectOfType<SceneController>();
    var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

    sceneController.UnloadAllScenes();
    sceneController.LoadParcelScenes(scenesToLoad);

    var scene = sceneController.loadedScenes["0,0"];

    ShapeComponentHelpers.InstantiateEntityWithShape(scene, "1", "box", new Vector3(-3, 1, 0));
    ShapeComponentHelpers.InstantiateEntityWithShape(scene, "2", "sphere", new Vector3(0, 1, 0));
    ShapeComponentHelpers.InstantiateEntityWithShape(scene, "3", "plane", new Vector3(2, 1, 0));
    ShapeComponentHelpers.InstantiateEntityWithShape(scene, "4", "cone", new Vector3(4, 1, 0));
    ShapeComponentHelpers.InstantiateEntityWithShape(scene, "5", "cylinder", new Vector3(6, 1, 0));
    ShapeComponentHelpers.InstantiateEntityWithShape(scene, "6", "gltf-model", new Vector3(0, 1, 6), "https://github.com/KhronosGroup/glTF-Sample-Models/blob/master/2.0/Avocado/glTF/Avocado.gltf");
    ShapeComponentHelpers.InstantiateEntityWithShape(scene, "7", "obj-model", new Vector3(10, 1, 0), "http://127.0.0.1:9991/OBJ/teapot.obj");
  }
}
