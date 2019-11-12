using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using WebSocketSharp;

namespace Tests
{
    public class WSSTests : TestsBase
    {
        // TODO: Find a way to run this test on Unity Cloud Build, even though it passes locally, it fails on timeout in Unity Cloud Build
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator BasicConnectionTest()
        {
            yield return InitScene();

            GameObject wssControllerGO = new GameObject("WSS Controller");

            WSSController wssController = wssControllerGO.AddComponent<WSSController>();
            wssController.sceneController = sceneController;
            DCLCharacterController.i.gravity = 0;

            yield return new WaitForSeconds(1.0f);

            Assert.IsTrue(wssController.isServerReady);

            using (WebSocketSharp.WebSocket ws = new WebSocket("ws://localhost:5000/dcl"))
            {
                try
                {
                    ws.Connect();
                }
                catch (Exception e)
                {
                    Debug.LogError("message: " + e.ToString());
                    Assert.Fail("Failed to connect to decentraland service!");
                }

                string payloadTest = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

                DCLWebSocketService.Message message = new DCLWebSocketService.Message()
                {
                    type = "LoadParcelScenes",
                    payload = payloadTest
                };

                string json = "";
                try
                {
                    json = JsonUtility.ToJson(message);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    Assert.Fail("Invalid object?: " + json);
                }

                ws.Send(json);
                Debug.Log("<color=#007F00><b>WSSTests</b></color> >>> Waiting for server response...");

                string lastMessage = "";

                ws.OnMessage += (sender, e) =>
                {
                    Debug.Log("<color=#007F00><b>WSSTests</b></color> >>> Server sent: " + e.Data);
                    lastMessage = e.Data;
                };

                float time = 0;
                while (time < 8.0f)
                {
                    time += Time.unscaledDeltaTime;

                    if (lastMessage.Contains("OK!"))
                    {
                        break;
                    }

                    yield return null;
                }

                Assert.LessOrEqual(Mathf.FloorToInt(time), 8);

                string loadedSceneID = "0,0";

                yield return null;

                Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID),
                    "Expected loadedScene not found!");
                Assert.IsTrue(sceneController.loadedScenes[loadedSceneID] != null,
                    "Expected loadedScene found but was null!!!");
            }

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}