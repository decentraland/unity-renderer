using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements.Tests;
using DCL.ECSRuntime;
using Google.Protobuf;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace ECSSystems.UIInputSenderSystem
{
    public class ECSUIInputSenderSystemShould : UIComponentsShouldBase
    {
        private IECSComponentWriter componentWriter;
        private ECSUIInputSenderSystem system;

        [SetUp]
        public void SetUpSystem()
        {
            componentWriter = Substitute.For<IECSComponentWriter>();
            system = new ECSUIInputSenderSystem(uiInputResultsComponent, componentWriter);
        }

        [Test]
        public void SendResultsWhenDirty()
        {
            var results = new (IMessage resultMessage, int componentId)[]
            {
                (Substitute.For<IMessage>(), 100),
                (Substitute.For<IMessage>(), 200),
                (Substitute.For<IMessage>(), 300),
                (Substitute.For<IMessage>(), 400),
            };

            uiInputResultsComponent.GetForAll()
                          .Returns(new List<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalUIInputResults>>>
                           {
                               CreateComponentData(true, results)
                           });

            system.Update();

            foreach (var r in results)
            {
                componentWriter.Received(1)
                               .PutComponent(
                                    r.resultMessage.GetType(),
                                    scene.sceneData.sceneNumber,
                                    entity.entityId,
                                    r.componentId,
                                    r.resultMessage,
                                    ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY
                                );
            }
        }

        [Test]
        public void NotSendResultsWhenIsNotDirty()
        {
            var results = new (IMessage resultMessage, int componentId)[]
            {
                (Substitute.For<IMessage>(), 100),
                (Substitute.For<IMessage>(), 200),
                (Substitute.For<IMessage>(), 300),
                (Substitute.For<IMessage>(), 400),
            };

            uiInputResultsComponent.GetForAll()
                          .Returns(new List<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalUIInputResults>>>
                           {
                               CreateComponentData(false, results)
                           });

            system.Update();

            foreach (var r in results)
            {
                componentWriter.DidNotReceive()
                               .PutComponent(
                                    r.resultMessage.GetType(),
                                    scene.sceneData.sceneNumber,
                                    entity.entityId,
                                    r.componentId,
                                    r.resultMessage,
                                    ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY
                                );
            }
        }

        private KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalUIInputResults>> CreateComponentData(bool dirty,
            params (IMessage resultMessage, int componentId)[] results)
        {
            var model = new InternalUIInputResults();
            model._dirty = dirty;

            foreach (var result in results) { model.Results.Enqueue(new InternalUIInputResults.Result(result.resultMessage, result.componentId)); }

            return new (scene, entity.entityId,
                new ECSComponentData<InternalUIInputResults>
                {
                    entity = entity, scene = scene, model = model
                });
        }
    }
}
