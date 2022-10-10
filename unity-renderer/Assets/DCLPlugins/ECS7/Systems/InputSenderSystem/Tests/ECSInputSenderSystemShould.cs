using System;
using System.Collections.Generic;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.InputSenderSystem;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class ECSInputSenderSystemShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;

        private IInternalECSComponent<InternalInputEventResults> inputResultComponent;
        private IECSComponentWriter componentWriter;
        private InternalECSComponents internalComponents;

        private Action updateSystems;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory);

            inputResultComponent = internalComponents.inputEventResultsComponent;
            componentWriter = Substitute.For<IECSComponentWriter>();

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
            scene = testUtils.CreateScene("temptation");

            var systemUpdate = ECSInputSenderSystem.CreateSystem(inputResultComponent, componentWriter);
            updateSystems = () =>
            {
                systemUpdate();
                internalComponents.WriteSystemUpdate();
            };
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void SendResultsWhenDirty()
        {
            IList<InternalInputEventResults.EventData> events = new List<InternalInputEventResults.EventData>()
            {
                new InternalInputEventResults.EventData() { button = ActionButton.Primary },
                new InternalInputEventResults.EventData() { button = ActionButton.Secondary },
                new InternalInputEventResults.EventData() { button = ActionButton.Action4 },
                new InternalInputEventResults.EventData() { button = ActionButton.Action3 },
                new InternalInputEventResults.EventData() { button = ActionButton.Action5 },
            };

            foreach (var eventData in events)
            {
                inputResultComponent.AddEvent(scene, eventData);
            }

            updateSystems();

            componentWriter.Received(1)
                           .PutComponent(
                               scene.sceneData.id,
                               SpecialEntityId.SCENE_ROOT_ENTITY,
                               ComponentID.POINTER_EVENTS_RESULT,
                               Arg.Do<PBPointerEventsResult>(r =>
                               {
                                   for (int i = 0; i < r.Commands.Count; i++)
                                   {
                                       Assert.AreEqual(events[i].button, r.Commands[i].Button);
                                   }
                               }),
                               ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY);
        }

        [Test]
        public void NotSendResultsWhenIsNotDirty()
        {
            IList<InternalInputEventResults.EventData> events = new List<InternalInputEventResults.EventData>()
            {
                new InternalInputEventResults.EventData() { button = ActionButton.Primary },
                new InternalInputEventResults.EventData() { button = ActionButton.Secondary },
                new InternalInputEventResults.EventData() { button = ActionButton.Action4 },
                new InternalInputEventResults.EventData() { button = ActionButton.Action3 },
                new InternalInputEventResults.EventData() { button = ActionButton.Action5 },
            };

            foreach (var eventData in events)
            {
                inputResultComponent.AddEvent(scene, eventData);
            }

            var compData = inputResultComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var model = compData.model;

            inputResultComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
            internalComponents.WriteSystemUpdate(); //clean dirty

            updateSystems();

            componentWriter.DidNotReceive()
                           .PutComponent(
                               scene.sceneData.id,
                               SpecialEntityId.SCENE_ROOT_ENTITY,
                               ComponentID.POINTER_EVENTS_RESULT,
                               Arg.Any<PBPointerEventsResult>(),
                               ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY);
        }

        [Test]
        public void NotRemoveEventsFromInternalComponent()
        {
            IList<InternalInputEventResults.EventData> events = new List<InternalInputEventResults.EventData>()
            {
                new InternalInputEventResults.EventData() { button = ActionButton.Primary },
                new InternalInputEventResults.EventData() { button = ActionButton.Secondary },
                new InternalInputEventResults.EventData() { button = ActionButton.Action4 },
                new InternalInputEventResults.EventData() { button = ActionButton.Action3 },
                new InternalInputEventResults.EventData() { button = ActionButton.Action5 },
            };

            foreach (var eventData in events)
            {
                inputResultComponent.AddEvent(scene, eventData);
            }

            updateSystems();

            var compData = inputResultComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var model = compData.model;

            for (int i = 0; i < events.Count; i++)
            {
                Assert.AreEqual(events[i].button, model.events.Dequeue().button);
            }
        }
    }
}