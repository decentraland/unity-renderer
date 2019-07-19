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
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_CREATE));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_CREATE));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_CREATE));
            msgs.Add(new Message(sceneId, MessagingTypes.SHARED_COMPONENT_CREATE));

            // Although we sent several component update messages for the same id, 
            // it should process only one because they are unreliable
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_COMPONENT_CREATE));
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_COMPONENT_CREATE));
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_COMPONENT_CREATE));
            msgs.Add(new Message(sceneId, MessagingTypes.SCENE_STARTED));


            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_CREATE));
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_REPARENT));

            // Second pack of messages
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_CREATE));

            int msgId = 0;

            sceneController.OnMessageProcessInfoStart += delegate (string id, string method, string payload)
            {
                if (VERBOSE)
                {
                    Debug.Log($"Msg Processed: {id} - {method} | expected: {msgs[msgId].id} - {msgs[msgId].method}");
                }

                Assert.IsTrue(msgs[msgId].id == id && msgs[msgId].method == method, $"Msg Processed: [{msgId}] {id} - {method} | expected: {msgs[msgId].id} - {msgs[msgId].method}");

                msgId++;
            };

            yield return null;

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    entityId,
                    MessagingTypes.ENTITY_CREATE,
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
                    MessagingTypes.SHARED_COMPONENT_CREATE,
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
                    MessagingTypes.ENTITY_COMPONENT_CREATE,
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
                    MessagingTypes.ENTITY_COMPONENT_CREATE,
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
                    MessagingTypes.ENTITY_COMPONENT_CREATE,
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
                    MessagingTypes.SCENE_STARTED,
                    ""
                )
            );

            Assert.IsTrue(busId == MessagingBusId.INIT);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    entityId,
                    MessagingTypes.ENTITY_REPARENT,
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
                    MessagingTypes.ENTITY_COMPONENT_CREATE,
                    JsonConvert.SerializeObject(
                        new EntityComponentCreateMessage
                        {
                            entityId = entityId,
                            classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
                        }
                    ))
            );

            Assert.IsTrue(busId == MessagingBusId.SYSTEM);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    sceneId,
                    entityId,
                    MessagingTypes.ENTITY_REPARENT,
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
                    MessagingTypes.ENTITY_CREATE,
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
                    MessagingTypes.ENTITY_CREATE,
                    JsonConvert.SerializeObject(
                        new CreateEntityMessage
                        {
                            id = entityId
                        }))
            );

            Assert.IsTrue(busId == MessagingBusId.SYSTEM);

            yield return new WaitForSeconds(1f);

            busId = sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    uiSceneId,
                    entityId,
                    MessagingTypes.ENTITY_REPARENT,
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
                    MessagingTypes.ENTITY_REPARENT,
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
                    MessagingTypes.ENTITY_REPARENT,
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
                    MessagingTypes.ENTITY_CREATE,
                    JsonConvert.SerializeObject(
                        new CreateEntityMessage
                        {
                            id = entityId
                        }))
            );

            Assert.IsTrue(busId == MessagingBusId.SYSTEM);

            yield return new WaitForSeconds(1f);
        }
    }
}
