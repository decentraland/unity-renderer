using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Components.UI;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UITextShape = DCL.Components.UI.TextShape;
using TextShape = DCL.Components.TextShape;
using Newtonsoft.Json;

namespace Tests
{
    public class UITests : TestsBase
    {
        Vector2 CalculateAlignedPosition(Rect parentRect, Rect elementRect, string vAlign = "center", string hAlign = "center")
        {
            Vector2 result = Vector2.zero;

            switch (vAlign)
            {
                case "top":
                    result.y = -elementRect.height / 2;
                    break;
                case "bottom":
                    result.y = -(parentRect.height - elementRect.height / 2);
                    break;
                default: // center
                    result.y = -parentRect.height / 2;
                    break;
            }

            switch (hAlign)
            {
                case "left":
                    result.x = elementRect.width / 2;
                    break;
                case "right":
                    result.x = (parentRect.width - elementRect.width / 2);
                    break;
                default: // center
                    result.x = parentRect.width / 2;
                    break;
            }

            return result;
        }

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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Canvas canvas = screenSpaceShape.transform.GetComponent<Canvas>();

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the character is inside the scene, the UIScreenSpaceShape should be visible");

            // Update canvas visibility value manually
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new ScreenSpaceShape.Model
                {
                    visible = false
                })
            })) as ScreenSpaceShape;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsFalse(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'invisible', its canvas shouldn't be visible");

            // Re-enable visibility
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new ScreenSpaceShape.Model
                {
                    visible = true
                })
            })) as ScreenSpaceShape;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'visible', its canvas should be visible");

            // Position character outside parcel
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 100f,
                y = 3f,
                z = 100f
            }));

            yield return null;

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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            RectTransform canvasRectTransform = screenSpaceShape.transform.GetComponent<RectTransform>();

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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Canvas canvas = screenSpaceShape.transform.GetComponent<Canvas>();

            // Check visibility
            Assert.IsTrue(canvas.enabled, "When the character is inside the scene, the UIScreenSpaceShape should be visible");

            // Update canvas visibility value manually
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new ScreenSpaceShape.Model
                {
                    visible = false
                })
            })) as ScreenSpaceShape;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsFalse(canvas.enabled, "When the UIScreenSpaceShape is explicitly updated as 'invisible', its canvas shouldn't be visible");

            // Update model without the visible property
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new ScreenSpaceShape.Model { })
            })) as ScreenSpaceShape;

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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            ContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as ContainerRectShape;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new ContainerRectShape.Model { })
            }));

            yield return uiContainerRectShape.routine;

            Image image = uiContainerRectShape.referencesContainer.image;

            // Check default properties are applied correctly
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 1f));
            Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(100f, uiContainerRectShape.transform.rect.width);
            Assert.AreEqual(100f, uiContainerRectShape.transform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerRectShape.transform.localPosition);

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new ContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    thickness = 5,
                    color = new Color(0.2f, 0.7f, 0.05f, 1f),
                    isPointerBlocker = true,
                    width = new UIValue(275f),
                    height = new UIValue(130f),
                    positionX = new UIValue(-30f),
                    positionY = new UIValue(-15f),
                    hAlign = "right",
                    vAlign = "bottom"
                })
            })) as ContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(0.2f * 255f, 0.7f * 255f, 0.05f * 255f, 1f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(275f, uiContainerRectShape.transform.rect.width);
            Assert.AreEqual(130f, uiContainerRectShape.transform.rect.height);
            Assert.IsTrue(uiContainerRectShape.referencesContainer.alignmentLayoutGroup.childAlignment == TextAnchor.LowerRight);

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiContainerRectShape.transform.rect, "bottom", "right");
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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            ContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as ContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape parent
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new ContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                })
            })) as ContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Check updated parent
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.transform);

            // Create 2nd UIContainerRectShape
            string uiContainerRectShape2Id = "uiContainerRectShape2";

            ContainerRectShape uiContainerRectShape2 = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShape2Id,
                name = "UIContainerRectShape2"
            })) as ContainerRectShape;

            yield return uiContainerRectShape2.routine;

            // Update UIContainerRectShape parent to the previous container
            uiContainerRectShape2 = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShape2Id,
                json = JsonUtility.ToJson(new ContainerRectShape.Model
                {
                    parentComponent = uiContainerRectShapeId,
                })
            })) as ContainerRectShape;

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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            ContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as ContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new ContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    thickness = 5,
                    color = new Color(0.5f, 0.8f, 0.1f, 1f),
                    isPointerBlocker = true,
                    width = new UIValue(200f),
                    height = new UIValue(150f),
                    positionX = new UIValue(20f),
                    positionY = new UIValue(45f)
                })
            })) as ContainerRectShape;

            yield return uiContainerRectShape.routine;

            Image image = uiContainerRectShape.referencesContainer.image;

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(0.5f * 255f, 0.8f * 255f, 0.1f * 255f, 1f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(200f, uiContainerRectShape.transform.rect.width);
            Assert.AreEqual(150f, uiContainerRectShape.transform.rect.height);
            Assert.AreEqual(new Vector3(20f, 45f, 0f), uiContainerRectShape.transform.localPosition);

            // Update UIContainerRectShape with missing values
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new ContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id
                })
            })) as ContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 1f));
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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            ContainerRectShape uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as ContainerRectShape;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new ContainerRectShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    width = new UIValue(50f, UIValue.Unit.PERCENT),
                    height = new UIValue(30f, UIValue.Unit.PERCENT)
                })
            })) as ContainerRectShape;

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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiImageShapeId = "uiImageShape";

            ImageShape uiImageshape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_IMAGE_SHAPE,
                id = uiImageShapeId,
                name = "UIImageShape"
            })) as ImageShape;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model { })
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

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect);
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());

            // Update UIContainerRectShape properties
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    opacity = 0.7f,
                    isPointerBlocker = true,
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    positionX = new UIValue(-55f),
                    positionY = new UIValue(30f),
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
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.AreEqual(new Color(255f, 255f, 255f, 255f * 0.7f), uiImageshape.referencesContainer.image.color);
            Assert.IsTrue(uiImageshape.referencesContainer.image.raycastTarget);

            Assert.AreEqual(108f, uiImageshape.transform.rect.width); // 128 - 20 (left and right padding)
            Assert.AreEqual(108f, uiImageshape.transform.rect.height); // 128 - 20 (left and right padding)
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture != null);

            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "bottom", "right");
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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            ImageShape uiImageshape = TestHelpers.SharedComponentCreate<ImageShape, ImageShape.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);

            yield return uiImageshape.routine;

            // Update UIContainerRectShape properties
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageshape.id,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    opacity = 0.7f,
                    isPointerBlocker = true,
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    positionX = new UIValue(17f),
                    positionY = new UIValue(32f),
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
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.AreEqual(new Color(255f, 255f, 255f, 255f * 0.7f), uiImageshape.referencesContainer.image.color);
            Assert.IsTrue(uiImageshape.referencesContainer.image.raycastTarget);

            Assert.AreEqual(108f, uiImageshape.transform.rect.width); // 128 - 20 (left and right padding)
            Assert.AreEqual(108f, uiImageshape.transform.rect.height); // 128 - 20 (left and right padding)
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture != null);

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "bottom", "right");
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

            // Update UIImageShape properties
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageshape.id,
                json = JsonUtility.ToJson(new ImageShape.Model { })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(uiImageshape.referencesContainer.image.color == new Color(255f, 255f, 255f, 255f));
            Assert.IsFalse(uiImageshape.referencesContainer.image.raycastTarget);
            Assert.AreEqual(100f, uiImageshape.transform.rect.width);
            Assert.AreEqual(100f, uiImageshape.transform.rect.height);
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture == null);

            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect);
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());

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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiImageShapeId = "uiImageShape";

            ImageShape uiImageshape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_IMAGE_SHAPE,
                id = uiImageShapeId,
                name = "UIImageShape"
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Align to right-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    parentComponent = screenSpaceShape.id,
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "right",
                    vAlign = "bottom",
                })
            })) as ImageShape;

            // waiting for the texture fetching
            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "bottom", "right");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to right-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "right",
                    vAlign = "center",
                })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "center", "right");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to right-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "right",
                    vAlign = "top",
                })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "top", "right");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "center",
                    vAlign = "bottom",
                })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "bottom", "center");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "center",
                    vAlign = "center",
                })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "center", "center");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "center",
                    vAlign = "top",
                })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "top", "center");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "left",
                    vAlign = "bottom",
                })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "bottom", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "left",
                    vAlign = "center",
                })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "center", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new ImageShape.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "left",
                    vAlign = "top",
                })
            })) as ImageShape;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiImageshape.transform.rect, "top", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UITextShapePropertiesAreAppliedCorrectly()
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
            ScreenSpaceShape screenSpaceShape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UITextShape
            UITextShape uiTextShape = TestHelpers.SharedComponentCreate<UITextShape, UITextShape.Model>(scene, CLASS_ID.UI_TEXT_SHAPE,
            new UITextShape.Model { });

            yield return uiTextShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiTextShape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsFalse(uiTextShape.referencesContainer.text.raycastTarget);
            Assert.AreEqual(100f, uiTextShape.transform.rect.width);
            Assert.AreEqual(100f, uiTextShape.transform.rect.height);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enabled);
            Assert.AreEqual(Color.white, uiTextShape.referencesContainer.text.color);
            Assert.AreEqual(100f, uiTextShape.referencesContainer.text.fontSize);
            Assert.AreEqual("", uiTextShape.referencesContainer.text.text);
            Assert.AreEqual(1, uiTextShape.referencesContainer.text.maxVisibleLines);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.lineSpacing);
            Assert.IsFalse(uiTextShape.referencesContainer.text.enableAutoSizing);
            Assert.IsFalse(uiTextShape.referencesContainer.text.enableWordWrapping);
            Assert.IsFalse(uiTextShape.referencesContainer.text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"));

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiTextShape.transform.rect);
            Assert.AreEqual(alignedPosition.ToString(), uiTextShape.transform.anchoredPosition.ToString());

            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.x);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.y);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.z);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.w);

            // Update UITextShape
            uiTextShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiTextShape.id,
                json = JsonUtility.ToJson(new UITextShape.Model
                {
                    isPointerBlocker = true,
                    hAlign = "left",
                    vAlign = "bottom",
                    textModel = new TextShape.Model
                    {
                        value = "hello world",
                        color = Color.green,
                        opacity = 0.5f,
                        fontSize = 35f,
                        hTextAlign = 0,
                        vTextAlign = 0,
                        paddingTop = 10f,
                        paddingRight = 30f,
                        paddingBottom = 20f,
                        paddingLeft = 15,
                        lineSpacing = 0.1f,
                        lineCount = 3,
                        shadowOffsetX = 0.1f,
                        shadowOffsetY = 0.1f,
                        shadowColor = Color.yellow,
                        textWrapping = true
                    }
                })
            })) as UITextShape;

            yield return uiTextShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiTextShape.referencesContainer.transform.parent == screenSpaceShape.transform);
            Assert.IsTrue(uiTextShape.referencesContainer.text.raycastTarget);
            Assert.AreEqual(100f, uiTextShape.transform.rect.width);
            Assert.AreEqual(100f, uiTextShape.transform.rect.height);
            Assert.AreEqual("hello world", uiTextShape.referencesContainer.text.text);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enabled);
            Assert.AreEqual(new Color(0f, 1f, 0f, 0.5f), uiTextShape.referencesContainer.text.color);
            Assert.AreEqual(35f, uiTextShape.referencesContainer.text.fontSize);
            Assert.AreEqual(3, uiTextShape.referencesContainer.text.maxVisibleLines);
            Assert.AreEqual(0.1f, uiTextShape.referencesContainer.text.lineSpacing);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enableWordWrapping);
            Assert.IsTrue(uiTextShape.referencesContainer.text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"));
            Assert.AreEqual(Color.yellow, uiTextShape.referencesContainer.text.fontMaterial.GetColor("_UnderlayColor"));

            alignedPosition = CalculateAlignedPosition(screenSpaceShape.transform.rect, uiTextShape.transform.rect, "bottom", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiTextShape.transform.anchoredPosition.ToString());

            Assert.AreEqual(15f, uiTextShape.referencesContainer.text.margin.x);
            Assert.AreEqual(10f, uiTextShape.referencesContainer.text.margin.y);
            Assert.AreEqual(30f, uiTextShape.referencesContainer.text.margin.z);
            Assert.AreEqual(20f, uiTextShape.referencesContainer.text.margin.w);
        }
    }
}
