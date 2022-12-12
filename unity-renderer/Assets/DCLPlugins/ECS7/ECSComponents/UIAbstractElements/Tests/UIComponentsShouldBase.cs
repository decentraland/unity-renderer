using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;

namespace DCL.ECSComponents.UIAbstractElements.Tests
{
    public class UIComponentsShouldBase
    {
        protected ECS7TestEntity entity;
        protected ECS7TestScene scene;
        protected ECS7TestUtilsScenesAndEntities sceneTestHelper;

        protected IInternalECSComponent<InternalUiContainer> internalUiContainer;

        [SetUp]
        public void SetUp()
        {
            sceneTestHelper = new ECS7TestUtilsScenesAndEntities();
            scene = sceneTestHelper.CreateScene(666);
            entity = scene.CreateEntity(1111);

            ECSComponentData<InternalUiContainer> internalCompData = null;
            internalUiContainer = Substitute.For<IInternalECSComponent<InternalUiContainer>>();
            internalUiContainer.GetFor(scene, entity).Returns((info) => internalCompData);
            internalUiContainer.WhenForAnyArgs(
                                    x => x.PutFor(scene, entity, Arg.Any<InternalUiContainer>()))
                               .Do(info =>
                                {
                                    internalCompData ??= new ECSComponentData<InternalUiContainer>
                                    {
                                        scene = info.ArgAt<IParcelScene>(0),
                                        entity = info.ArgAt<IDCLEntity>(1)
                                    };
                                    internalCompData.model = info.ArgAt<InternalUiContainer>(2);
                                });
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }

        /*protected static void CreateUiContainer<T>(IECSComponentHandler<T> handler, string componentId)
        {
            handler.OnComponentCreated(scene, entity);

            Assert.IsNotNull(handler.uiElement);
            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                    Arg.Is<InternalUiContainer>(i => i.rootElement.Contains(handler.uiElement)
                                                                     && i.components.Contains(COMPONENT_ID)));
        }*/
    }
}
