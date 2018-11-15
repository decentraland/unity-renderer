using UnityEngine;

public class EntityShapeUpdateTestController : MonoBehaviour {
  void Awake() {
    SceneController sceneController = FindObjectOfType<SceneController>();

    sceneController.CreateEntity("1");
    sceneController.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityBoxShapeUpdateTest") as TextAsset).text);

    sceneController.CreateEntity("2");
    sceneController.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntitySphereShapeUpdateTest") as TextAsset).text);

    sceneController.CreateEntity("3");
    sceneController.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityPlaneShapeUpdateTest") as TextAsset).text);

    sceneController.CreateEntity("4");
    sceneController.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityConeShapeUpdateTest") as TextAsset).text);

    sceneController.CreateEntity("5");
    sceneController.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityCylinderShapeUpdateTest") as TextAsset).text);

    sceneController.CreateEntity("6");
    sceneController.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityGLTFShapeUpdateTest") as TextAsset).text);

    sceneController.CreateEntity("7");
    sceneController.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityOBJShapeUpdateTest") as TextAsset).text);
  }
}
