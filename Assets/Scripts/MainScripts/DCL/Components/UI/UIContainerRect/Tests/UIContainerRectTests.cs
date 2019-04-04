using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class UIContainerRectTests : TestsBase
    {
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
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRect uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRect;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new UIContainerRect.Model { })
            }));

            yield return uiContainerRectShape.routine;

            UnityEngine.UI.Image image = uiContainerRectShape.referencesContainer.image;

            // Check default properties are applied correctly
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 1f));
            Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(100f, uiContainerRectShape.childHookRectTransform.rect.width);
            Assert.AreEqual(100f, uiContainerRectShape.childHookRectTransform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerRectShape.childHookRectTransform.localPosition);

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new UIContainerRect.Model
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
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(0.2f * 255f, 0.7f * 255f, 0.05f * 255f, 1f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(275f, uiContainerRectShape.childHookRectTransform.rect.width);
            Assert.AreEqual(130f, uiContainerRectShape.childHookRectTransform.rect.height);
            Assert.IsTrue(uiContainerRectShape.referencesContainer.alignmentLayoutGroup.childAlignment == TextAnchor.LowerRight);

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiContainerRectShape.childHookRectTransform.rect, "bottom", "right");
            alignedPosition += new Vector2(-30, -15); // apply offset position

            Assert.AreEqual(alignedPosition.ToString(), uiContainerRectShape.childHookRectTransform.anchoredPosition.ToString());

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
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRect uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape parent
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new UIContainerRect.Model
                {
                    parentComponent = screenSpaceShape.id,
                })
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            // Check updated parent
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);

            // Create 2nd UIContainerRectShape
            string uiContainerRectShape2Id = "uiContainerRectShape2";

            UIContainerRect uiContainerRectShape2 = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShape2Id,
                name = "UIContainerRectShape2"
            })) as UIContainerRect;

            yield return uiContainerRectShape2.routine;

            // Update UIContainerRectShape parent to the previous container
            uiContainerRectShape2 = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShape2Id,
                json = JsonUtility.ToJson(new UIContainerRect.Model
                {
                    parentComponent = uiContainerRectShapeId,
                })
            })) as UIContainerRect;

            yield return uiContainerRectShape2.routine;

            // Check updated parent
            Assert.IsTrue(uiContainerRectShape2.referencesContainer.transform.parent == uiContainerRectShape.childHookRectTransform);

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
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRect uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new UIContainerRect.Model
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
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            UnityEngine.UI.Image image = uiContainerRectShape.referencesContainer.image;

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(0.5f * 255f, 0.8f * 255f, 0.1f * 255f, 1f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(200f, uiContainerRectShape.childHookRectTransform.rect.width);
            Assert.AreEqual(150f, uiContainerRectShape.childHookRectTransform.rect.height);
            Assert.AreEqual(new Vector3(20f, 45f, 0f), uiContainerRectShape.childHookRectTransform.localPosition);

            // Update UIContainerRectShape with missing values
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new UIContainerRect.Model
                {
                    parentComponent = screenSpaceShape.id
                })
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 1f));
            Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(100f, uiContainerRectShape.childHookRectTransform.rect.width);
            Assert.AreEqual(100f, uiContainerRectShape.childHookRectTransform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerRectShape.childHookRectTransform.localPosition);

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
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiContainerRectShapeId = "uiContainerRectShape";

            UIContainerRect uiContainerRectShape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_CONTAINER_RECT,
                id = uiContainerRectShapeId,
                name = "UIContainerRectShape"
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerRectShapeId,
                json = JsonUtility.ToJson(new UIContainerRect.Model
                {
                    parentComponent = screenSpaceShape.id,
                    width = new UIValue(50, UIValue.Unit.PERCENT),
                    height = new UIValue(30, UIValue.Unit.PERCENT)
                })
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            UnityEngine.UI.Image image = uiContainerRectShape.childHookRectTransform.GetComponent<UnityEngine.UI.Image>();

            // Check updated properties are applied correctly
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.width * 0.5f, uiContainerRectShape.childHookRectTransform.rect.width);
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.height * 0.3f, uiContainerRectShape.childHookRectTransform.rect.height);

            screenSpaceShape.Dispose();
        }
    }
}
