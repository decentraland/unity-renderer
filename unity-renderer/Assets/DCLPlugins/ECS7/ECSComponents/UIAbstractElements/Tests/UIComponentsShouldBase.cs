using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace DCL.ECSComponents.UIAbstractElements.Tests
{
    public abstract class UIComponentsShouldBase
    {
        protected ECS7TestEntity entity;
        protected ECS7TestScene scene;
        protected ECS7TestUtilsScenesAndEntities sceneTestHelper;

        protected IInternalECSComponent<InternalUiContainer> internalUiContainer;
        protected IInternalECSComponent<InternalUIInputResults> uiInputResultsComponent;
        protected InternalUIInputResults uiInputResults;

        [SetUp]
        public void SetUp()
        {
            sceneTestHelper = new ECS7TestUtilsScenesAndEntities();
            scene = sceneTestHelper.CreateScene(666);
            entity = scene.CreateEntity(1111);

            uiInputResultsComponent = Substitute.For<IInternalECSComponent<InternalUIInputResults>>();
            uiInputResults = new InternalUIInputResults(new Queue<InternalUIInputResults.Result>());

            uiInputResultsComponent.GetFor(scene, entity).Returns(_ =>
            {
                return new ECSComponentData<InternalUIInputResults>(
                    scene,
                    entity,
                    uiInputResults,
                    null
                    );
            });

            var uiDoc = InstantiateUiDocument();

            ECSComponentData<InternalUiContainer>? internalCompData = null;
            internalUiContainer = Substitute.For<IInternalECSComponent<InternalUiContainer>>();
            internalUiContainer.GetFor(scene, entity).Returns((info) => internalCompData);
            internalUiContainer.WhenForAnyArgs(
                                    x => x.PutFor(scene, entity, Arg.Any<InternalUiContainer>()))
                               .Do(info =>
                                {
                                    internalCompData = new ECSComponentData<InternalUiContainer>
                                    (
                                        scene: info.ArgAt<IParcelScene>(0),
                                        entity: info.ArgAt<IDCLEntity>(1),
                                        model: info.ArgAt<InternalUiContainer>(2),
                                        handler: null
                                    );

                                    // to test events we need a panel, panel comes from `UIDocument`
                                    if (!uiDoc.rootVisualElement.Contains(internalCompData.Value.model.rootElement))
                                        uiDoc.rootVisualElement.Add(internalCompData.Value.model.rootElement);
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
