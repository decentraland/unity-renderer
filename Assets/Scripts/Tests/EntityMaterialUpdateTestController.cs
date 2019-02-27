using DCL.Helpers;
using UnityEngine;

public class EntityMaterialUpdateTestController : MonoBehaviour
{
    void Start()
    {
        var sceneController = FindObjectOfType<SceneController>();
        var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(scenesToLoad);

        var scene = sceneController.loadedScenes["0,0"];

        TestHelpers.InstantiateEntityWithMaterial(scene, "1", new Vector3(0, 1, 0), new DCL.Components.BasicMaterial.Model
        {
            texture = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
            samplingMode = 0,
            wrap = 0
        }, "testBasicMaterial");

        TestHelpers.InstantiateEntityWithMaterial(scene, "2", new Vector3(3, 1, 0), new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = TestHelpers.GetTestsAssetsPath() + "/Images/avatar.png",
            metallic = 0,
            roughness = 1,
            hasAlpha = true
        }, "testMaterial1");

        string materialID = "testMaterial2";
        TestHelpers.InstantiateEntityWithMaterial(scene, "3", new Vector3(5, 1, 0), new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = TestHelpers.GetTestsAssetsPath() + "/Images/avatar.png",
            metallic = 1,
            roughness = 1,
            alphaTexture = TestHelpers.GetTestsAssetsPath() + "/Images/avatar.png",
        }, materialID);

        // Re-assign last PBR material to new entity
        TestHelpers.InstantiateEntityWithShape(scene, "4", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(5, 1, 2));

        scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
        {
            entityId = "4",
            id = materialID,
            name = "material"
        }));

        // Update material attached to 2 entities, adding albedoColor
        scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
        {
            id = materialID,
            json = JsonUtility.ToJson(new DCL.Components.PBRMaterial.Model
            {
                albedoTexture = TestHelpers.GetTestsAssetsPath() + "/Images/avatar.png",
                metallic = 1,
                roughness = 1,
                alphaTexture = TestHelpers.GetTestsAssetsPath() + "/Images/avatar.png",
                albedoColor = "#FF9292"
            })
        }));
    }
}
