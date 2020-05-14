using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CursorControllerTests : TestsBase
    {
        protected override IEnumerator SetUp()
        {
            sceneInitialized = false;
            return base.SetUp();
        }

        [UnityTest]
        public IEnumerator OnPointerHoverFeedbackIsDisplayedCorrectly()
        {
            DecentralandEntity entity;
            BoxShape shape;

            shape = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });

            TestHelpers.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity, new Vector3(3, 3, 3));
            yield return shape.routine;

            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };

            var component = TestHelpers.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
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
            DecentralandEntity entity;
            BoxShape shape;

            shape = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });

            TestHelpers.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity, new Vector3(3, 3, 3));
            yield return shape.routine;

            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };

            var component = TestHelpers.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
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
            TestHelpers.UpdateShape(scene, shape.id, JsonConvert.SerializeObject(
            new
            {
                visible = false
            }));

            yield return null;

            // Check cursor shows normal sprite
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            DCLCharacterController.i.ResumeGravity();
        }

        [UnityTest]
        [NUnit.Framework.Explicit("This test started failing on the CI out of the blue. Will be re-enabled after implementing a solution dealing with high delta times")]
        [Category("Explicit")]
        public IEnumerator FeedbackIsNotDisplayedOnParent()
        {
            var cursorController = GameObject.FindObjectOfType<CursorController>();

            Assert.IsNotNull(cameraController, "camera is null?");

            // Create parent entity
            DecentralandEntity blockingEntity;
            BoxShape blockingShape = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out blockingEntity,
                new BoxShape.Model() { });
            TestHelpers.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(1, 1, 1));
            yield return blockingShape.routine;

            // Create target entity for click
            DecentralandEntity clickTargetEntity;
            BoxShape clickTargetShape = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out clickTargetEntity,
                new BoxShape.Model() { });
            TestHelpers.SetEntityTransform(scene, clickTargetEntity, new Vector3(0, 0, 5), Quaternion.identity, new Vector3(1, 1, 1));
            yield return clickTargetShape.routine;

            // Enparent target entity as a child of the blocking entity
            TestHelpers.SetEntityParent(scene, clickTargetEntity, blockingEntity);

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
            var component = TestHelpers.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, clickTargetEntity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            // Check if target entity is triggered by looking at the parent entity
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            // Move character in front of target entity and rotate camera
            DCLCharacterController.i.SetPosition(new Vector3(3, 2, 12));
            cameraController.SetRotation(0, 0, 0, new Vector3(0, 0, -1));

            yield return null;

            // Check if target entity is triggered when looked at directly
            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.hoverCursor);
        }
    }
}
