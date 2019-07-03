using DCL;
using DCL.Helpers;
using DCL.Models;
using DCL.Components;
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

            // For UI component creation to work, we need a screenSpaceShape in the scene
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            UIImage image = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);

            yield return image.routine;

            string uiSceneId = sceneController.GlobalSceneId;
            string entityId = "E1";
            string sceneId = scene.sceneData.id;
            string busId = "";

            List<Message> msgs = new List<Message>();

            // First pack of messages to process
            msgs.Add(new Message(uiSceneId, "CreateEntity"));
            msgs.Add(new Message(uiSceneId, "CreateEntity"));
            msgs.Add(new Message(uiSceneId, "SetEntityParent"));
            msgs.Add(new Message(sceneId, "CreateEntity"));

            // Although we sent several component update messages for the same id, 
            // it should process only one because they are unreliable
            msgs.Add(new Message(sceneId, "SceneStarted"));
            msgs.Add(new Message(sceneId, "CreateEntity"));
            msgs.Add(new Message(sceneId, "UpdateEntityComponent"));
            msgs.Add(new Message(sceneId, "SetEntityParent"));

            // Second pack of messages
            msgs.Add(new Message(uiSceneId, "SetEntityParent"));
            msgs.Add(new Message(uiSceneId, "CreateEntity"));
            msgs.Add(new Message(sceneId, "SetEntityParent"));
            msgs.Add(new Message(sceneId, "CreateEntity"));

            int msgId = 0;

            sceneController.OnMessageProcessInfoStart += delegate (string id, string method, string payload)
            {
                if (VERBOSE)
                {
                    Debug.Log($"Msg Processed: {id} - {method} | expected: {msgs[msgId].id} - {msgs[msgId].method}");
                }

                Assert.IsTrue(msgs[msgId].id == id && msgs[msgId].method == method);

                msgId++;
            };

            yield return new WaitForAllMessagesProcessed();

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    entityId,
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
                    entityId,
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
                    entityId + "_" + (int)CLASS_ID_COMPONENT.TRANSFORM,
                    "UpdateEntityComponent",
                    JsonConvert.SerializeObject(
                        new EntityComponentCreateMessage
                        {
                            entityId = entityId,
                            classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
                        }
                    ))
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    entityId + "_" + (int)CLASS_ID_COMPONENT.TRANSFORM,
                    "UpdateEntityComponent",
                    JsonConvert.SerializeObject(
                        new EntityComponentCreateMessage
                        {
                            entityId = entityId,
                            classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
                        }
                    ))
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    entityId + "_" + (int)CLASS_ID_COMPONENT.TRANSFORM,
                    "UpdateEntityComponent",
                    JsonConvert.SerializeObject(
                        new EntityComponentCreateMessage
                        {
                            entityId = entityId,
                            classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
                        }
                    ))
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    "",
                    "SceneStarted",
                    ""
                )
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    entityId,
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
                    entityId + "_" + (int)CLASS_ID_COMPONENT.TRANSFORM,
                    "UpdateEntityComponent",
                    JsonConvert.SerializeObject(
                        new EntityComponentCreateMessage
                        {
                            entityId = entityId,
                            classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
                        }
                    ))
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    entityId,
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
                    entityId,
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
                    entityId,
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
                    entityId,
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
                    entityId,
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
                    entityId,
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
                    entityId,
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
