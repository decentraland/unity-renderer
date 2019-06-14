using DCL;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;

namespace Tests
{
    public class MessageTests : TestsBase
    {
        bool VERBOSE = false;

        class Message
        {
            public string id;
            public string method;

            public Message(string id, string method)
            {
                this.id = id;
                this.method = method;
            }
        }

        [UnityTest]
        public IEnumerator MessagingTest()
        {
            yield return InitScene();

            var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

            yield return new WaitForSeconds(0.1f);

            sceneController.UnloadAllScenes();

            yield return new WaitForSeconds(0.1f);

            sceneController.LoadParcelScenes(scenesToLoad);

            yield return new WaitForAllMessagesProcessed();

            string uiSceneId = sceneController.GlobalSceneId;
            string entityId = "a5f571bd-bce1-4cf8-a158-b8f3e92e4fb0";
            string sceneId = "0,0";
            string busId = "";

            List<Message> msgs = new List<Message>();

            // First pack of messages to process
            msgs.Add(new Message(uiSceneId, "CreateEntity"));
            msgs.Add(new Message(uiSceneId, "SetEntityParent"));
            msgs.Add(new Message(uiSceneId, "CreateEntity"));
            msgs.Add(new Message(sceneId, "CreateEntity"));
            msgs.Add(new Message(sceneId, "SceneStarted"));
            msgs.Add(new Message(sceneId, "SetEntityParent"));
            msgs.Add(new Message(sceneId, "CreateEntity"));
            // Second pack of messages
            msgs.Add(new Message(uiSceneId, "SetEntityParent"));
            msgs.Add(new Message(uiSceneId, "CreateEntity"));
            msgs.Add(new Message(sceneId, "SetEntityParent"));
            msgs.Add(new Message(sceneId, "CreateEntity"));

            int msgId = 0;

            sceneController.OnMessageProcessInfoStart += delegate (string id, string method, string payload)
            {
                if (VERBOSE)
                    Debug.Log($"Msg Processed: {id} - {method}");

                Assert.IsTrue(msgs[msgId].id == id && msgs[msgId].method == method);

                msgId++;
            };

            yield return new WaitForAllMessagesProcessed();

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    "CreateEntity",
                    JsonConvert.SerializeObject(
                        new CreateEntityMessage
                        {
                            id = entityId
                        }))
            );

            Assert.IsTrue(busId == MessagingBusId.UI);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    "CreateEntity",
                    JsonConvert.SerializeObject(
                        new CreateEntityMessage
                        {
                            id = entityId
                        }))
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    "SceneStarted",
                    ""
                )
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    "SetEntityParent",
                    JsonConvert.SerializeObject(
                        new
                        {
                            entityId = entityId,
                            parentId = "0"
                        })
                )
            );

            Assert.IsTrue(busId == MessagingBusId.UI);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    "SetEntityParent",
                    JsonConvert.SerializeObject(
                        new
                        {
                            entityId = entityId,
                            parentId = "0"
                        })
                )
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    "CreateEntity",
                    JsonConvert.SerializeObject(
                        new CreateEntityMessage
                        {
                            id = entityId
                        }))
            );

            Assert.IsTrue(busId == MessagingBusId.UI);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    "CreateEntity",
                    JsonConvert.SerializeObject(
                        new CreateEntityMessage
                        {
                            id = entityId
                        }))
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            yield return new WaitForAllMessagesProcessed();

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    "SetEntityParent",
                    JsonConvert.SerializeObject(
                        new
                        {
                            entityId = entityId,
                            parentId = "0"
                        })
                )
            );

            Assert.IsTrue(busId == MessagingBusId.UI);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    "SetEntityParent",
                    JsonConvert.SerializeObject(
                        new
                        {
                            entityId = entityId,
                            parentId = "0"
                        })
                )
            );

            Assert.IsTrue(busId == MessagingBusId.SYSTEM);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    "CreateEntity",
                    JsonConvert.SerializeObject(
                        new CreateEntityMessage
                        {
                            id = entityId
                        }))
            );

            Assert.IsTrue(busId == MessagingBusId.UI);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    "CreateEntity",
                    JsonConvert.SerializeObject(
                        new CreateEntityMessage
                        {
                            id = entityId
                        }))
            );

            Assert.IsTrue(busId == MessagingBusId.SYSTEM);

            yield return new WaitForAllMessagesProcessed();
        }
    }
}
