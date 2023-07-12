using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests
{
    public class PointerEventsShould
    {
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private PointerEventsHandler handler;
        private IInternalECSComponent<InternalPointerEvents> internalPointerEventsComponent;
        private InternalECSComponents internalComponent;

        [SetUp]
        public void SetUp()
        {
            testUtils = new ECS7TestUtilsScenesAndEntities();
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(100);

            InternalPointerEvents? componentData = null;
            internalPointerEventsComponent = Substitute.For<IInternalECSComponent<InternalPointerEvents>>();

            internalPointerEventsComponent.When(substituteCall =>
                                               substituteCall.PutFor(
                                                   Arg.Any<IParcelScene>(),
                                                   Arg.Any<IDCLEntity>(),
                                                   Arg.Any<InternalPointerEvents>()))
                                          .Do(info => componentData = info.ArgAt<InternalPointerEvents>(2));

            internalPointerEventsComponent.GetFor(Arg.Any<IParcelScene>(), Arg.Any<IDCLEntity>())
                                          .Returns(i =>
                                           {
                                               if (componentData == null)
                                                   return null;

                                               return new ECSComponentData<InternalPointerEvents>
                                               (
                                                   entity: entity,
                                                   scene: scene,
                                                   model: componentData.Value,
                                                   handler: null
                                               );
                                           });

            internalPointerEventsComponent.When(substituteCall =>
                                               substituteCall.RemoveFor(
                                                   Arg.Any<IParcelScene>(),
                                                   Arg.Any<IDCLEntity>()))
                                          .Do(_ => componentData = null);

            handler = new PointerEventsHandler(internalPointerEventsComponent);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void CreateInternalPointerEventsCorrectly()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBPointerEvents());
            Assert.IsNotNull(internalPointerEventsComponent.GetFor(scene, entity)?.model);
        }

        [Test]
        public void UpdateInternalPointerEventsCorrectly()
        {
            PBPointerEvents pointerEvents = new PBPointerEvents()
            {
                PointerEvents =
                {
                    new List<PBPointerEvents.Types.Entry>()
                    {
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.PetDown,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaAny,
                                HoverText = "Temptation"
                            }
                        }
                    }
                }
            };

            handler.OnComponentModelUpdated(scene, entity, pointerEvents);
            var model = internalPointerEventsComponent.GetFor(scene, entity).Value.model;

            Assert.AreEqual(pointerEvents.PointerEvents.Count, model.PointerEvents.Count);
            Assert.AreEqual(pointerEvents.PointerEvents[0].EventType, model.PointerEvents[0].EventType);
            Assert.AreEqual(pointerEvents.PointerEvents[0].EventInfo.Button, model.PointerEvents[0].EventInfo.Button);
            Assert.AreEqual(pointerEvents.PointerEvents[0].EventInfo.HoverText, model.PointerEvents[0].EventInfo.HoverText);
        }

        [Test]
        public void RemoveInternalPointerEventsCorrectly()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBPointerEvents());
            handler.OnComponentRemoved(scene, entity);
            Assert.IsNull(internalPointerEventsComponent.GetFor(scene, entity)?.model);
        }
    }
}
