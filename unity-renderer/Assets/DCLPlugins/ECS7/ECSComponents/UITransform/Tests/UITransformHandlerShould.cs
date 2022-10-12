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

            handler = new UITransformHandler(internalUiContainer);
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
                                   Arg.Is<InternalUiContainer>(model => model.parentId == 123123 && model.hasTransform));
        }

        [Test]
        public void SetUiElementStyle()
        {
            var model = new PBUiTransform()
            {
                Display = YGDisplay.Flex,
                Overflow = YGOverflow.Visible,
                FlexDirection = YGFlexDirection.ColumnReverse,
                FlexBasis = float.NaN,
                FlexGrow = 23,
                FlexShrink = 1,
                FlexWrap = YGWrap.WrapReverse,
                AlignContent = YGAlign.Center,
                AlignItems = YGAlign.Stretch,
                AlignSelf = YGAlign.Center,
                JustifyContent = YGJustify.SpaceAround,
                Height = 99,
                Width = 34,
                MaxWidth = float.NaN,
                MaxHeight = 10,
                MinHeight = 0,
                MinWidth = 0,
                PaddingBottom = 10,
                PaddingBottomUnit = YGUnit.Percent,
                PaddingLeft = 0,
                PaddingLeftUnit = YGUnit.Point,
                PaddingRight = 111,
                PaddingRightUnit = YGUnit.Point,
                PaddingTop = 5,
                PaddingTopUnit = YGUnit.Percent,
                MarginBottom = 10,
                MarginBottomUnit = YGUnit.Percent,
                MarginLeft = 0,
                MarginLeftUnit = YGUnit.Point,
                MarginRight = 111,
                MarginRightUnit = YGUnit.Point,
                MarginTop = 5,
                MarginTopUnit = YGUnit.Percent,
                BorderBottom = 1,
                BorderTop = 2,
                BorderRight = 3,
                BorderLeft = 4,
                PositionType = YGPositionType.Absolute
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
                Assert.AreEqual(Position.Absolute, style.position.value);
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
                hasTransform = true,
                parentElement = parent
            };
            parent.Add(containerModel.rootElement);
            internalUiContainer.PutFor(scene, entity, containerModel);
            internalUiContainer.ClearReceivedCalls();

            handler.OnComponentRemoved(scene, entity);
            internalUiContainer.Received(1)
                               .PutFor(scene, entity,
                                   Arg.Is<InternalUiContainer>(
                                       model => model.parentId == SpecialEntityId.SCENE_ROOT_ENTITY
                                                && !model.hasTransform
                                                && model.parentElement == null));
            Assert.AreEqual(0, parent.childCount);
        }
    }
}