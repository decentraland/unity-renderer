using DCL.Helpers;
using DCL.Components;
using DCL.Models;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public class VisualTestSceneController1 : MonoBehaviour
{
    public bool takeSnapshots = false;
    public bool snapshotsAreBaseline = false;
    public bool instantiateSceneObjects = true;

    IEnumerator Start()
    {
        if (instantiateSceneObjects)
        {
            InstantiateTestedObjects();
        }

        if (takeSnapshots)
        {
            yield return TakeSnapshots();
        }
    }

    void InstantiateTestedObjects()
    {
        var sceneController = FindObjectOfType<SceneController>();
        var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(scenesToLoad);

        var scene = sceneController.loadedScenes["0,0"];

        TestHelpers.InstantiateEntityWithMaterial(scene, "1", new Vector3(-3, 1, 3), new DCL.Components.BasicMaterial.Model
        {
            texture = "http://127.0.0.1:9991/Images/atlas.png",
            samplingMode = 0,
            wrap = 0
        }, "testBasicMaterial");

        TestHelpers.InstantiateEntityWithMaterial(scene, "2", new Vector3(0, 1, 3), new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = "http://127.0.0.1:9991/Images/avatar.png",
            metallic = 0,
            roughness = 1,
            hasAlpha = true
        }, "testMaterial1");

        string materialID = "testMaterial2";
        TestHelpers.InstantiateEntityWithMaterial(scene, "3", new Vector3(3, 1, 3), new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = "http://127.0.0.1:9991/Images/avatar.png",
            metallic = 1,
            roughness = 1,
            alphaTexture = "http://127.0.0.1:9991/Images/avatar.png",
        }, materialID);

        // Re-assign last PBR material to new entity
        TestHelpers.InstantiateEntityWithShape(scene, "4", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(6, 1, 3));

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
                albedoTexture = "http://127.0.0.1:9991/Images/avatar.png",
                metallic = 1,
                roughness = 1,
                alphaTexture = "http://127.0.0.1:9991/Images/avatar.png",
                albedoColor = "#FF9292"
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
        string baselineImagesPath = "VisualTests/Test1/BaselineImages/";
        string currentTestImagesPath = "VisualTests/Test1/CurrentTestImages/";
        string outputPath = "/Resources/" + (snapshotsAreBaseline ? baselineImagesPath : currentTestImagesPath);

        string snapshot1Name = "snapshot_1";
        string snapshot2Name = "snapshot_2";
        string snapshot3Name = "snapshot_3";
        string snapshot4Name = "snapshot_4";

        // Create Camera
        Camera visualTestsCam = new GameObject("VisualTestsCam").AddComponent<Camera>();
        visualTestsCam.depth = 11;

        yield return new WaitForEndOfFrame();

        // Take 1st snapshot
        VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(10f, 10f, 0f));
        yield return VisualTestHelpers.TakeSnapshot(outputPath + snapshot1Name + ".png", visualTestsCam);

        // Take 2nd snapshot
        yield return new WaitForSeconds(0.2f);
        VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(0f, 10f, 10f));
        yield return VisualTestHelpers.TakeSnapshot(outputPath + snapshot2Name + ".png", visualTestsCam);

        // Take 3rd snapshot
        yield return new WaitForSeconds(0.2f);
        VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(-10f, 10f, 0f));
        yield return VisualTestHelpers.TakeSnapshot(outputPath + snapshot3Name + ".png", visualTestsCam);

        // Take 4th snapshot
        yield return new WaitForSeconds(0.2f);
        VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(0f, 10f, -10f));
        yield return VisualTestHelpers.TakeSnapshot(outputPath + snapshot4Name + ".png", visualTestsCam);

        yield return new WaitForSeconds(0.2f);
        Destroy(visualTestsCam.gameObject);

        if (snapshotsAreBaseline) yield break; // We don't need to compare anything if the snapshots taken are baseline images

        // ---------------------------------------------
        // COMPARE IMAGES

        Texture2D baselineSnapshot1 = Resources.Load<Texture2D>(baselineImagesPath + snapshot1Name);
        Texture2D baselineSnapshot2 = Resources.Load<Texture2D>(baselineImagesPath + snapshot2Name);
        Texture2D baselineSnapshot3 = Resources.Load<Texture2D>(baselineImagesPath + snapshot3Name);
        Texture2D baselineSnapshot4 = Resources.Load<Texture2D>(baselineImagesPath + snapshot4Name);

        Texture2D currentSnapshot1 = Resources.Load<Texture2D>(currentTestImagesPath + snapshot1Name);
        Texture2D currentSnapshot2 = Resources.Load<Texture2D>(currentTestImagesPath + snapshot2Name);
        Texture2D currentSnapshot3 = Resources.Load<Texture2D>(currentTestImagesPath + snapshot3Name);
        Texture2D currentSnapshot4 = Resources.Load<Texture2D>(currentTestImagesPath + snapshot4Name);

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
