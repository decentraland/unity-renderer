using DCL.Helpers;
using System;
using System.Collections;
using DCL.Controllers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Environment = DCL.Environment;

public class GLTFLoadingTestController : MonoBehaviour
{
    public string dataTrackingURL = "https://tracking.decentraland.org/track";

    float loadingStartingTime;

    void Start()
    {
        // ---------
        var sceneController = Environment.i.world.sceneController;
        string scenesToLoad = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/Tests/TestJSON/SceneLoadingTest").text;

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(scenesToLoad);

        var scene = Environment.i.world.state.GetScene(666) as ParcelScene;

        // FULL GLB
        TestUtils.InstantiateEntityWithShape(scene, 1, DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(-2.5f, 1, 0),
            TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb");

        // GLB + Separated Textures
        TestUtils.InstantiateEntityWithShape(scene, 2, DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0f, 1, 0),
            TestAssetsUtils.GetPath() + "/GLB/TrunkSeparatedTextures/Trunk.glb");

        // GLTF
        TestUtils.InstantiateEntityWithShape(scene, 3, DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(2.5f, 1, 0),
            TestAssetsUtils.GetPath() + "/GLTF/Trunk/Trunk.gltf");
        // ---------
    }

    void SendLoadingTimeDataToEndpoint()
    {
        GLTFLoadingTestTrackingData data = new GLTFLoadingTestTrackingData();
        data.events[0].TestResultInMilliseconds = (Time.time - loadingStartingTime).ToString();

        string parsedJSON = JsonUtility.ToJson(data);

        StartCoroutine(PostRequest(dataTrackingURL, parsedJSON));
    }

    IEnumerator PostRequest(string url, string json)
    {
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler) new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        // Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }
    }

    [Serializable]
    class GLTFLoadingTestTrackingData
    {
        public string user = "WorldTeam-Unity";
        public TestEvent[] events = new TestEvent[1];

        public GLTFLoadingTestTrackingData() { events[0] = new TestEvent(); }

        [Serializable]
        public class TestEvent
        {
            public string TestRun = "GLTF-LoadingTime";
            public string TestResultInMilliseconds;
            public string GLTBFileSize = "9.5MB (Lantern)";
        }
    }
}
