using System.Collections;
using System.Linq;
using DCL.CRDT;
using DCL.ECS7.UI;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using DCLPlugins.ECS7.TestsUtils;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace DCL.ECS7.Tests
{
    public class CanvasPainterUITransformIntegrationTest 
    {
        private RendererState rendererState;
        private CanvasPainter canvasPainter;
        private ECSComponentsManager componentsManager;
        private ECS7ComponentsComposer componentsComposer;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private IWorldState worldState;
        
        [SetUp]
        protected void SetUp()
        {
            worldState = Substitute.For<IWorldState>();
            
            rendererState = ScriptableObject.CreateInstance<RendererState>();
            var componentFactory = new ECSComponentsFactory();
            componentsComposer = new ECS7ComponentsComposer(componentFactory, Substitute.For<IECSComponentWriter>(), Substitute.For<IInternalECSComponents>());
            componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
            canvasPainter = new CanvasPainter(DataStore.i.ecs7, rendererState,Substitute.For<IUpdateEventHandler>(), componentsManager, worldState);
            rendererState.Set(true);
            canvasPainter.rootNode.rootVisualElement.style.justifyContent = new StyleEnum<Justify>(Justify.Center);
            canvasPainter.rootNode.rootVisualElement.style.alignContent = new StyleEnum<Align>(Align.Center);
        }

        [TearDown]
        protected void TearDown()
        {
            componentsComposer.Dispose();
            canvasPainter.Dispose();
            DataStore.i.ecs7.scenes.Clear();
            DataStore.i.ecs7.uiDataContainer = new UIDataContainer();
        }

        [Test]
        public void DrawListOfItemCorrectly()
        {
            int parentId = 512;
            int childIdStartIndex = 513;
            int amountOfChildren = 7;
            var testScene = CreateUITestScene();

            var parentModel = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            parentModel.Width = 300;
            parentModel.Height = 700;
            parentModel.FlexDirection = YGFlexDirection.Column;
            CreateAndExecuteCRDTMessage(testScene,parentId, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(parentModel));

            PBUiTransform childModel = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            for (int i = 0; i <= amountOfChildren; i++)
            {
                int childId = childIdStartIndex + i;
                childModel.Parent = parentId;
                childModel.Height = 20;
            
                CreateAndExecuteCRDTMessage(testScene, childId, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(childModel));
            }

            DrawUI();
            
            // Assert parent
            VisualElementRepresentation visualElementRepresentation = canvasPainter.visualElements[parentId];
            
            Assert.IsTrue(visualElementRepresentation.parentId == SpecialEntityId.SCENE_ROOT_ENTITY);
            Assert.IsTrue(visualElementRepresentation.visualElement.style.flexDirection == canvasPainter.GetFlexDirection(parentModel.FlexDirection));

            AssertWidth(parentModel, visualElementRepresentation.visualElement);
            AssertHeight(parentModel, visualElementRepresentation.visualElement);
            
            // Assert childrens
            for (int i = 0; i <= amountOfChildren; i++)
            {
                int childId = childIdStartIndex + i;
                Assert.IsTrue(canvasPainter.visualElements[childId].parentId == parentId);
                
                AssertHeight(childModel, canvasPainter.visualElements[childId].visualElement);
            }
        }

        [Test]
        public void DrawLayoutCorrectly()
        {
            int parentId = 512;
            var testScene = CreateUITestScene();

            var parentModel = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            parentModel.Width = 300;
            parentModel.Height = 700;
            
            parentModel.MaxWidth = 300;
            parentModel.MaxWidthUnit = YGUnit.Percent;
            parentModel.MaxHeight = 700;
            parentModel.MaxHeightUnit = YGUnit.Percent;
            
            parentModel.MinWidth = 100;
            parentModel.MinWidthUnit = YGUnit.Percent;
            parentModel.MinHeight = 100;
            parentModel.MinHeightUnit = YGUnit.Percent;

            parentModel.PaddingBottom = 20;
            parentModel.PaddingBottomUnit = YGUnit.Percent;
            parentModel.PaddingTop = 20;
            parentModel.PaddingTopUnit = YGUnit.Percent;
            parentModel.PaddingLeft = 20;
            parentModel.PaddingLeftUnit = YGUnit.Percent;
            parentModel.PaddingRight = 20;
            parentModel.PaddingRightUnit = YGUnit.Percent;
            
            parentModel.MarginBottom = 20;
            parentModel.MarginBottomUnit = YGUnit.Percent;
            parentModel.MarginTop = 20;
            parentModel.MarginTopUnit = YGUnit.Percent;
            parentModel.MarginLeft = 20;
            parentModel.MarginLeftUnit = YGUnit.Percent;
            parentModel.MarginRight = 20;
            parentModel.MarginRightUnit = YGUnit.Percent;    
            
            parentModel.BorderBottom = 20;
            parentModel.BorderTop = 20;
            parentModel.BorderLeft = 20;
            parentModel.BorderRight = 20;
            
            CreateAndExecuteCRDTMessage(testScene,parentId, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(parentModel));
            
            DrawUI();
            
            // Assert
            VisualElement visualElement = canvasPainter.visualElements[parentId].visualElement;
            IStyle style = visualElement.style;
            
            Assert.IsTrue(canvasPainter.visualElements[parentId].parentId == SpecialEntityId.SCENE_ROOT_ENTITY);
            
            AssertWidth(parentModel, visualElement);
            AssertHeight(parentModel, visualElement);
            
            AssertLenght(parentModel.MaxHeight, canvasPainter.GetUnit(parentModel.MaxHeightUnit), style.maxHeight);
            AssertLenght(parentModel.MaxWidth, canvasPainter.GetUnit(parentModel.MaxWidthUnit), style.maxWidth);
               
            AssertLenght(parentModel.MinHeight, canvasPainter.GetUnit(parentModel.MinHeightUnit), style.minHeight);
            AssertLenght(parentModel.MinWidth, canvasPainter.GetUnit(parentModel.MinWidthUnit), style.minWidth);
            
            AssertLenght(parentModel.PaddingBottom, canvasPainter.GetUnit(parentModel.PaddingBottomUnit), style.paddingBottom);
            AssertLenght(parentModel.PaddingTop, canvasPainter.GetUnit(parentModel.PaddingTopUnit), style.paddingTop);
            AssertLenght(parentModel.PaddingLeft, canvasPainter.GetUnit(parentModel.PaddingLeftUnit), style.paddingLeft);
            AssertLenght(parentModel.PaddingRight, canvasPainter.GetUnit(parentModel.PaddingRightUnit), style.paddingRight);
          
            AssertLenght(parentModel.MarginBottom, canvasPainter.GetUnit(parentModel.MarginBottomUnit), style.marginBottom);
            AssertLenght(parentModel.MarginTop, canvasPainter.GetUnit(parentModel.MarginTopUnit), style.marginTop);
            AssertLenght(parentModel.MarginLeft, canvasPainter.GetUnit(parentModel.MarginLeftUnit), style.marginLeft);
            AssertLenght(parentModel.MarginRight, canvasPainter.GetUnit(parentModel.MarginRightUnit), style.marginRight);

            Assert.IsTrue(Mathf.Approximately(style.borderBottomWidth.value, parentModel.BorderBottom));
            Assert.IsTrue(Mathf.Approximately(style.borderTopWidth.value, parentModel.BorderTop));
            Assert.IsTrue(Mathf.Approximately(style.borderLeftWidth.value, parentModel.BorderLeft));
            Assert.IsTrue(Mathf.Approximately(style.borderRightWidth.value, parentModel.BorderRight));
        }

        [Test]
        public void DrawPercentPositionCorrectly()
        {
            int parentId = 512;
            int childIdStartIndex = 513;
            int amountOfChildren = 3;
            var testScene = CreateUITestScene();

            var parentModel = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            parentModel.Width = 300;
            parentModel.Height = 700;
            parentModel.FlexDirection = YGFlexDirection.RowReverse;
            parentModel.AlignContent = YGAlign.Center;
            parentModel.FlexWrap = YGWrap.WrapReverse;
            CreateAndExecuteCRDTMessage(testScene,parentId, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(parentModel));

            PBUiTransform childModel = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            for (int i = 0; i <= amountOfChildren; i++)
            {
                int childId = childIdStartIndex + i;
                childModel.Parent = parentId;
                childModel.FlexGrow = 1;
                childModel.FlexShrink = 0;
                childModel.FlexBasis = 200;
                childModel.FlexBasisUnit = YGUnit.Percent;
                
                childModel.Height = 20;
                childModel.HeightUnit = YGUnit.Percent;
                
                childModel.Width = 30;
                childModel.WidthUnit = YGUnit.Percent;
                
                childModel.AlignSelf = YGAlign.Center;
            
                CreateAndExecuteCRDTMessage(testScene, childId, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(childModel));
            }

            DrawUI();
            
            // Assert parent
            VisualElementRepresentation parentVisualElementRepresentation = canvasPainter.visualElements[parentId];
            
            Assert.IsTrue(parentVisualElementRepresentation.parentId == SpecialEntityId.SCENE_ROOT_ENTITY);
            Assert.IsTrue(parentVisualElementRepresentation.visualElement.style.flexDirection == canvasPainter.GetFlexDirection(parentModel.FlexDirection));
            Assert.IsTrue(parentVisualElementRepresentation.visualElement.style.alignContent == canvasPainter.GetAlign(parentModel.AlignContent));
            Assert.IsTrue(parentVisualElementRepresentation.visualElement.style.flexWrap == canvasPainter.GetWrap(parentModel.FlexWrap));

            AssertWidth(parentModel, parentVisualElementRepresentation.visualElement);
            AssertHeight(parentModel, parentVisualElementRepresentation.visualElement);
            
            // Assert childrens
            for (int i = 0; i <= amountOfChildren; i++)
            {
                int childId = childIdStartIndex + i;
                VisualElementRepresentation childVisualElementRepresentation = canvasPainter.visualElements[childId];
                IStyle childStyle = childVisualElementRepresentation.visualElement.style;
                
                Assert.IsTrue(childVisualElementRepresentation.parentId == parentId);
                Assert.IsTrue(childStyle.alignSelf == canvasPainter.GetAlign(childModel.AlignSelf));

                Assert.IsTrue(Mathf.Approximately(childStyle.flexGrow.value ,childModel.FlexGrow));
                Assert.IsTrue(Mathf.Approximately(childStyle.flexShrink.value ,childModel.FlexShrink));

                AssertLenght(childModel.FlexBasis, canvasPainter.GetUnit(childModel.FlexBasisUnit), canvasPainter.visualElements[childId].visualElement.style.flexBasis);
                AssertWidth(childModel, childVisualElementRepresentation.visualElement);
                AssertHeight(childModel, childVisualElementRepresentation.visualElement);
            }
        }

        [Test]
        public void DrawParentAndChildrenCorrectly()
        {
            int parentId = 512;
            int childId = 513;
            var testScene = CreateUITestScene();

            var parentModel = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            parentModel.AlignItems = YGAlign.Center;
            parentModel.JustifyContent = YGJustify.Center;
            var childModel = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            childModel.Parent = parentId;
            childModel.Width = 400;
            childModel.Height = 400;
            
            CreateAndExecuteCRDTMessage(testScene,parentId, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(parentModel));
            CreateAndExecuteCRDTMessage(testScene, childId, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(childModel));

            DrawUI();
            
            // Assert parent
            VisualElementRepresentation parentVisualElementRepresentation = canvasPainter.visualElements[parentId];
            IStyle parentStyle = parentVisualElementRepresentation.visualElement.style;
            
            Assert.IsTrue(parentVisualElementRepresentation.parentId == SpecialEntityId.SCENE_ROOT_ENTITY);
            Assert.IsTrue(parentStyle.justifyContent == canvasPainter.GetJustify(parentModel.JustifyContent));
            Assert.IsTrue(parentStyle.alignItems == canvasPainter.GetAlign(parentModel.AlignItems));

            AssertWidth(parentModel, parentVisualElementRepresentation.visualElement);
            AssertHeight(parentModel, parentVisualElementRepresentation.visualElement);
            
            // Assert children
            VisualElementRepresentation childVisualElementRepresentation = canvasPainter.visualElements[childId];
            
            Assert.IsTrue(childVisualElementRepresentation.parentId == parentId);
            
            AssertWidth(childModel, childVisualElementRepresentation.visualElement);
            AssertHeight(childModel, childVisualElementRepresentation.visualElement);
        }

        private void AssertHeight(PBUiTransform model, VisualElement visualElement)
        {
            AssertLenght(model.Height, canvasPainter.GetUnit(model.HeightUnit), visualElement.style.height);
        }

        private void AssertWidth(PBUiTransform model, VisualElement visualElement)
        {
            AssertLenght(model.Width, canvasPainter.GetUnit(model.WidthUnit), visualElement.style.width);
        }

        private void AssertLenght(float value, LengthUnit unit, StyleLength length )
        {
            Assert.IsTrue(Mathf.Approximately(length.value.value, value));
            Assert.IsTrue(unit == length.value.unit);
        }

        private ECS7TestScene CreateUITestScene()
        {
            string testSceneId = "testId";
            worldState.Configure().GetCurrentSceneId().Returns(testSceneId);
            var testScene = testUtils.CreateScene(testSceneId);
            testScene.CreateEntity(512);
            testScene.CreateEntity(513);
            DataStore.i.ecs7.scenes.Add(testScene);
            return testScene;
        }

        private void DrawUI()
        {           
            canvasPainter.framesCounter = 9999;
            canvasPainter.Update();
        }

        private void CreateAndExecuteCRDTMessage(ECS7TestScene testScene, int entityId, int componentId, object data)
        {
            CRDTMessage message = new CRDTMessage()
            {
                key1 = entityId,
                key2 = componentId,
                data = data,
                timestamp = 1
            };
            testScene.crdtExecutor.Execute(message);
        }
    }
}