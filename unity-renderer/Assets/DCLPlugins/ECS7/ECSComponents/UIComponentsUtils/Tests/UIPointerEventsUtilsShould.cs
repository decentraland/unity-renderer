using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements.Tests;
using DCL.ECSRuntime;
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
            (
                scene: scene,
                entity: entity,
                model: new InternalUIInputResults(new Queue<InternalUIInputResults.Result>()),
                handler: null
            );

            var inputResultsComp = Substitute.For<IInternalECSComponent<InternalUIInputResults>>();
            IPooledWrappedComponent result = Substitute.For<IPooledWrappedComponent>();

            inputResultsComp.GetFor(scene, entity).Returns(_ => internalCompData);

            yield return CreateStretchedElement();

            callback = UIPointerEventsUtils
               .RegisterFeedback<PointerUpEvent>(inputResultsComp, _ => result, scene, entity, uiElement, COMPONENT_ID);

            using (var evt = PointerUpEvent.GetPooled(new Event
                   {
                       type = EventType.MouseUp,
                       mousePosition = new Vector2(50f, 50f)
                   })) { document.rootVisualElement.panel.visualTree.SendEvent(evt); }

            CollectionAssert.Contains(internalCompData.model.Results, new InternalUIInputResults.Result(result, COMPONENT_ID));
        }
    }
}

