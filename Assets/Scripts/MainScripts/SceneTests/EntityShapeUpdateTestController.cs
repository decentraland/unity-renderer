using UnityEngine;

public class EntityShapeUpdateTestController : MonoBehaviour {
  void Awake() {
    var sceneController = FindObjectOfType<SceneController>();
    var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

    sceneController.UnloadAllScenes();
    sceneController.LoadParcelScenes(scenesToLoad);

    var scene = sceneController.loadedScenes["0,0"];

    scene.CreateEntity("1");
    scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityBoxShapeUpdateTest") as TextAsset).text);

    scene.CreateEntity("2");
    scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntitySphereShapeUpdateTest") as TextAsset).text);

    scene.CreateEntity("3");
    scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityPlaneShapeUpdateTest") as TextAsset).text);

    scene.CreateEntity("4");
    scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityConeShapeUpdateTest") as TextAsset).text);

    scene.CreateEntity("5");
    scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityCylinderShapeUpdateTest") as TextAsset).text);

    scene.CreateEntity("6");
    scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityGLTFShapeUpdateTest") as TextAsset).text);

    scene.CreateEntity("7");
    scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityOBJShapeUpdateTest") as TextAsset).text);
  }
}
