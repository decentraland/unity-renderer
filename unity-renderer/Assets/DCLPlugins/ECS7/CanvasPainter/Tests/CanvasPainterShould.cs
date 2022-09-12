using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.UI;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NUnit.Framework;
using NSubstitute;
using NSubstitute.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.ECS7.Tests
{
    public class CanvasPainterShould
    {
        private DataStore_ECS7 dataStoreEcs7;
        private RendererState rendererState;
        private CanvasPainter canvasPainter;
        private ECSComponentsManager componentsManager;
        private ECS7ComponentsComposer componentsComposer;
        private IWorldState worldState;

        [SetUp]
        public void Setup()
        {
            worldState = Substitute.For<IWorldState>();
            dataStoreEcs7 = new DataStore_ECS7();
            dataStoreEcs7.uiDataContainer = Substitute.For<IUIDataContainer>();
            
            rendererState = ScriptableObject.CreateInstance<RendererState>();
            var componentFactory = new ECSComponentsFactory();
            componentsComposer = new ECS7ComponentsComposer(componentFactory, Substitute.For<IECSComponentWriter>(), Substitute.For<IInternalECSComponents>());
            componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            
            canvasPainter = new CanvasPainter(dataStoreEcs7, rendererState,Substitute.For<IUpdateEventHandler>(), componentsManager, worldState);
            rendererState.Set(true);
        }

        [TearDown]
        public void TearDown()
        {
            componentsComposer.Dispose();
            canvasPainter.Dispose();
        }

        [Test]
        public void CreateContainersCorrectly()
        {
            // Arrange
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();
            var entitiesDict = new Dictionary<long, IDCLEntity>();
            entitiesDict.Add(0,entity);
            parcelScene.Configure().entities.Returns(entitiesDict);
      
            var sceneDataContainer = new UISceneDataContainer();
            
            sceneDataContainer.AddUIComponent(entity, CreateUITransformModel());
            canvasPainter.dataContainer.Configure().GetDataContainer(parcelScene).Returns(sceneDataContainer);
            canvasPainter.SetScene(parcelScene);
            
            // Act
            canvasPainter.CreateContainers(new List<long>());
            
            // Assert
            Assert.True(canvasPainter.visualElements.Count > 0);
        }
        
        [Test]
        public void DrawTheCorrectSceneUI()
        {
            // Arrange
            var initialParcelScene = Substitute.For<IParcelScene>();
            initialParcelScene.Configure().sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
            {
                id = "Oldscene"
            });
            var finalParcelScene = Substitute.For<IParcelScene>();
            finalParcelScene.Configure().sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
            {
                id = "Newscene"
            });
            
            var initialSceneDataContainer = new UISceneDataContainer();
            var finalSceneDataContainer = new UISceneDataContainer();
            
            var loadedScenes = new Dictionary<string, IParcelScene>();
            loadedScenes.Add("Oldscene", initialParcelScene);
            loadedScenes.Add("Newscene", finalParcelScene);
            
            dataStoreEcs7.scenes.Add(initialParcelScene);
            dataStoreEcs7.scenes.Add(finalParcelScene);
            
            canvasPainter.dataContainer.Configure().GetDataContainer(initialParcelScene).Returns(initialSceneDataContainer);
            canvasPainter.dataContainer.Configure().GetDataContainer(finalParcelScene).Returns(finalSceneDataContainer);
            canvasPainter.SetScene(initialParcelScene);
            canvasPainter.framesCounter = 9999;

            worldState.Configure().GetLoadedScenes().Returns(loadedScenes);
            worldState.Configure().GetCurrentSceneId().Returns("Newscene");
            
            // Act
            canvasPainter.Update();

            // Assert
            Assert.AreEqual(finalSceneDataContainer, canvasPainter.sceneDataContainerToUse);
        }

        [Test]
        public void RemoveVisualElementCorrectly()
        {
            // Arrange
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(5);
            var sceneDataContainer = new UISceneDataContainer();
            
            sceneDataContainer.AddUIComponent(entity, CreateUITransformModel());
            canvasPainter.dataContainer.Configure().GetDataContainer(parcelScene).Returns(sceneDataContainer);
            canvasPainter.SetScene(parcelScene);
            
            var visualElement = new Image();
            var visualElementRepresentation = new VisualElementRepresentation()
            {
                entityId = 5,
                parentId = SpecialEntityId.SCENE_ROOT_ENTITY,
                visualElement =  visualElement
            };
            
            canvasPainter.visualElements.Add(5, visualElementRepresentation);
            
            // Act
            sceneDataContainer.RemoveUITransform(entity);
            
            // Assert
            Assert.False(canvasPainter.visualElements.ContainsKey(5));
        }
        
        [Test]
        public void SetSceneCorrectly()
        {
            // Arrange
            var parcelScene = Substitute.For<IParcelScene>();
            var sceneDataContainer = new UISceneDataContainer();
            canvasPainter.dataContainer.Configure().GetDataContainer(parcelScene).Returns(sceneDataContainer);
            
            // Act
            canvasPainter.SetScene(parcelScene);
            
            // Assert
            Assert.AreEqual(sceneDataContainer, canvasPainter.sceneDataContainerToUse);
            Assert.IsTrue(canvasPainter.canvasCleared);
        }
        
        [Test]
        public void SetOrphanElementCorrectly()
        {
            // Arrange
            var childVisualElement = new Image();
            var childVisualElementRepresentation = new VisualElementRepresentation()
            {
                entityId = 5,
                parentId = 345345,
                visualElement =  childVisualElement
            };
            
            // Act
            canvasPainter.SetParent(ref childVisualElementRepresentation, true);
            
            // Assert
            Assert.False(childVisualElement.visible);
            Assert.AreEqual(1, canvasPainter.orphanVisualElements.Count);
        }
        
        [Test]
        public void SetRootParentCorrectly()
        {
            // Arrange
            var childVisualElement = new Image();
            var childVisualElementRepresentation = new VisualElementRepresentation()
            {
                entityId = 5,
                parentId = SpecialEntityId.SCENE_ROOT_ENTITY,
                visualElement =  childVisualElement
            };
            
            // Act
            canvasPainter.SetParent(ref childVisualElementRepresentation, false);
            
            // Assert
            Assert.AreEqual(canvasPainter.rootNode.rootVisualElement, childVisualElementRepresentation.parentVisualElement);
        }
        
        [Test]
        public void SetParentEntityCorrectly()
        {
            // Arrange
            var childVisualElement = new Image();
            var parentVisualElement = new VisualElement();
            var childVisualElementRepresentation = new VisualElementRepresentation()
            {
                entityId = 5,
                parentId = 50,
                visualElement =  childVisualElement
            };
            
            var parentVisualElementRepresentation = new VisualElementRepresentation()
            {
                entityId = 50,
                parentId = SpecialEntityId.SCENE_ROOT_ENTITY,
                visualElement =  parentVisualElement
            };
            
            canvasPainter.visualElements.Add(50, parentVisualElementRepresentation);
            
            // Act
            canvasPainter.SetParent(ref childVisualElementRepresentation, false);

            // Assert
            Assert.AreEqual(parentVisualElement, childVisualElementRepresentation.parentVisualElement);
        }

        [Test]
        public void TranslateUITransformIntoVisualElementCorrectly()
        {
            // Arrange
            var model = CreateUITransformModel();
            
            // Act
            var visualElement = canvasPainter.TransformToVisualElement(model, new Image());

            // Assert
            Assert.AreEqual(visualElement.style.display, canvasPainter.GetDisplay(model.Display));
            Assert.AreEqual(visualElement.style.overflow, canvasPainter.GetOverflow(model.Overflow));
            
            // Flex
            Assert.AreEqual(visualElement.style.flexDirection, canvasPainter.GetFlexDirection(model.FlexDirection));
            Assert.IsTrue(visualElement.style.flexBasis == new Length(model.FlexBasis, canvasPainter.GetUnit(model.FlexBasisUnit)));
            
            Assert.IsTrue(Mathf.Approximately(visualElement.style.flexGrow.value, model.FlexGrow));
            Assert.IsTrue(Mathf.Approximately(visualElement.style.flexShrink.value, model.FlexShrink));
            Assert.AreEqual(visualElement.style.flexWrap, canvasPainter.GetWrap(model.FlexWrap));
            Assert.AreEqual(visualElement.style.position, canvasPainter.GetPosition(model.PositionType));
            
            // Align
            Assert.AreEqual(visualElement.style.alignContent, canvasPainter.GetAlign(model.AlignContent));
            Assert.AreEqual(visualElement.style.alignItems, canvasPainter.GetAlign(model.AlignItems));
            Assert.AreEqual(visualElement.style.alignSelf, canvasPainter.GetAlign(model.AlignSelf));
            
            Assert.AreEqual(visualElement.style.justifyContent, canvasPainter.GetJustify(model.JustifyContent));
            
            // Layout size
            Assert.IsTrue(visualElement.style.height == new Length(model.Height, canvasPainter.GetUnit(model.HeightUnit)));
            Assert.IsTrue(visualElement.style.width == new Length(model.Width, canvasPainter.GetUnit(model.WidthUnit)));
            
            Assert.IsTrue(visualElement.style.maxHeight == new Length(model.MaxHeight, canvasPainter.GetUnit(model.MaxHeightUnit)));
            Assert.IsTrue(visualElement.style.maxWidth == new Length(model.MaxWidth, canvasPainter.GetUnit(model.MaxWidthUnit)));
            
            Assert.IsTrue(visualElement.style.minHeight == new Length(model.MinHeight, canvasPainter.GetUnit(model.MinHeightUnit)));
            Assert.IsTrue(visualElement.style.minWidth == new Length(model.MinWidth, canvasPainter.GetUnit(model.MinWidthUnit)));
            
            // Padding
            Assert.IsTrue(visualElement.style.paddingBottom == new Length(model.PaddingBottom, canvasPainter.GetUnit(model.PaddingBottomUnit)));
            Assert.IsTrue(visualElement.style.paddingLeft == new Length(model.PaddingLeft, canvasPainter.GetUnit(model.PaddingLeftUnit)));
            Assert.IsTrue(visualElement.style.paddingRight == new Length(model.PaddingRight, canvasPainter.GetUnit(model.PaddingRightUnit)));
            Assert.IsTrue(visualElement.style.paddingTop == new Length(model.PaddingTop, canvasPainter.GetUnit(model.PaddingTopUnit)));
            
            // Margins
            Assert.IsTrue(visualElement.style.marginLeft == new Length(model.MarginLeft, canvasPainter.GetUnit(model.MarginLeftUnit)));
            Assert.IsTrue(visualElement.style.marginRight == new Length(model.MarginRight, canvasPainter.GetUnit(model.MarginRightUnit)));
            Assert.IsTrue(visualElement.style.marginBottom == new Length(model.MarginBottom, canvasPainter.GetUnit(model.MarginBottomUnit)));
            Assert.IsTrue(visualElement.style.marginTop == new Length(model.MarginTop, canvasPainter.GetUnit(model.MarginTopUnit)));
            
            // Border
            Assert.IsTrue(Mathf.Approximately(visualElement.style.borderBottomWidth.value, model.BorderBottom));
            Assert.IsTrue(Mathf.Approximately(visualElement.style.borderLeftWidth.value, model.BorderLeft));
            Assert.IsTrue(Mathf.Approximately(visualElement.style.borderRightWidth.value, model.BorderRight));
            Assert.IsTrue(Mathf.Approximately(visualElement.style.borderTopWidth.value, model.BorderTop));
            
            // Position
            Assert.AreEqual(visualElement.style.position, canvasPainter.GetPosition(model.PositionType));
        }

        private PBUiTransform CreateUITransformModel()
        {
            var model = new PBUiTransform();
            model.Parent = 123;
            
            model.Display = YGDisplay.Flex;
            model.Overflow = YGOverflow.Hidden;

            model.FlexDirection = YGFlexDirection.Column;
            model.FlexBasis = 1;
            model.FlexBasisUnit = YGUnit.Point;
            model.FlexGrow = 1;
            model.FlexShrink = 0;
            model.FlexWrap = YGWrap.Wrap;
            model.PositionType = YGPositionType.Absolute;

            model.AlignContent = YGAlign.Center;
            model.AlignItems = YGAlign.Center;
            model.AlignSelf = YGAlign.Center;
            model.JustifyContent = YGJustify.Center;
            
            model.Height = 200;
            model.HeightUnit = YGUnit.Percent;
            model.Width = 200;
            model.WidthUnit = YGUnit.Percent;
            
            model.MinHeight = 200;
            model.MinHeightUnit = YGUnit.Percent;
            model.MinWidth = 200;
            model.MinWidthUnit = YGUnit.Percent;
            
            model.MaxHeight = 200;
            model.MaxHeightUnit = YGUnit.Percent;
            model.MaxWidth = 200;
            model.MaxWidthUnit = YGUnit.Percent;

            model.PaddingBottom = 50;
            model.PaddingBottomUnit = YGUnit.Percent;
            model.PaddingTop = 50;
            model.PaddingTopUnit = YGUnit.Percent;
            model.PaddingRight = 50;
            model.PaddingRightUnit = YGUnit.Percent;
            model.PaddingLeft = 50;
            model.PaddingLeftUnit = YGUnit.Percent;

            model.MarginBottom = 50;
            model.MarginBottomUnit = YGUnit.Percent;
            model.MarginLeft = 50;
            model.MarginLeftUnit = YGUnit.Percent;
            model.MarginRight = 50;
            model.MarginRightUnit = YGUnit.Percent;
            model.MarginTop = 50;
            model.MarginTopUnit = YGUnit.Percent;

            model.BorderBottom = 20;
            model.BorderLeft = 20;
            model.BorderRight = 20;
            model.BorderTop = 20;

            model.PositionType = YGPositionType.Absolute;
            
            return model;
        }
    }
}