using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using DCL;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL.Camera;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Tests
{
    public class CursorControllerTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private Camera mainCamera;
        private CursorController cursorController;
        private UUIDEventsPlugin uuidEventsPlugin;
        private CoreComponentsPlugin coreComponentsPlugin;
        private UIComponentsPlugin uiComponentsPlugin;

        protected override List<GameObject> SetUp_LegacySystems()
        {
            List<GameObject> result = new List<GameObject>();
            result.Add(MainSceneFactory.CreateEnvironment());
            result.Add(MainSceneFactory.CreateEventSystem());
            return result;
        }

        protected override ServiceLocator InitializeServiceLocator()
        {
            ServiceLocator result = DCL.ServiceLocatorTestFactory.CreateMocked();
            result.Register<IRuntimeComponentFactory>( () => new RuntimeComponentFactory());
            result.Register<IWorldState>( () => new WorldState());
            result.Register<IUpdateEventHandler>( () => new UpdateEventHandler());
            result.Register<IWebRequestController>( () => new WebRequestController(
                new GetWebRequestFactory(),
                new WebRequestAssetBundleFactory(),
                new WebRequestTextureFactory(),
                new WebRequestAudioFactory(),
                new PostWebRequestFactory(),
                new PutWebRequestFactory(),
                new PatchWebRequestFactory(),
                new DeleteWebRequestFactory()
            ) );
            return result;
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            Utils.LockCursor();

            cursorController = TestUtils.CreateComponentWithGameObject<CursorController>("CursorController");
            cursorController.normalCursor = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector3.zero);
            cursorController.normalCursor.name = "Normal";
            cursorController.hoverCursor = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector3.zero);
            cursorController.hoverCursor.name = "Hover";
            cursorController.cursorImage = cursorController.gameObject.AddComponent<Image>();
            cursorController.cursorImage.enabled = false;
            cursorController.SetCursor(cursorController.normalCursor);

            yield return base.SetUp();

            coreComponentsPlugin = new CoreComponentsPlugin();
            uuidEventsPlugin = new UUIDEventsPlugin();
            uiComponentsPlugin = new UIComponentsPlugin();
            scene = TestUtils.CreateTestScene();

            Physics.autoSyncTransforms = true;

            mainCamera = TestUtils.CreateComponentWithGameObject<Camera>("Main Camera");
            mainCamera.tag = "MainCamera";
            mainCamera.transform.position = Vector3.zero;
            mainCamera.transform.forward = Vector3.forward;
        }

        protected override IEnumerator TearDown()
        {
            coreComponentsPlugin.Dispose();
            uuidEventsPlugin.Dispose();
            uiComponentsPlugin.Dispose();
            Object.Destroy(cursorController.normalCursor);
            Object.Destroy(cursorController.hoverCursor);
            Object.Destroy(cursorController.gameObject);
            Object.Destroy(mainCamera.gameObject);
            Utils.UnlockCursor();
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator OnPointerHoverFeedbackIsDisplayedCorrectly()
        {
            IDCLEntity entity;
            BoxShape shape;

            shape = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });

            TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity, new Vector3(3, 3, 3));
            yield return shape.routine;

            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };

            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            yield return null;

            // Check cursor shows normal sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            mainCamera.transform.position = new Vector3(8, 2, 7f);
            mainCamera.transform.rotation = Quaternion.identity;

            //Debug.Break();
            yield return null;
            yield return null;

            // Check cursor shows hover sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.hoverCursor);

            mainCamera.transform.forward = Vector3.back;

            //Debug.Break();
            yield return null;
            yield return null;

            // Check cursor shows normal sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);
        }

        [UnityTest]
        public IEnumerator OnPointerHoverFeedbackNotDisplayedOnInvisibles()
        {
            IDCLEntity entity;
            BoxShape shape;

            shape = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });

            TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity, new Vector3(3, 3, 3));
            yield return shape.routine;

            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };

            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            yield return null;

            // Check cursor shows normal sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            mainCamera.transform.position = new Vector3(8, 2, 7f);
            mainCamera.transform.forward = Vector3.forward;

            yield return null;

            // Check cursor shows hover sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.hoverCursor);

            // Make shape invisible
            TestUtils.UpdateShape(scene, shape.id, JsonConvert.SerializeObject(
                new
                {
                    visible = false,
                    withCollisions = false,
                    isPointerBlocker = false
                }));

            yield return null;

            // Check cursor shows normal sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);
        }

        [UnityTest]
        public IEnumerator FeedbackIsNotDisplayedOnParent()
        {
            // Create parent entity
            IDCLEntity blockingEntity;
            BoxShape blockingShape = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out blockingEntity,
                new BoxShape.Model() { });
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(1, 1, 1));
            yield return blockingShape.routine;

            // Create target entity for click
            IDCLEntity clickTargetEntity;
            BoxShape clickTargetShape = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out clickTargetEntity,
                new BoxShape.Model() { });
            TestUtils.SetEntityTransform(scene, clickTargetEntity, new Vector3(0, 0, 5), Quaternion.identity, new Vector3(1, 1, 1));
            yield return clickTargetShape.routine;

            // Enparent target entity as a child of the blocking entity
            TestUtils.SetEntityParent(scene, clickTargetEntity, blockingEntity);

            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 3, 1);

            yield return null;

            // Create pointer down component and add it to target entity
            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, clickTargetEntity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return component.routine;

            Assert.IsTrue(component != null);

            // Check if target entity is triggered by looking at the parent entity
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            // Move character in front of target entity and rotate camera
            mainCamera.transform.position = new Vector3(3, 3, 12);
            mainCamera.transform.forward = Vector3.back;

            yield return null;
            yield return null;
            yield return null;

            // Check if target entity is triggered when looked at directly
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.hoverCursor);
        }

        [UnityTest]
        public IEnumerator OnPointerHoverFeedbackIsBlockedByUI()
        {
            IDCLEntity entity;
            BoxShape shape;

            shape = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });

            TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity, new Vector3(3, 3, 3));
            yield return shape.routine;

            var onPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);
            Assert.IsTrue(component != null);

            // Hover feedback is disabled by default
            Assert.IsFalse(uuidEventsPlugin.hoverCanvas.canvas.enabled);

            mainCamera.transform.position = new Vector3(8, 2, 7f);
            mainCamera.transform.forward = Vector3.forward;

            yield return null;

            Assert.IsNotNull(uuidEventsPlugin.hoverCanvas);
            Assert.IsTrue(uuidEventsPlugin.hoverCanvas.canvas.enabled);

            // Put UI in the middle
            UIScreenSpace screenSpaceShape =
                TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            UIContainerRect uiContainerRectShape =
                TestUtils.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                    CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model() { color = Color.white });

            yield return uiContainerRectShape.routine;

            yield return null;

            // Check hover feedback is no longer enabled
            Assert.IsFalse(uuidEventsPlugin.hoverCanvas.canvas.enabled);
        }

        [UnityTest]
        public IEnumerator OnPointerHoverFeedbackIsNotBlockedByFullyAlphaUIContainer()
        {
            IDCLEntity entity;
            BoxShape shape;

            shape = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });

            TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity, new Vector3(3, 3, 3));
            yield return shape.routine;

            var onPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);
            Assert.IsTrue(component != null);

            yield return null;

            mainCamera.transform.position = new Vector3(8, 2, 7f);
            mainCamera.transform.forward = Vector3.forward;

            yield return null;

            Assert.IsNotNull(uuidEventsPlugin.hoverCanvas);

            // Check hover feedback is enabled
            Assert.IsTrue(uuidEventsPlugin.hoverCanvas.canvas.enabled);

            // Put UI in the middle
            UIScreenSpace screenSpaceShape =
                TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            UIContainerRect uiContainerRectShape =
                TestUtils.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                    CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model() { color = Color.clear });
            yield return uiContainerRectShape.routine;

            yield return null;

            // Check hover feedback is still enabled
            Assert.IsTrue(uuidEventsPlugin.hoverCanvas.canvas.enabled);
        }
    }
}
