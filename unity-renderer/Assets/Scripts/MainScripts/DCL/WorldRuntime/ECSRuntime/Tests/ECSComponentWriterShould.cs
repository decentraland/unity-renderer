using System.Text;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCL.ECSRuntime.Tests;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSComponentWriterShould
    {
        private const int SCENE_NUMBER = 666;
        private const long ENTITY_ID = 42;
        private const int COMPONENT_ID = 26;

        private ECSComponentWriter componentWriter;
        private IDummyEventSubscriber<int, long, int, byte[], int, ECSComponentWriteType, CrdtMessageType> writeComponentSubscriber;
        private IParcelScene scene;
        private IDCLEntity entity;

        [SetUp]
        public void SetUp()
        {
            writeComponentSubscriber = Substitute.For<IDummyEventSubscriber<int, long, int, byte[], int, ECSComponentWriteType, CrdtMessageType>>();
            componentWriter = new ECSComponentWriter(writeComponentSubscriber.React);
            scene = Substitute.For<IParcelScene>();
            entity = Substitute.For<IDCLEntity>();

            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene() { sceneNumber = SCENE_NUMBER });
            entity.entityId.Returns(ENTITY_ID);

            componentWriter.AddOrReplaceComponentSerializer<TestingComponent>(COMPONENT_ID,
                model =>
                {
                    string value = JsonUtility.ToJson(model);
                    return Encoding.UTF8.GetBytes(value);
                });
        }

        [Test]
        public void PutComponent()
        {
            var model = new TestingComponent()
            {
                someString = "new-temptation",
                someVector = Vector3.up
            };

            componentWriter.PutComponent(scene, entity, COMPONENT_ID, model, ECSComponentWriteType.DEFAULT);
            writeComponentSubscriber.Received(1)
                                    .React(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, Arg.Any<byte[]>(),
                                        Arg.Any<int>(), Arg.Any<ECSComponentWriteType>(), Arg.Any<CrdtMessageType>());
        }

        [Test]
        public void RemoveComponent()
        {
            componentWriter.RemoveComponent(scene.sceneData.sceneNumber, entity.entityId, COMPONENT_ID, ECSComponentWriteType.DEFAULT);
            writeComponentSubscriber.Received(1)
                                    .React(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, null,
                                        Arg.Any<int>(), Arg.Any<ECSComponentWriteType>(), Arg.Any<CrdtMessageType>());
        }
    }
}
