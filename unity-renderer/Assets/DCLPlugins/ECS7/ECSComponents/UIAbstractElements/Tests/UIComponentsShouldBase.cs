using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIAbstractElements.Tests
{
    public abstract class UIComponentsShouldBase
    {
        protected ECS7TestEntity entity;
        protected ECS7TestScene scene;
        protected ECS7TestUtilsScenesAndEntities sceneTestHelper;

        protected IInternalECSComponent<InternalUiContainer> internalUiContainer;
        protected IInternalECSComponent<InternalUIInputResults> uiInputResults;

        [SetUp]
        public void SetUp()
        {
            sceneTestHelper = new ECS7TestUtilsScenesAndEntities();
            scene = sceneTestHelper.CreateScene(666);
            entity = scene.CreateEntity(1111);

            uiInputResults = Substitute.For<IInternalECSComponent<InternalUIInputResults>>();

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

        protected static UIDocument InstantiateUiDocument() =>
            Object.Instantiate(Resources.Load<UIDocument>("ScenesUI"));

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }
    }
}
