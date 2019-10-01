using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Tests
{
    public class MessageTests : TestsBase
    {
        static bool VERBOSE = false;

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
        public IEnumerator EntityComponentMessagesArentInterrupted()
        {
            yield return InitScene();

            var entity = TestHelpers.CreateSceneEntity(scene);

            yield return null;

            sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    scene.sceneData.id,
                    entity.entityId + "_" + (int)CLASS_ID_COMPONENT.AVATAR_SHAPE,
                    MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE,
                    JsonConvert.SerializeObject(
                        new EntityComponentCreateMessage
                        {
                            entityId = entity.entityId,
                            classId = (int)CLASS_ID_COMPONENT.AVATAR_SHAPE,
                            json = JsonUtility.ToJson(new AvatarModel())
                        }
                    ))
            );

            sceneController.SendSceneMessage(
                TestHelpers.CreateSceneMessage(
                    scene.sceneData.id,
                    entity.entityId + "_" + (int)CLASS_ID_COMPONENT.AVATAR_SHAPE,
                    MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE,
                    JsonConvert.SerializeObject(
                        new EntityComponentCreateMessage
                        {
                            entityId = entity.entityId,
                            classId = (int)CLASS_ID_COMPONENT.AVATAR_SHAPE,
                            json = JsonUtility.ToJson(new AvatarModel())
                        }
                    ))
            );

            sceneController.OnMessageProcessInfoStart += delegate (string id, string method, string payload)
            {
                if (VERBOSE)
                {
                    Debug.Log($"Msg Processed: {id} - {method} - {payload}");
                }
            };

            //NOTE(Brian): Interruption asserts are inside ComponentUpdateHandler.cs
            yield return new WaitForAllMessagesProcessed();
        }


        [UnityTest]
        public IEnumerator UnreliableMessagesAreReplacedCorrectly()
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
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_CREATE));

            // Although we sent several component update messages for the same id, 
            // it should process only one because they are unreliable
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE));
            msgs.Add(new Message(sceneId, MessagingTypes.INIT_DONE));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_CREATE));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_CREATE));
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE));
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_CREATE));

            // Second pack of messages
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_CREATE));

            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_REPARENT));
            msgs.Add(new Message(uiSceneId, MessagingTypes.ENTITY_CREATE));

            msgs.Add(new Message(sceneId, MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE));

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
                    MessagingTypes.ENTITY_CREATE,
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
                    MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE,
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
                    MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE,
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
                    MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE,
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
                    MessagingTypes.INIT_DONE,
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
                    MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE,
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
