using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;

public class VisualTestController : MonoBehaviour
{
    public static VisualTestController i { get; private set; }
    public new Camera camera;
    public bool takeSnapshots = false;
    public bool snapshotsAreBaseline = false;
    public bool instantiateSceneObjects = true;

    private void Awake()
    {
        i = this;
    }

    IEnumerator Start()
    {
        if (instantiateSceneObjects)
        {
            yield return InstantiateTestedObjects();
        }

        if (takeSnapshots)
        {
            if (snapshotsAreBaseline)
            {
                yield return VisualTestHelpers.GenerateBaselineForTest(TakeSnapshots());
            }
            else
            {
                yield return TakeSnapshots();
            }
        }
    }

    IEnumerator InstantiateTestedObjects()
    {
        var sceneController = FindObjectOfType<Main>().sceneController;
        var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(scenesToLoad);

        yield return null;

        var scene = Environment.i.worldState.loadedScenes["0,0"];
        string textureUrl = DCL.Helpers.Utils.GetTestsAssetsPath() + "/Images/atlas.png";

        TestHelpers.InstantiateEntityWithMaterial(scene, "1", new Vector3(-3, 1, 3),
            new DCL.Components.BasicMaterial.Model
            {
                texture = textureUrl,
            }, "testBasicMaterial");

        TestHelpers.InstantiateEntityWithMaterial(scene, "2", new Vector3(0, 1, 3), new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = textureUrl,
            metallic = 0,
            roughness = 1,
        }, "testMaterial1");

        string materialID = "testMaterial2";
        TestHelpers.InstantiateEntityWithMaterial(scene, "3", new Vector3(3, 1, 3), new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = textureUrl,
            metallic = 1,
            roughness = 1,
            alphaTexture = textureUrl,
        }, materialID);

        // Re-assign last PBR material to new entity
        TestHelpers.InstantiateEntityWithShape(scene, "4", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(6, 1, 3));

        string entityId = "4";

        scene.SharedComponentAttach(
            entityId,
            materialID
        );

        Color color = new Color(1, 0.7f, 0.7f);

        // Update material attached to 2 entities, adding albedoColor
        scene.SharedComponentUpdate(materialID, JsonUtility.ToJson(new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = textureUrl,
            metallic = 1,
            roughness = 1,
            alphaTexture = textureUrl,
            albedoColor = color
        }));

        TestHelpers.InstantiateEntityWithShape(scene, "5", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(-6, 1, 6));
        TestHelpers.InstantiateEntityWithShape(scene, "6", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(-3, 1, 6));
        TestHelpers.InstantiateEntityWithShape(scene, "7", DCL.Models.CLASS_ID.PLANE_SHAPE, new Vector3(0, 1, 6));
        TestHelpers.InstantiateEntityWithShape(scene, "8", DCL.Models.CLASS_ID.CONE_SHAPE, new Vector3(3, 1, 6));
        TestHelpers.InstantiateEntityWithShape(scene, "9", DCL.Models.CLASS_ID.CYLINDER_SHAPE, new Vector3(6, 1, 6));
    }

    IEnumerator TakeSnapshots()
    {
        yield return VisualTestHelpers.TakeSnapshot("snapshot_1.png", new Vector3(10f, 10f, 0f), Vector3.zero);
        yield return VisualTestHelpers.TakeSnapshot("snapshot_2.png", new Vector3(0f, 10f, 0f), Vector3.zero);
        yield return VisualTestHelpers.TakeSnapshot("snapshot_3.png", new Vector3(-10f, 10f, 0f), Vector3.zero);
        yield return VisualTestHelpers.TakeSnapshot("snapshot_4.png", new Vector3(0f, 10f, -10f), Vector3.zero);
    }
}