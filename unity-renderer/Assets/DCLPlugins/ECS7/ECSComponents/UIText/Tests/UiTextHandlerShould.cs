using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSComponents.UIText;
using DCL.ECSRuntime;
using DCL.Models;
using Decentraland.Common;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Position = UnityEngine.UIElements.Position;

namespace Tests
{
    public class UiTextHandlerShould
    {
        private const int COMPONENT_ID = 34;

        private ECS7TestEntity entity;
        private ECS7TestScene scene;
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;

        private IInternalECSComponent<InternalUiContainer> internalUiContainer;
        private UiTextHandler handler;

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

            handler = new UiTextHandler(internalUiContainer, AssetPromiseKeeper_Font.i, COMPONENT_ID);
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
            AssetPromiseKeeper_Font.i.Cleanup();
        }

        [Test]
        public void CreateUiContainer()
        {
            handler.OnComponentCreated(scene, entity);

            Assert.IsNotNull(handler.uiElement);
            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(i => i.rootElement.Contains(handler.uiElement)
                                                                    && i.components.Contains(COMPONENT_ID)));
        }

        [Test]
        public void UpdateTextUiElement()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBUiText()
            {
                Value = "temptation",
                FontSize = 34,
                Color = new Color4() { R = 0.1f, G = 0.2f, B = 0.3f, A = 1 },
                TextAlign = TextAlignMode.TamMiddleRight
            });

            Assert.AreEqual("temptation", handler.uiElement.text);
            Assert.AreEqual(new StyleLength(new Length(34, LengthUnit.Pixel)), handler.uiElement.style.fontSize);
            Assert.AreEqual(new Color(0.1f, 0.2f, 0.3f, 1), handler.uiElement.style.color.value);
            Assert.AreEqual(TextAnchor.MiddleRight, handler.uiElement.style.unityTextAlign.value);
        }

        [Test]
        public void RemoveCorrectly()
        {
            handler.OnComponentCreated(scene, entity);
            internalUiContainer.ClearReceivedCalls();

            handler.OnComponentRemoved(scene, entity);

            Assert.IsNull(handler.uiElement);
            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(i => i.rootElement.childCount == 0
                                                                    && i.components.Count == 0));
        }
    }
}
