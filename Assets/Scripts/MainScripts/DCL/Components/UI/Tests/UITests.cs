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
    public class UITests : TestsBase
    {
        [UnityTest]
        public IEnumerator UIScreenSpaceShapeVisibilityUpdate()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;// WaitForUICanvasUpdate();

            Canvas canvas = screenSpaceShape.transform.GetComponent<Canvas>();

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the character is inside the scene, the UIScreenSpaceShape should be visible");

            // Update canvas visibility value manually
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new DCL.Components.UIScreenSpaceShape.Model
                {
                    visible = false
                })
            })) as UIScreenSpaceShape;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsFalse(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'invisible', its canvas shouldn't be visible");

            // Re-enable visibility
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new DCL.Components.UIScreenSpaceShape.Model
                {
                    visible = true
                })
            })) as UIScreenSpaceShape;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'visible', its canvas should be visible");

            // Position character outside parcel
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 100f,
                y = 0f,
                z = 100f
            }));

            // Check visibility
            Assert.IsFalse(canvas.enabled, "When the character is outside the scene, the UIScreenSpaceShape shouldn't be visible");

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIScreenSpaceShapeIsScaledWhenCharacterIsElsewhere()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character outside parcel (1,1)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 1.5f,
                y = 0f,
                z = 1.5f
            }));

            yield return null;

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            RectTransform canvasRectTransform = screenSpaceShape.transform.GetComponent<RectTransform>();

            Debug.Log($"width = {canvasRectTransform.rect.width}... height = {canvasRectTransform.rect.height}");
            // Check if canvas size is correct (1280x720 taking into account unity scaling minor inconsistences)
            Assert.IsTrue(canvasRectTransform.rect.width >= 1275 && canvasRectTransform.rect.width <= 1285);
            Assert.IsTrue(canvasRectTransform.rect.height >= 715 && canvasRectTransform.rect.height <= 725);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIScreenSpaceShapeMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Canvas canvas = screenSpaceShape.transform.GetComponent<Canvas>();

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the character is inside the scene, the UIScreenSpaceShape should be visible");

            // Update canvas visibility value manually
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new DCL.Components.UIScreenSpaceShape.Model
                {
                    visible = false
                })
            })) as UIScreenSpaceShape;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsFalse(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'invisible', its canvas shouldn't be visible");

            // Update model without the visible property
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new DCL.Components.UIScreenSpaceShape.Model { })
            })) as UIScreenSpaceShape;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsTrue(canvas.enabled);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIContainerRectShapePropertiesAreAppliedCorrectly()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRectShape;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model { })
            }));

            yield return uiContainerRectShape.routine;

            Image image = uiContainerRectShape.referencesContainer.image;

            // Check default properties are applied correctly
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 255f));
            Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(100f, uiContainerRectShape.transform.rect.width);
            Assert.AreEqual(100f, uiContainerRectShape.transform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerRectShape.transform.localPosition);

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new UIContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    thickness = 5,
                    color = new Color(0.2f, 0.7f, 0.05f, 1f),
                    isPointerBlocker = true,
                    width = 275f,
                    height = 130f,
                    position = new Vector2(-30f, -15f),
                    hAlign = "right",
                    vAlign = "bottom"
                })
            })) as UIContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(0.2f * 255f, 0.7f * 255f, 0.05f * 255f, 1f * 255f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(275f, uiContainerRectShape.transform.rect.width);
            Assert.AreEqual(130f, uiContainerRectShape.transform.rect.height);
            Assert.IsTrue(uiContainerRectShape.referencesContainer.alignmentLayoutGroup.childAlignment == TextAnchor.LowerRight);

            Vector2 alignedPosition = new Vector2((screenSpaceShape.transform.rect.width - uiContainerRectShape.transform.rect.width / 2),
                                                    -(screenSpaceShape.transform.rect.height - uiContainerRectShape.transform.rect.height / 2));
            alignedPosition += new Vector2(-30, -15); // apply offset position

            Assert.AreEqual(alignedPosition.ToString(), uiContainerRectShape.transform.anchoredPosition.ToString());

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIComponentsParenting()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);


            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape parent
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                })
            })) as UIContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Check updated parent
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.transform);

            // Create 2nd UIContainerRectShape
            string uiContainerRectShape2Id = "uiContainerRectShape2";

            UIContainerRectShape uiContainerRectShape2 = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShape2Id,
                name = "UIContainerRectShape2"
            })) as UIContainerRectShape;

            yield return uiContainerRectShape2.routine;

            // Update UIContainerRectShape parent to the previous container
            uiContainerRectShape2 = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShape2Id,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model
                {
                    parentComponent = uiContainerRectShapeId,
                })
            })) as UIContainerRectShape;

            yield return uiContainerRectShape2.routine;

            // Check updated parent
            Assert.IsTrue(uiContainerRectShape2.referencesContainer.transform.parent == uiContainerRectShape.transform);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIContainerRectShapeMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new UIContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    thickness = 5,
                    color = new Color(0.5f, 0.8f, 0.1f, 1f),
                    isPointerBlocker = true,
                    width = 200f,
                    height = 150f,
                    position = new Vector2(20f, 45f)
                })
            })) as UIContainerRectShape;

            yield return uiContainerRectShape.routine;

            Image image = uiContainerRectShape.referencesContainer.image;

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(0.5f * 255f, 0.8f * 255f, 0.1f * 255f, 1f * 255f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(200f, uiContainerRectShape.transform.rect.width);
            Assert.AreEqual(150f, uiContainerRectShape.transform.rect.height);
            Assert.AreEqual(new Vector3(20f, 45f, 0f), uiContainerRectShape.transform.localPosition);

            // Update UIContainerRectShape with missing values
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id
                })
            })) as UIContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 255f));
            Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(100f, uiContainerRectShape.transform.rect.width);
            Assert.AreEqual(100f, uiContainerRectShape.transform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerRectShape.transform.localPosition);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIContainerRectShapeNormalizedSize()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    sizeInPixels = false,
                    width = 0.5f,
                    height = 0.3f
                })
            })) as UIContainerRectShape;

            yield return uiContainerRectShape.routine;

            Image image = uiContainerRectShape.transform.GetComponent<Image>();

            // Check updated properties are applied correctly
            Assert.AreEqual(screenSpaceShape.transform.rect.width * 0.5f, uiContainerRectShape.transform.rect.width);
            Assert.AreEqual(screenSpaceShape.transform.rect.height * 0.3f, uiContainerRectShape.transform.rect.height);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIImageShapePropertiesAreAppliedCorrectly()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiImageShapeId = "uiImageShape";

            UIImageShape uiImageshape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_IMAGE_SHAPE,
                id = uiImageShapeId,
                name = "UIImageShape"
            })) as UIImageShape;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model { })
            }));

            yield return uiImageshape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(uiImageshape.referencesContainer.image.color == new Color(255f, 255f, 255f, 255f));
            Assert.IsFalse(uiImageshape.referencesContainer.image.raycastTarget);
            Assert.AreEqual(100f, uiImageshape.transform.rect.width);
            Assert.AreEqual(100f, uiImageshape.transform.rect.height);
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture == null);

            Vector2 alignedPosition = new Vector2((uiImageshape.transform.rect.width / 2), -(uiImageshape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.transform.anchoredPosition.ToString());

            // Update UIContainerRectShape properties
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    opacity = 0.7f,
                    isPointerBlocker = true,
                    width = 128f,
                    height = 128f,
                    position = new Vector2(-55f, 30f),
                    hAlign = "right",
                    vAlign = "bottom",
                    sourceLeft = 64f,
                    sourceTop = 64f,
                    sourceWidth = 64f,
                    sourceHeight = 64f,
                    paddingTop = 10f,
                    paddingRight = 10f,
                    paddingBottom = 10f,
                    paddingLeft = 10f
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.AreEqual(new Color(255f, 255f, 255f, 255f * 0.7f), uiImageshape.referencesContainer.image.color);
            Assert.IsTrue(uiImageshape.referencesContainer.image.raycastTarget);

            Assert.AreEqual(108f, uiImageshape.transform.rect.width); // 128 - 20 (left and right padding)
            Assert.AreEqual(108f, uiImageshape.transform.rect.height); // 128 - 20 (left and right padding)
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture != null);

            alignedPosition = new Vector2((screenSpaceShape.transform.rect.width - uiImageshape.transform.rect.width / 2),
                                        -(screenSpaceShape.transform.rect.height - uiImageshape.transform.rect.height / 2));
            alignedPosition += new Vector2(-65f, 40f); // affected by padding

            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());

            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.x);
            Assert.AreEqual(0f, uiImageshape.referencesContainer.image.uvRect.y);
            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.width);
            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.height);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.bottom);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.top);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.left);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.right);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIImageShapeMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiImageShapeId = "uiImageShape";

            UIImageShape uiImageshape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_IMAGE_SHAPE,
                id = uiImageShapeId,
                name = "UIImageShape"
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Update UIContainerRectShape properties
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    opacity = 0.7f,
                    isPointerBlocker = true,
                    width = 128f,
                    height = 128f,
                    position = new Vector2(17f, 32f),
                    hAlign = "right",
                    vAlign = "bottom",
                    sourceLeft = 64f,
                    sourceTop = 64f,
                    sourceWidth = 64f,
                    sourceHeight = 64f,
                    paddingTop = 10f,
                    paddingRight = 10f,
                    paddingBottom = 10f,
                    paddingLeft = 10f
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.AreEqual(new Color(255f, 255f, 255f, 255f * 0.7f), uiImageshape.referencesContainer.image.color);
            Assert.IsTrue(uiImageshape.referencesContainer.image.raycastTarget);

            Assert.AreEqual(108f, uiImageshape.transform.rect.width); // 128 - 20 (left and right padding)
            Assert.AreEqual(108f, uiImageshape.transform.rect.height); // 128 - 20 (left and right padding)
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture != null);

            Vector2 alignedPosition = new Vector2((screenSpaceShape.transform.rect.width - uiImageshape.transform.rect.width / 2),
                                        -(screenSpaceShape.transform.rect.height - uiImageshape.transform.rect.height / 2));
            alignedPosition += new Vector2(7f, 42f); // affected by padding

            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());

            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.x);
            Assert.AreEqual(0f, uiImageshape.referencesContainer.image.uvRect.y);
            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.width);
            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.height);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.bottom);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.top);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.left);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.right);

            // Update UIContainerRectShape properties
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model { })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(uiImageshape.referencesContainer.image.color == new Color(255f, 255f, 255f, 255f));
            Assert.IsFalse(uiImageshape.referencesContainer.image.raycastTarget);
            Assert.AreEqual(100f, uiImageshape.transform.rect.width);
            Assert.AreEqual(100f, uiImageshape.transform.rect.height);
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture == null);

            alignedPosition = new Vector2((uiImageshape.transform.rect.width / 2), -(uiImageshape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.transform.anchoredPosition.ToString());

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIShapeAlign()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpaceShape, DCL.Components.UIScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiImageShapeId = "uiImageShape";

            UIImageShape uiImageshape = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.UI_IMAGE_SHAPE,
                id = uiImageShapeId,
                name = "UIImageShape"
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Align to right-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "right",
                    vAlign = "bottom",
                })
            })) as UIImageShape;

            // waiting for the texture fetching
            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            Vector2 alignedPosition = new Vector2((screenSpaceShape.transform.rect.width - uiImageshape.transform.rect.width / 2),
                                        -(screenSpaceShape.transform.rect.height - uiImageshape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to right-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "right",
                    vAlign = "center",
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = new Vector2((screenSpaceShape.transform.rect.width - uiImageshape.transform.rect.width / 2),
                                        -(screenSpaceShape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to right-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "right",
                    vAlign = "top",
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = new Vector2((screenSpaceShape.transform.rect.width - uiImageshape.transform.rect.width / 2),
                                        -(uiImageshape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "center",
                    vAlign = "bottom",
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = new Vector2((screenSpaceShape.transform.rect.width / 2),
                                        -(screenSpaceShape.transform.rect.height - uiImageshape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "center",
                    vAlign = "center",
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = new Vector2((screenSpaceShape.transform.rect.width / 2),
                                        -(screenSpaceShape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "center",
                    vAlign = "top",
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = new Vector2((screenSpaceShape.transform.rect.width / 2),
                                        -(uiImageshape.transform.rect.height - uiImageshape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "left",
                    vAlign = "bottom",
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = new Vector2((uiImageshape.transform.rect.width / 2),
                                        -(screenSpaceShape.transform.rect.height - uiImageshape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "left",
                    vAlign = "center",
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = new Vector2((uiImageshape.transform.rect.width / 2),
                                        -(screenSpaceShape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new DCL.Components.UIImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = 128f,
                    height = 128f,
                    hAlign = "left",
                    vAlign = "top",
                })
            })) as UIImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = new Vector2((uiImageshape.transform.rect.width / 2),
                                        -(uiImageshape.transform.rect.height / 2));
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            screenSpaceShape.Dispose();
        }
    }
}
