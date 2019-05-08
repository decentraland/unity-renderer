using DCL.Helpers;
using DCL.Components;
using DCL.Models;
using DCL.Configuration;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class VisualTestSceneController1 : MonoBehaviour
{
    public bool takeSnapshots = false;
    public bool snapshotsAreBaseline = false;
    public bool instantiateSceneObjects = true;

    IEnumerator Start()
    {
        if (instantiateSceneObjects)
        {
            yield return InstantiateTestedObjects();
        }

        if (takeSnapshots)
        {
            yield return TakeSnapshots();
        }
    }

    IEnumerator InstantiateTestedObjects()
    {
        var sceneController = FindObjectOfType<SceneController>();
        var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(scenesToLoad);

        yield return null;

        var scene = sceneController.loadedScenes["0,0"];
        string textureUrl = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png";

        TestHelpers.InstantiateEntityWithMaterial(scene, "1", new Vector3(-3, 1, 3), new DCL.Components.BasicMaterial.Model
        {
            texture = textureUrl,
        }, "testBasicMaterial");

        TestHelpers.InstantiateEntityWithMaterial(scene, "2", new Vector3(0, 1, 3), new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = textureUrl,
            metallic = 0,
            roughness = 1,
            hasAlpha = true
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

        scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
        {
            entityId = "4",
            id = materialID,
            name = "material"
        }));

        Color color = new Color(1, 0.7f, 0.7f);

        // Update material attached to 2 entities, adding albedoColor
        scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
        {
            id = materialID,
            json = JsonUtility.ToJson(new DCL.Components.PBRMaterial.Model
            {
                albedoTexture = textureUrl,
                metallic = 1,
                roughness = 1,
                alphaTexture = textureUrl,
                albedoColor = color
            })
        }));

        TestHelpers.InstantiateEntityWithShape(scene, "5", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(-6, 1, 6));
        TestHelpers.InstantiateEntityWithShape(scene, "6", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(-3, 1, 6));
        TestHelpers.InstantiateEntityWithShape(scene, "7", DCL.Models.CLASS_ID.PLANE_SHAPE, new Vector3(0, 1, 6));
        TestHelpers.InstantiateEntityWithShape(scene, "8", DCL.Models.CLASS_ID.CONE_SHAPE, new Vector3(3, 1, 6));
        TestHelpers.InstantiateEntityWithShape(scene, "9", DCL.Models.CLASS_ID.CYLINDER_SHAPE, new Vector3(6, 1, 6));
    }

    IEnumerator TakeSnapshots()
    {
        yield return new WaitForSeconds(2f);
        string baselineImagesPath = Application.dataPath + "/../TestResources/VisualTests/Test1/BaselineImages/";
        string currentTestImagesPath = Application.dataPath + "/../TestResources/VisualTests/Test1/CurrentTestImages/";
        string outputPath = snapshotsAreBaseline ? baselineImagesPath : currentTestImagesPath;

        string snapshot1Name = "snapshot_1.png";
        string snapshot2Name = "snapshot_2.png";
        string snapshot3Name = "snapshot_3.png";
        string snapshot4Name = "snapshot_4.png";
        int snapshotsWidth = TestSettings.VISUAL_TESTS_SNAPSHOT_WIDTH;
        int snapshotsHeight = TestSettings.VISUAL_TESTS_SNAPSHOT_HEIGHT;

        // Create Camera
        Camera visualTestsCam = new GameObject("VisualTestsCam").AddComponent<Camera>();
        visualTestsCam.depth = 11;

        yield return new WaitForEndOfFrame();

        // Take 1st snapshot
        VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(10f, 10f, 0f));
        yield return VisualTestHelpers.TakeSnapshot(outputPath, snapshot1Name, visualTestsCam, snapshotsWidth, snapshotsHeight);

        // Take 2nd snapshot
        yield return new WaitForSeconds(0.2f);
        VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(0f, 10f, 10f));
        yield return VisualTestHelpers.TakeSnapshot(outputPath, snapshot2Name, visualTestsCam, snapshotsWidth, snapshotsHeight);

        // Take 3rd snapshot
        yield return new WaitForSeconds(0.2f);
        VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(-10f, 10f, 0f));
        yield return VisualTestHelpers.TakeSnapshot(outputPath, snapshot3Name, visualTestsCam, snapshotsWidth, snapshotsHeight);

        // Take 4th snapshot
        yield return new WaitForSeconds(0.2f);
        VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(0f, 10f, -10f));
        yield return VisualTestHelpers.TakeSnapshot(outputPath, snapshot4Name, visualTestsCam, snapshotsWidth, snapshotsHeight);

        yield return new WaitForSeconds(0.2f);
        Destroy(visualTestsCam.gameObject);

        if (snapshotsAreBaseline) yield break; // We don't need to compare anything if the snapshots taken are baseline images

        // ---------------------------------------------
        // COMPARE IMAGES

        Texture2D baselineSnapshot1 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
        baselineSnapshot1.LoadImage(File.ReadAllBytes(baselineImagesPath + snapshot1Name));

        Texture2D baselineSnapshot2 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
        baselineSnapshot2.LoadImage(File.ReadAllBytes(baselineImagesPath + snapshot2Name));

        Texture2D baselineSnapshot3 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
        baselineSnapshot3.LoadImage(File.ReadAllBytes(baselineImagesPath + snapshot3Name));

        Texture2D baselineSnapshot4 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
        baselineSnapshot4.LoadImage(File.ReadAllBytes(baselineImagesPath + snapshot4Name));

        Texture2D currentSnapshot1 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
        currentSnapshot1.LoadImage(File.ReadAllBytes(currentTestImagesPath + snapshot1Name));

        Texture2D currentSnapshot2 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
        currentSnapshot2.LoadImage(File.ReadAllBytes(currentTestImagesPath + snapshot2Name));

        Texture2D currentSnapshot3 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
        currentSnapshot3.LoadImage(File.ReadAllBytes(currentTestImagesPath + snapshot3Name));

        Texture2D currentSnapshot4 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
        currentSnapshot4.LoadImage(File.ReadAllBytes(currentTestImagesPath + snapshot4Name));

        float imageAffinity = VisualTestHelpers.GetImageAffinityPercentage(baselineSnapshot1, currentSnapshot1, currentTestImagesPath + "snapshot_diff_1.png");
        Debug.Log(snapshot1Name + " affinity: " + imageAffinity + "%");

        imageAffinity = VisualTestHelpers.GetImageAffinityPercentage(baselineSnapshot2, currentSnapshot2, currentTestImagesPath + "snapshot_diff_2.png");
        Debug.Log(snapshot2Name + " affinity: " + imageAffinity + "%");

        imageAffinity = VisualTestHelpers.GetImageAffinityPercentage(baselineSnapshot3, currentSnapshot3, currentTestImagesPath + "snapshot_diff_3.png");
        Debug.Log(snapshot3Name + " affinity: " + imageAffinity + "%");

        imageAffinity = VisualTestHelpers.GetImageAffinityPercentage(baselineSnapshot4, currentSnapshot4, currentTestImagesPath + "snapshot_diff_4.png");
        Debug.Log(snapshot4Name + " affinity: " + imageAffinity + "%");
    }
}
