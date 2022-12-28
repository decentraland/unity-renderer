using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements.Tests;
using DCL.ECSRuntime;
using DCL.Models;
using Google.Protobuf;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.Tests
{
    [TestFixture]
    public class UIPointerEventsUtilsShould : UIComponentsShouldBase
    {
        private const int COMPONENT_ID = 101;

        private UIDocument document;

        private EventCallback<PointerUpEvent> callback;
        private VisualElement uiElement;
        private UIEventsSubscriptions subscriptions;

        [SetUp]
        public void SetupElement()
        {
            document = InstantiateUiDocument();
        }

        [TearDown]
        public void DisposeCallbacks()
        {
            if (callback != null)
                uiElement.UnregisterCallback(callback);

            callback = null;

            subscriptions?.Dispose();
            subscriptions = null;
        }

        [UnityTest]
        public IEnumerator FilterMismatchedButton([Values(1, 2)] int mismatchedButtonId)
        {
            ECSComponentData<InternalInputEventResults> internalCompData = new ECSComponentData<InternalInputEventResults>
            {
                model = new InternalInputEventResults {events = new Queue<InternalInputEventResults.EventData>()}
            };

            var inputResultsComp = Substitute.For<IInternalECSComponent<InternalInputEventResults>>();
            inputResultsComp.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).Returns(_ => internalCompData);

            yield return CreateStretchedElement();

            var requestedInteraction = new List<PBPointerEvents.Types.Entry>
            {
                new ()
                {
                    EventType = PointerEventType.PetDown,
                    EventInfo = new PBPointerEvents.Types.Info { Button = InputAction.IaPointer }
                }
            };

            subscriptions = UIPointerEventsUtils.AddCommonInteractivity(uiElement, scene, entity,
                requestedInteraction, inputResultsComp);

            using (var evt = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = mismatchedButtonId,
                       mousePosition = new UnityEngine.Vector2(50f, 50f)
                   })) { document.rootVisualElement.panel.visualTree.SendEvent(evt); }

            inputResultsComp.DidNotReceive().PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, default);
            CollectionAssert.IsEmpty(internalCompData.model.events);
        }

        [UnityTest]
        public IEnumerator FilterUnsupportedAction(
            [Values(InputAction.IaAction3, InputAction.IaSecondary)] InputAction unsupportedAction)
        {
            ECSComponentData<InternalInputEventResults> internalCompData = new ECSComponentData<InternalInputEventResults>
            {
                model = new InternalInputEventResults {events = new Queue<InternalInputEventResults.EventData>()}
            };

            var inputResultsComp = Substitute.For<IInternalECSComponent<InternalInputEventResults>>();
            inputResultsComp.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).Returns(_ => internalCompData);

            yield return CreateStretchedElement();

            var requestedInteraction = new List<PBPointerEvents.Types.Entry>
            {
                new ()
                {
                    EventType = PointerEventType.PetDown,
                    EventInfo = new PBPointerEvents.Types.Info { Button = unsupportedAction }
                }
            };

            subscriptions = UIPointerEventsUtils.AddCommonInteractivity(uiElement, scene, entity,
                requestedInteraction, inputResultsComp);

            // Now simulate PointerDown event

            using (var evt = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       mousePosition = new UnityEngine.Vector2(50f, 50f)
                   })) { document.rootVisualElement.panel.visualTree.SendEvent(evt); }

            CollectionAssert.IsEmpty(internalCompData.model.events);
        }

        [UnityTest]
        public IEnumerator AddCommonInteractivity()
        {
            ECSComponentData<InternalInputEventResults> internalCompData = new ECSComponentData<InternalInputEventResults>
            {
                model = new InternalInputEventResults {events = new ()}
            };

            var inputResultsComp = Substitute.For<IInternalECSComponent<InternalInputEventResults>>();
            inputResultsComp.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).Returns(_ => internalCompData);

            yield return CreateStretchedElement();

            var requestedInteraction = new List<PBPointerEvents.Types.Entry>
            {
                new ()
                {
                    EventType = PointerEventType.PetDown,
                    EventInfo = new PBPointerEvents.Types.Info { Button = InputAction.IaPointer }
                }
            };

            subscriptions = UIPointerEventsUtils.AddCommonInteractivity(uiElement, scene, entity,
                requestedInteraction, inputResultsComp);

            // Now simulate PointerDown event

            using (var evt = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       mousePosition = new UnityEngine.Vector2(50f, 50f)
                   })) { document.rootVisualElement.panel.visualTree.SendEvent(evt); }

            inputResultsComp.Received().PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, internalCompData.model);

            var result = internalCompData.model.events.Dequeue();
            Assert.AreEqual(InputAction.IaPointer, result.button);
            Assert.AreEqual(50f, result.hit.Position.X);
            Assert.AreEqual(50f, result.hit.Position.Y);
            Assert.AreEqual(0, result.hit.Position.Z);
        }

        private IEnumerator CreateStretchedElement()
        {
            uiElement = new VisualElement
            {
                style =
                {
                    flexGrow = 1f,
                },
            };

            document.rootVisualElement.Add(uiElement);
            document.rootVisualElement.style.width = 100f;
            document.rootVisualElement.style.height = 100f;
            uiElement.StretchToParentSize();

            // layout needs a frame to update, otherwise a zero-sized element will not receive an event
            // as the root will be picked as the best candidate by the events system
            return null;
        }

        [UnityTest]
        public IEnumerator AddInternalInputResult()
        {
            ECSComponentData<InternalUIInputResults> internalCompData = new ECSComponentData<InternalUIInputResults>
            {
                model = new InternalUIInputResults()
            };

            var inputResultsComp = Substitute.For<IInternalECSComponent<InternalUIInputResults>>();
            var result = Substitute.For<IMessage>();

            inputResultsComp.GetFor(scene, entity).Returns(_ => internalCompData);

            yield return CreateStretchedElement();

            callback = UIPointerEventsUtils
               .RegisterFeedback<PointerUpEvent, IMessage>(inputResultsComp, _ => result, scene, entity, uiElement, COMPONENT_ID);

            using (var evt = PointerUpEvent.GetPooled(new Event
                   {
                       type = EventType.MouseUp,
                       mousePosition = new UnityEngine.Vector2(50f, 50f)
                   })) { document.rootVisualElement.panel.visualTree.SendEvent(evt); }

            CollectionAssert.Contains(internalCompData.model.Results, new InternalUIInputResults.Result(result, COMPONENT_ID));
        }
    }
}
