using System;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Tests
{
    public class UITransformHandlerShould
    {
        const int COMPONENT_ID = 34;

        private ECS7TestEntity entity;
        private ECS7TestScene scene;
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;

        private IInternalECSComponent<InternalUiContainer> internalUiContainer;
        private UITransformHandler handler;

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

            handler = new UITransformHandler(internalUiContainer, COMPONENT_ID);
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }

        [Test]
        public void SetTransform()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBUiTransform() { Parent = 123123 });
            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(model => model.parentId == 123123 && model.components.Contains(COMPONENT_ID)));
        }

        [Test]
        public void SetUiElementStyle()
        {
            var model = new PBUiTransform()
            {
                Display = YGDisplay.YgdFlex,
                Overflow = YGOverflow.YgoVisible,
                FlexDirection = YGFlexDirection.YgfdColumnReverse,
                FlexBasis = float.NaN,
                FlexGrow = 23,
                FlexShrink = 1,
                FlexWrap = YGWrap.YgwWrapReverse,
                AlignContent = YGAlign.YgaCenter,
                AlignItems = YGAlign.YgaStretch,
                AlignSelf = YGAlign.YgaCenter,
                JustifyContent = YGJustify.YgjSpaceAround,
                Height = 99,
                Width = 34,
                MaxWidth = float.NaN,
                MaxHeight = 10,
                MinHeight = 0,
                MinWidth = 0,
                PaddingBottom = 10,
                PaddingBottomUnit = YGUnit.YguPercent,
                PaddingLeft = 0,
                PaddingLeftUnit = YGUnit.YguPoint,
                PaddingRight = 111,
                PaddingRightUnit = YGUnit.YguPoint,
                PaddingTop = 5,
                PaddingTopUnit = YGUnit.YguPercent,
                MarginBottom = 10,
                MarginBottomUnit = YGUnit.YguPercent,
                MarginLeft = 0,
                MarginLeftUnit = YGUnit.YguPoint,
                MarginRight = 111,
                MarginRightUnit = YGUnit.YguPoint,
                MarginTop = 5,
                MarginTopUnit = YGUnit.YguPercent,
                BorderBottom = 1,
                BorderTop = 2,
                BorderRight = 3,
                BorderLeft = 4,
                PositionType = YGPositionType.YgptAbsolute
            };

            Action<InternalUiContainer> styleCheck = m =>
            {
                var style = m.rootElement.style;
                Assert.AreEqual(DisplayStyle.Flex, style.display.value);
                Assert.AreEqual(Overflow.Hidden, style.overflow.value);
                Assert.AreEqual(FlexDirection.ColumnReverse, style.flexDirection.value);
                Assert.AreEqual(StyleKeyword.Auto, style.flexBasis.keyword);
                Assert.AreEqual(23, style.flexGrow.value);
                Assert.AreEqual(1, style.flexShrink.value);
                Assert.AreEqual(Wrap.WrapReverse, style.flexWrap.value);
                Assert.AreEqual(Align.Center, style.alignContent.value);
                Assert.AreEqual(Align.Stretch, style.alignItems.value);
                Assert.AreEqual(Align.Center, style.alignSelf.value);
                Assert.AreEqual(Justify.SpaceAround, style.justifyContent.value);
                Assert.AreEqual(99, style.height.value.value);
                Assert.AreEqual(32, style.width.value.value);
                Assert.AreEqual(StyleKeyword.Auto, style.maxWidth.keyword);
                Assert.AreEqual(10, style.maxHeight.value.value);
                Assert.AreEqual(0, style.minHeight.value.value);
                Assert.AreEqual(0, style.minWidth.value.value);
                Assert.AreEqual(10, style.paddingBottom.value.value);
                Assert.AreEqual(LengthUnit.Percent, style.paddingBottom.value.unit);
                Assert.AreEqual(0, style.paddingLeft.value.value);
                Assert.AreEqual(LengthUnit.Pixel, style.paddingLeft.value.unit);
                Assert.AreEqual(111, style.paddingRight.value.value);
                Assert.AreEqual(LengthUnit.Pixel, style.paddingRight.value.unit);
                Assert.AreEqual(5, style.paddingTop.value.value);
                Assert.AreEqual(LengthUnit.Percent, style.paddingTop.value.unit);
                Assert.AreEqual(10, style.marginBottom.value.value);
                Assert.AreEqual(LengthUnit.Percent, style.marginBottom.value.unit);
                Assert.AreEqual(0, style.marginLeft.value.value);
                Assert.AreEqual(LengthUnit.Pixel, style.marginLeft.value.unit);
                Assert.AreEqual(111, style.marginRight.value.value);
                Assert.AreEqual(LengthUnit.Pixel, style.marginRight.value.unit);
                Assert.AreEqual(5, style.marginTop.value.value);
                Assert.AreEqual(LengthUnit.Percent, style.marginTop.value.unit);
                Assert.AreEqual(1, style.borderBottomWidth.value);
                Assert.AreEqual(2, style.borderTopWidth.value);
                Assert.AreEqual(3, style.borderRightWidth.value);
                Assert.AreEqual(4, style.borderLeftWidth.value);
                Assert.AreEqual(UnityEngine.UIElements.Position.Absolute, style.position.value);
            };

            handler.OnComponentModelUpdated(scene, entity, model);

            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Do<InternalUiContainer>(m => styleCheck(m)));
        }

        [Test]
        public void RemoveParenting()
        {
            VisualElement parent = new VisualElement();
            var containerModel = new InternalUiContainer()
            {
                parentId = 2,
                parentElement = parent
            };
            containerModel.components.Add(COMPONENT_ID);
            parent.Add(containerModel.rootElement);
            internalUiContainer.PutFor(scene, entity, containerModel);
            internalUiContainer.ClearReceivedCalls();

            handler.OnComponentRemoved(scene, entity);
            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(
                                       model => model.parentId == SpecialEntityId.SCENE_ROOT_ENTITY
                                                && !model.components.Contains(COMPONENT_ID)
                                                && model.parentElement == null));
            Assert.AreEqual(0, parent.childCount);
        }
    }
}