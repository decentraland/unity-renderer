using System.Collections;
using DCL.CRDT;
using DCL.ECS7.UI;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
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
    public class CanvasPainterIntegrationTest : IntegrationTestSuite
    {
        public static string baselineImagesPath =
            Application.dataPath + "/../TestResources/VisualTests/BaselineImages/";
        public static string testImagesPath = Application.dataPath + "/../TestResources/VisualTests/CurrentTestImages/";

        
        private RendererState rendererState;
        private CanvasPainter canvasPainter;
        private ECSComponentsManager componentsManager;
        private ECS7ComponentsComposer componentsComposer;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private IWorldState worldState;
        private GameObject mainGameObject;
        private GameObject canvasGameObject;
        private GameObject childrenGameObject;
        private Camera camera;
        
        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            mainGameObject = new GameObject();
            
            // Configure camera
            camera = mainGameObject.AddComponent<Camera>();
            camera.backgroundColor = Color.white;
            camera.allowHDR = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            
            worldState = Substitute.For<IWorldState>();
            
            rendererState = ScriptableObject.CreateInstance<RendererState>();
            var componentFactory = new ECSComponentsFactory();
            componentsComposer = new ECS7ComponentsComposer(componentFactory, Substitute.For<IECSComponentWriter>());
            componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
            canvasPainter = new CanvasPainter(DataStore.i.ecs7, rendererState,Substitute.For<IUpdateEventHandler>(), componentsManager, worldState);
            rendererState.Set(true);
            canvasPainter.rootNode.rootVisualElement.style.justifyContent = new StyleEnum<Justify>(Justify.Center);
            canvasPainter.rootNode.rootVisualElement.style.alignContent = new StyleEnum<Align>(Align.Center);
            
            // Configure canvas
            canvasGameObject = new GameObject();
            var canvas = canvasGameObject.AddComponent<Canvas>();
            var canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1440, 900);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            
            var canvasRectTransform = canvasGameObject.GetComponent<RectTransform>();
            canvasRectTransform.anchorMin = new Vector2(1, 0);
            canvasRectTransform.anchorMax = new Vector2(0, 1);
            canvasRectTransform.pivot = new Vector2(0.5f, 0.5f);
            canvasRectTransform.sizeDelta = new Vector2(1440, 900);
            yield return null;
            
            // Create render texture
            RenderTexture renderTexture = new RenderTexture(1440, 900, 16);
            renderTexture.Create();
            canvasPainter.rootNode.panelSettings.targetTexture = renderTexture;
            
            // Configure RawImage
            childrenGameObject = new GameObject();
            childrenGameObject.transform.SetParent(canvasGameObject.transform, false);
            var rawImage = childrenGameObject.AddComponent<RawImage>();
            var rawImageRectTransform = rawImage.GetComponent<RectTransform>();
            rawImageRectTransform.anchorMin = new Vector2(0, 0);
            rawImageRectTransform.anchorMax = new Vector2(1, 1);
            rawImageRectTransform.pivot = new Vector2(0.5f, 0.5f);
            rawImageRectTransform.sizeDelta = new Vector2(1440, 900);
            rawImageRectTransform.offsetMax = Vector2.zero;
            rawImageRectTransform.offsetMin = Vector2.zero;
            
            rawImage.texture = renderTexture;
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            yield return base.TearDown();
            GameObject.Destroy(mainGameObject);
            GameObject.Destroy(childrenGameObject);
            componentsComposer.Dispose();
            canvasPainter.Dispose();
            DataStore.i.ecs7.scenes.Clear();
            DataStore.i.ecs7.uiDataContainer = new UIDataContainer();
        }

        [UnityTest]
        [Explicit, Category("Explicit")]
        public IEnumerator CastShadowFalseShouldWork_Generate()
        {
            yield return VisualTestUtils.GenerateBaselineForTest(DrawUITransformCorrectly());
        }

        [UnityTest]
        public IEnumerator DrawUITransformCorrectly()
        {
            string testSceneId = "testId";
            worldState.Configure().currentSceneId.Returns(testSceneId);
            var testScene = testUtils.CreateScene(testSceneId);
            testScene.CreateEntity(512);
            testScene.CreateEntity(513);
            DataStore.i.ecs7.scenes.Add(testScene);

            var parent = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            parent.AlignItems = YGAlign.Center;
            parent.JustifyContent = YGJustify.Center;
            var child = ECS7TestUIUtils.CreatePBUiTransformDefaultModel();
            child.Width = 400;
            child.Height = 400;
            var message = CreateCRDTMessage(512, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(parent));
            var message2 = CreateCRDTMessage(513, ComponentID.UI_TRANSFORM, UITransformSerializer.Serialize(child));
            testScene.crdtExecutor.ExecuteWithoutStoringState(message);
            testScene.crdtExecutor.ExecuteWithoutStoringState(message2);

            canvasPainter.framesCounter = 9999;
            canvasPainter.Update();
            
            string textureName =  "CanvasPainterTest";
            yield return VisualTestUtils.TakeSnapshot(textureName, camera);
        }

        private CRDTMessage CreateCRDTMessage(int entityId, int componentId, object data)
        {
            CRDTMessage message = new CRDTMessage()
            {
                key1 = entityId,
                key2 = componentId,
                data = data,
                timestamp = 1
            };
            return message;
        }
    }
}