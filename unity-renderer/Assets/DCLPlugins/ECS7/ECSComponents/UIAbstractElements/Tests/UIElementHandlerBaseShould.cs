using DCL.ECS7.InternalComponents;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIAbstractElements.Tests
{
    [TestFixture]
    public class UIElementHandlerBaseShould : UIComponentsShouldBase
    {
        private VisualElement uiElement;
        private UIElementHandlerBase handler;
        private const int COMPONENT_ID = 1001;

        [SetUp]
        public void HandlerSetUp()
        {
            uiElement = new VisualElement();
            handler = Substitute.For<UIElementHandlerBase>(internalUiContainer, COMPONENT_ID);
        }

        [Test]
        public void AddElementToRoot()
        {
            handler.AddElementToRoot(scene, entity, uiElement);

            internalUiContainer.Received(1)
                               .PutFor(scene, entity, Arg.Is<InternalUiContainer>(i => i.rootElement.Contains(uiElement)
                                                                                       && i.components.Contains(COMPONENT_ID)));
        }

        [Test]
        public void RemoveElementFromRoot()
        {
            handler.AddElementToRoot(scene, entity, uiElement);
            internalUiContainer.ClearReceivedCalls();

            handler.RemoveElementFromRoot(scene, entity, uiElement);

            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                    Arg.Is<InternalUiContainer>(i => i.rootElement.childCount == 0
                                                                     && i.components.Count == 0));
        }

        [Test]
        public void RemoveComponentFromRoot()
        {
            var containerModel = new InternalUiContainer(entity.entityId);
            containerModel.components.Add(COMPONENT_ID);
            internalUiContainer.PutFor(scene, entity, containerModel);

            internalUiContainer.ClearReceivedCalls();

            handler.RemoveComponentFromEntity(scene, entity);

            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                    Arg.Is<InternalUiContainer>(i => i.components.Count == 0));
        }
    }
}
