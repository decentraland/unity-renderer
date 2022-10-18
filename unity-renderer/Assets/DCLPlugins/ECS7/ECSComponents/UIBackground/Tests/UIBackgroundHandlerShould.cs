using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class UIBackgroundHandlerShould
    {
        const int COMPONENT_ID = 34;

        private ECS7TestEntity entity;
        private ECS7TestScene scene;
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;

        private IInternalECSComponent<InternalUiContainer> internalUiContainer;
        private UIBackgroundHandler handler;

        [SetUp]
        public void SetUp()
        {
            sceneTestHelper = new ECS7TestUtilsScenesAndEntities();
            scene = sceneTestHelper.CreateScene("temptation");
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

            handler = new UIBackgroundHandler(internalUiContainer, COMPONENT_ID);
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }

        [Test]
        public void UpdateUiStyle()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBUiBackground()
            {
                BackgroundColor = new Color4()
                {
                    R = 0.1f,
                    G = 0.2f,
                    B = 0.3f,
                    A = 0.4f
                }
            });

            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(i => i.rootElement.style.backgroundColor == new Color(0.1f, 0.2f, 0.3f, 0.4f)
                                                                    && i.components.Contains(COMPONENT_ID)));
        }

        [Test]
        public void RemoveFromUiWhenFullyTransparent()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBUiBackground()
            {
                BackgroundColor = new Color4()
                {
                    R = 0.1f,
                    G = 0.2f,
                    B = 0.3f,
                    A = 0.4f
                }
            });
            internalUiContainer.ClearReceivedCalls();

            handler.OnComponentModelUpdated(scene, entity, new PBUiBackground()
            {
                BackgroundColor = new Color4()
                {
                    R = 0.1f,
                    G = 0.2f,
                    B = 0.3f,
                    A = 0
                }
            });
            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(i => i.rootElement.style.backgroundColor == Color.clear
                                                                    && !i.components.Contains(COMPONENT_ID)));
        }

        [Test]
        public void RemoveFromUiWhenNoColorSet()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBUiBackground()
            {
                BackgroundColor = new Color4()
                {
                    R = 0.1f,
                    G = 0.2f,
                    B = 0.3f,
                    A = 0.4f
                }
            });
            internalUiContainer.ClearReceivedCalls();

            handler.OnComponentModelUpdated(scene, entity, new PBUiBackground());

            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(i => i.rootElement.style.backgroundColor == Color.clear
                                                                    && !i.components.Contains(COMPONENT_ID)));
        }

        [Test]
        public void RemoveComponentCorrectly()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBUiBackground()
            {
                BackgroundColor = new Color4()
                {
                    R = 0.1f,
                    G = 0.2f,
                    B = 0.3f,
                    A = 0.4f
                }
            });
            internalUiContainer.ClearReceivedCalls();

            handler.OnComponentRemoved(scene, entity);

            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(i => i.rootElement.style.backgroundColor == Color.clear
                                                                    && !i.components.Contains(COMPONENT_ID)));
        }
    }
}