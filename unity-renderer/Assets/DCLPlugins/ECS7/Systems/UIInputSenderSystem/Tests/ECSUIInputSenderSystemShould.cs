using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSComponents.UIAbstractElements.Tests;
using DCL.ECSRuntime;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using TestUtils;

namespace ECSSystems.UIInputSenderSystem
{
    public class ECSUIInputSenderSystemShould : UIComponentsShouldBase
    {
        private DualKeyValueSet<long, int, WriteData> outgoingMessages;
        private ECSUIInputSenderSystem system;

        [SetUp]
        public void SetUpSystem()
        {
            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();

            var componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { 666, new ComponentWriter(outgoingMessages) }
            };

            system = new ECSUIInputSenderSystem(uiInputResultsComponent, componentsWriter);
        }

        [Test]
        public void SendResultsWhenDirty()
        {
            var results = new (PBUiInputResult resultMessage, int componentId)[]
            {
                (new PBUiInputResult(), 100),
                (new PBUiInputResult(), 200),
                (new PBUiInputResult(), 300),
                (new PBUiInputResult(), 400),
            };

            uiInputResultsComponent.GetForAll()
                                   .Returns(new List<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalUIInputResults>>>
                                    {
                                        CreateComponentData(true, results)
                                    });

            system.Update();

            foreach (var r in results)
            {
                outgoingMessages.Put_Called<PBUiInputResult>(
                    entity.entityId,
                    r.componentId,
                    null
                );
            }
        }

        [Test]
        public void NotSendResultsWhenIsNotDirty()
        {
            var results = new (PBUiInputResult resultMessage, int componentId)[]
            {
                (new PBUiInputResult(), 100),
                (new PBUiInputResult(), 200),
                (new PBUiInputResult(), 300),
                (new PBUiInputResult(), 400),
            };

            uiInputResultsComponent.GetForAll()
                                   .Returns(new List<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalUIInputResults>>>
                                    {
                                        CreateComponentData(false, results)
                                    });

            system.Update();

            foreach (var r in results)
            {
                outgoingMessages.Put_NotCalled(
                    entity.entityId,
                    r.componentId
                );
            }
        }

        private KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalUIInputResults>> CreateComponentData(bool dirty,
            params (PBUiInputResult resultMessage, int componentId)[] results)
        {
            var componentPool = new WrappedComponentPool<IWrappedComponent<PBUiInputResult>>(0, () => new ProtobufWrappedComponent<PBUiInputResult>(new PBUiInputResult()));

            var model = new InternalUIInputResults(new Queue<InternalUIInputResults.Result>());
            model.dirty = dirty;

            foreach (var result in results)
            {
                var pooledElement = componentPool.Get();
                model.Results.Enqueue(new InternalUIInputResults.Result(pooledElement, result.componentId));
            }

            return new (scene, entity.entityId,
                new ECSComponentData<InternalUIInputResults>
                (
                    entity: entity, scene: scene, model: model, handler: null
                ));
        }
    }
}
