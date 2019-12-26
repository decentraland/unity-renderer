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

            Assert.AreEqual(cursorController.hoverCursor, cursorController.cursorImage.sprite);

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

            Assert.AreEqual(cursorController.cursorImage.sprite, cursorController.normalCursor);

            DCLCharacterController.i.ResumeGravity();
        }
    }
}
