using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Tests
{
    public class UITests
    {
        [UnityTest]
        public IEnumerator UIScreenSpaceShapeVisibilityUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            //Get character
            var characterController = GameObject.FindObjectOfType<DCLCharacterController>() ?? (GameObject.Instantiate(Resources.Load("Prefabs/CharacterController") as GameObject)).GetComponent<DCLCharacterController>();
            characterController.gravity = 0f;

            // Position character inside parcel (0,0)
            characterController.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            string uiComponentId = "uiCanvas";

            UIScreenSpaceShape screenSpaceShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_SCREEN_SPACE_SHAPE,
                id = uiComponentId,
                name = "UIScreenSpaceShape"
            })) as UIScreenSpaceShape;

            Canvas canvas = screenSpaceShape.transform.GetComponent<Canvas>();

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the character is inside the scene, the UIScreenSpaceShape should be visible");

            // Update canvas visibility value manually
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiComponentId,
                json = JsonUtility.ToJson(new DCL.Components.UIScreenSpaceShape.Model
                {
                    visible = false
                })
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check visibility
            Assert.IsFalse(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'invisible', its canvas shouldn't be visible");

            // Re-enable visibility
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiComponentId,
                json = JsonUtility.ToJson(new DCL.Components.UIScreenSpaceShape.Model
                {
                    visible = true
                })
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'visible', its canvas should be visible");

            // Position character outside parcel
            characterController.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 100f,
                y = 0f,
                z = 100f
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check visibility
            Assert.IsFalse(canvas.enabled, "When the character is outside the scene, the UIScreenSpaceShape shouldn't be visible");

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIScreenSpaceShapeMissingValuesGetDefaultedOnUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            //Get character
            var characterController = GameObject.FindObjectOfType<DCLCharacterController>() ?? (GameObject.Instantiate(Resources.Load("Prefabs/CharacterController") as GameObject)).GetComponent<DCLCharacterController>();
            characterController.gravity = 0f;

            // Position character inside parcel (0,0)
            characterController.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            string uiComponentId = "uiCanvas";

            UIScreenSpaceShape screenSpaceShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_SCREEN_SPACE_SHAPE,
                id = uiComponentId,
                name = "UIScreenSpaceShape"
            })) as UIScreenSpaceShape;

            Canvas canvas = screenSpaceShape.transform.GetComponent<Canvas>();

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the character is inside the scene, the UIScreenSpaceShape should be visible");

            // Update canvas visibility value manually
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiComponentId,
                json = JsonUtility.ToJson(new DCL.Components.UIScreenSpaceShape.Model
                {
                    visible = false
                })
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check visibility
            Assert.IsFalse(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'invisible', its canvas shouldn't be visible");

            // Update model without the visible property
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiComponentId,
                json = JsonUtility.ToJson(new DCL.Components.UIScreenSpaceShape.Model { })
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check visibility
            Assert.IsTrue(canvas.enabled);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIContainerRectShapePropertiesAreAppliedCorrectly()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            //Get character
            var characterController = GameObject.FindObjectOfType<DCLCharacterController>() ?? (GameObject.Instantiate(Resources.Load("Prefabs/CharacterController") as GameObject)).GetComponent<DCLCharacterController>();
            characterController.gravity = 0f;

            // Position character inside parcel (0,0)
            characterController.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            string uiScreenSpaceShapeId = "uiCanvas";

            UIScreenSpaceShape screenSpaceShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_SCREEN_SPACE_SHAPE,
                id = uiScreenSpaceShapeId,
                name = "UIScreenSpaceShape"
            })) as UIScreenSpaceShape;

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRectShape;

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Image image = uiContainerRectShape.transform.GetComponent<Image>();

            // Check default properties are applied correctly
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 255f));
            Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(uiContainerRectShape.transform.rect.width, screenSpaceShape.transform.rect.width);
            Assert.AreEqual(uiContainerRectShape.transform.rect.height, screenSpaceShape.transform.rect.height);
            Assert.IsTrue(uiContainerRectShape.transform.anchoredPosition == Vector2.zero);
            Assert.IsTrue(uiContainerRectShape.transform.anchorMin == new Vector2(0.5f, 0.5f));
            Assert.IsTrue(uiContainerRectShape.transform.anchorMax == new Vector2(0.5f, 0.5f));

            // Update UIContainerRectShape properties
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model
                {
                    parentComponent = uiScreenSpaceShapeId,
                    thickness = 5,
                    color = new Color(128f, 200f, 27f, 255f),
                    isPointerBlocker = true,
                    width = 0.3f,
                    height = 0.5f,
                    position = new Vector2(-0.3f, 0f),
                    hAlign = "right",
                    vAlign = "bottom",
                    alignmentUsesSize = false
                })
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerRectShape.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(128f, 200f, 27f, 255f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(uiContainerRectShape.transform.rect.width, screenSpaceShape.transform.rect.width * 0.3f);
            Assert.AreEqual(uiContainerRectShape.transform.rect.height, screenSpaceShape.transform.rect.height * 0.5f);
            Assert.AreEqual(uiContainerRectShape.transform.anchoredPosition, new Vector2(screenSpaceShape.transform.rect.width * -0.3f, 0f));
            Assert.IsTrue(uiContainerRectShape.transform.anchorMin == new Vector2(1f, 0f));
            Assert.IsTrue(uiContainerRectShape.transform.anchorMax == new Vector2(1f, 0f));

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIContainerRectShapeMissingValuesGetDefaultedOnUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            //Get character
            var characterController = GameObject.FindObjectOfType<DCLCharacterController>() ?? (GameObject.Instantiate(Resources.Load("Prefabs/CharacterController") as GameObject)).GetComponent<DCLCharacterController>();
            characterController.gravity = 0f;

            // Position character inside parcel (0,0)
            characterController.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            string uiScreenSpaceShapeId = "uiCanvas";

            UIScreenSpaceShape screenSpaceShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_SCREEN_SPACE_SHAPE,
                id = uiScreenSpaceShapeId,
                name = "UIScreenSpaceShape"
            })) as UIScreenSpaceShape;

            yield return new WaitForEndOfFrame();

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRectShape;

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Image image = uiContainerRectShape.transform.GetComponent<Image>();

            // Update UIContainerRectShape properties
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model
                {
                    parentComponent = uiScreenSpaceShapeId,
                    thickness = 5,
                    color = new Color(128f, 200f, 27f, 255f),
                    isPointerBlocker = true,
                    width = 0.3f,
                    height = 0.5f,
                    position = new Vector2(0.5f, 0.5f)
                })
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerRectShape.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(128f, 200f, 27f, 255f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(uiContainerRectShape.transform.rect.width, screenSpaceShape.transform.rect.width * 0.3f);
            Assert.AreEqual(uiContainerRectShape.transform.rect.height, screenSpaceShape.transform.rect.height * 0.5f);
            Assert.IsTrue(uiContainerRectShape.transform.anchoredPosition == new Vector2(screenSpaceShape.transform.rect.width * 0.5f, screenSpaceShape.transform.rect.height * 0.5f));

            // Update UIContainerRectShape with missing values
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model
                {
                    parentComponent = uiScreenSpaceShapeId
                })
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check default properties are applied correctly
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 255f));
            Assert.IsFalse(image.raycastTarget);
            Assert.IsTrue(uiContainerRectShape.transform.rect.width.ToString() == screenSpaceShape.transform.rect.width.ToString());
            Assert.IsTrue(uiContainerRectShape.transform.rect.height.ToString() == screenSpaceShape.transform.rect.height.ToString());
            Assert.IsTrue(uiContainerRectShape.transform.anchoredPosition == Vector2.zero);
            Assert.IsTrue(uiContainerRectShape.transform.anchorMin == new Vector2(0.5f, 0.5f));
            Assert.IsTrue(uiContainerRectShape.transform.anchorMax == new Vector2(0.5f, 0.5f));

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIContainerRectShapeSizeInPixels()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            //Get character
            var characterController = GameObject.FindObjectOfType<DCLCharacterController>() ?? (GameObject.Instantiate(Resources.Load("Prefabs/CharacterController") as GameObject)).GetComponent<DCLCharacterController>();
            characterController.gravity = 0f;

            // Position character inside parcel (0,0)
            characterController.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            string uiScreenSpaceShapeId = "uiCanvas";

            UIScreenSpaceShape screenSpaceShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_SCREEN_SPACE_SHAPE,
                id = uiScreenSpaceShapeId,
                name = "UIScreenSpaceShape"
            })) as UIScreenSpaceShape;

            yield return new WaitForEndOfFrame();

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRectShape;

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Image image = uiContainerRectShape.transform.GetComponent<Image>();

            // Update UIContainerRectShape properties
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model
                {
                    parentComponent = uiScreenSpaceShapeId,
                    sizeInPixels = true,
                    width = 50f,
                    height = 50f
                })
            }));

            // Unity UI takes 2 frames to update
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Check updated properties are applied correctly
            Assert.AreEqual(uiContainerRectShape.transform.rect.width, 50f);

            screenSpaceShape.Dispose();
        }
    }
}