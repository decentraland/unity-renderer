using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using DCL;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using DCL.Camera;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CursorControllerTests : IntegrationTestSuite_Legacy
    {
        protected override bool enableSceneIntegrityChecker => false;

        private CameraController cameraController => SceneReferences.i.cameraController;
        private ParcelScene scene;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            Environment.i.world.sceneController.SortScenesByDistance();
            scene = TestUtils.CreateTestScene();
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

            var cursorController = GameObject.FindObjectOfType<CursorController>();

            // Check cursor shows normal sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            DCLCharacterController.i.PauseGravity();
            DCLCharacterController.i.SetPosition(new Vector3(8, 1, 7f));

            var cameraController = GameObject.FindObjectOfType<CameraController>();

            // Rotate camera towards the interactive object
            var cameraRotationPayload = new CameraController.SetRotationPayload()
            {
                x = 45,
                y = 0,
                z = 0
            };

            cameraController.SetRotation(JsonConvert.SerializeObject(cameraRotationPayload, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            yield return null;

            // Check cursor shows hover sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.hoverCursor);

            // Rotate the camera away from the interactive object
            cameraRotationPayload = new CameraController.SetRotationPayload()
            {
                x = 0,
                y = 0,
                z = 0,
                cameraTarget = (DCLCharacterController.i.transform.position - entity.gameObject.transform.position)
            };

            cameraController.SetRotation(JsonConvert.SerializeObject(cameraRotationPayload, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            yield return null;

            // Check cursor shows normal sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            DCLCharacterController.i.ResumeGravity();
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

            var cursorController = GameObject.FindObjectOfType<CursorController>();

            // Check cursor shows normal sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            DCLCharacterController.i.PauseGravity();
            DCLCharacterController.i.SetPosition(new Vector3(8, 1, 7f));

            // Rotate camera towards the interactive object
            cameraController.SetRotation(45, 0, 0);

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

            DCLCharacterController.i.ResumeGravity();
        }

        [UnityTest]
        public IEnumerator FeedbackIsNotDisplayedOnParent()
        {
            var cursorController = GameObject.FindObjectOfType<CursorController>();

            Assert.IsNotNull(cameraController, "camera is null?");

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
            DCLCharacterController.i.SetPosition(new Vector3(3, 2, 1));
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
            DCLCharacterController.i.SetPosition(new Vector3(3, 2, 12));
            cameraController.SetRotation(0, 0, 0, new Vector3(0, 0, -1));

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

            yield return null;

            DCLCharacterController.i.SetPosition(new Vector3(8, 1, 7f));

            var cameraController = GameObject.FindObjectOfType<CameraController>();

            // Rotate camera towards the interactive object
            cameraController.SetRotation(45, 0, 0);

            yield return null;

            var hoverCanvasController = InteractionHoverCanvasController.i;
            Assert.IsNotNull(hoverCanvasController);

            // Check hover feedback is enabled
            Assert.IsTrue(hoverCanvasController.canvas.enabled);

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
            Assert.IsFalse(hoverCanvasController.canvas.enabled);

            DCLCharacterController.i.ResumeGravity();
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

            DCLCharacterController.i.SetPosition(new Vector3(8, 1, 7f));

            var cameraController = GameObject.FindObjectOfType<CameraController>();

            // Rotate camera towards the interactive object
            cameraController.SetRotation(45, 0, 0);

            yield return null;

            var hoverCanvasController = InteractionHoverCanvasController.i;
            Assert.IsNotNull(hoverCanvasController);

            // Check hover feedback is enabled
            Assert.IsTrue(hoverCanvasController.canvas.enabled);

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
            Assert.IsTrue(hoverCanvasController.canvas.enabled);

            DCLCharacterController.i.ResumeGravity();
        }
    }
}