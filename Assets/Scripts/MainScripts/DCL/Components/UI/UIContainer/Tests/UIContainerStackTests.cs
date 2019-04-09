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
    public class UIContainerStackTests : TestsBase
    {
        [UnityTest]
        public IEnumerator PropertiesAreAppliedCorrectly()
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

            // Create UIContainerStack
            UIContainerStack uiContainerStack = TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene, CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;

            UnityEngine.UI.Image image = uiContainerStack.referencesContainer.image;

            // Check default properties are applied correctly
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 1f));
            Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(100f, uiContainerStack.childHookRectTransform.rect.width);
            Assert.AreEqual(100f, uiContainerStack.childHookRectTransform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerStack.childHookRectTransform.localPosition);

            // Update UIContainerStack properties
            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerStack.id,
                json = JsonUtility.ToJson(new UIContainerStack.Model
                {
                    parentComponent = screenSpaceShape.id,
                    color = new Color(0.2f, 0.7f, 0.05f, 1f),
                    isPointerBlocker = true,
                    width = new UIValue(275f),
                    height = new UIValue(130f),
                    positionX = new UIValue(-30f),
                    positionY = new UIValue(-15f),
                    hAlign = "right",
                    vAlign = "bottom"
                })
            }));
            yield return uiContainerStack.routine;

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerStack.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(image.color == new Color(0.2f, 0.7f, 0.05f, 1f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(275f, uiContainerStack.childHookRectTransform.rect.width);
            Assert.AreEqual(130f, uiContainerStack.childHookRectTransform.rect.height);
            Assert.IsTrue(uiContainerStack.referencesContainer.alignmentLayoutGroup.childAlignment == TextAnchor.LowerRight);

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiContainerStack.childHookRectTransform.rect, "bottom", "right");
            alignedPosition += new Vector2(-30, -15); // apply offset position

            Assert.AreEqual(alignedPosition.ToString(), uiContainerStack.childHookRectTransform.anchoredPosition.ToString());

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator MissingValuesGetDefaultedOnUpdate()
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
            UIContainerStack uiContainerStack = TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene, CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;

            // Update UIContainerRectShape properties
            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerStack.id,
                json = JsonUtility.ToJson(new UIContainerStack.Model
                {
                    parentComponent = screenSpaceShape.id,
                    color = new Color(0.5f, 0.8f, 0.1f, 1f),
                    isPointerBlocker = true,
                    width = new UIValue(200f),
                    height = new UIValue(150f),
                    positionX = new UIValue(20f),
                    positionY = new UIValue(45f)
                })
            }));
            yield return uiContainerStack.routine;

            UnityEngine.UI.Image image = uiContainerStack.referencesContainer.image;

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerStack.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(image.color == new Color(0.5f, 0.8f, 0.1f, 1f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(200f, uiContainerStack.childHookRectTransform.rect.width);
            Assert.AreEqual(150f, uiContainerStack.childHookRectTransform.rect.height);
            Assert.AreEqual(new Vector3(20f, 45f, 0f), uiContainerStack.childHookRectTransform.localPosition);

            // Update UIContainerRectShape with missing values
            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerStack.id,
                json = JsonUtility.ToJson(new UIContainerStack.Model
                {
                    parentComponent = screenSpaceShape.id
                })
            }));
            yield return uiContainerStack.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 1f));
            Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(100f, uiContainerStack.childHookRectTransform.rect.width);
            Assert.AreEqual(100f, uiContainerStack.childHookRectTransform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerStack.childHookRectTransform.localPosition);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator NormalizedSize()
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

            // Create UIContainerStack
            UIContainerStack uiContainerStack = TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene, CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;

            // Update UIContainerStack properties
            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerStack.id,
                json = JsonUtility.ToJson(new UIContainerStack.Model
                {
                    parentComponent = screenSpaceShape.id,
                    width = new UIValue(50, UIValue.Unit.PERCENT),
                    height = new UIValue(30, UIValue.Unit.PERCENT)
                })
            }));
            yield return uiContainerStack.routine;

            UnityEngine.UI.Image image = uiContainerStack.childHookRectTransform.GetComponent<UnityEngine.UI.Image>();

            // Check updated properties are applied correctly
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.width * 0.5f, uiContainerStack.childHookRectTransform.rect.width);
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.height * 0.3f, uiContainerStack.childHookRectTransform.rect.height);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator VerticalStackIsAppliedCorrectly()
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

            // Create UIContainerStack
            UIContainerStack uiContainerStack = TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene, CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerStack.id,
                json = JsonUtility.ToJson(new UIContainerStack.Model
                {
                    width = new UIValue(500f),
                    height = new UIValue(300f)
                })
            }));
            yield return uiContainerStack.routine;

            // Check container stack was initialized correctly
            Assert.IsTrue(uiContainerStack != null);

            var layoutGroup = uiContainerStack.referencesContainer.GetComponentInChildren<VerticalLayoutGroup>();
            Assert.IsTrue(layoutGroup != null);
            Assert.IsFalse(layoutGroup.childControlHeight);
            Assert.IsFalse(layoutGroup.childControlWidth);
            Assert.IsFalse(layoutGroup.childForceExpandWidth);
            Assert.IsFalse(layoutGroup.childForceExpandHeight);

            // Create 1st child object
            UIContainerRect childComponent1 = TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene, CLASS_ID.UI_CONTAINER_RECT);
            yield return childComponent1.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent1.id,
                json = JsonUtility.ToJson(new UIContainerRect.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(130f),
                    height = new UIValue(70f)
                })
            }));
            yield return childComponent1.routine;

            Assert.IsTrue(childComponent1.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            // Create 2nd child object
            UIImage childComponent2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return childComponent2.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent2.id,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(75f),
                    height = new UIValue(35f)
                })
            }));
            yield return childComponent2.routine;

            Assert.IsTrue(childComponent2.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            // Create 3rd child object
            UIInputText childComponent3 = TestHelpers.SharedComponentCreate<UIInputText, UIInputText.Model>(scene, CLASS_ID.UI_INPUT_TEXT_SHAPE);
            yield return childComponent3.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent3.id,
                json = JsonUtility.ToJson(new UIInputText.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(150f),
                    height = new UIValue(50f)
                })
            }));
            yield return childComponent3.routine;

            Assert.IsTrue(childComponent3.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            // Check stacked components position
            Assert.AreEqual(-childComponent1.referencesContainer.rectTransform.rect.height / 2, childComponent1.referencesContainer.rectTransform.anchoredPosition.y);
            Assert.AreEqual(childComponent1.referencesContainer.rectTransform.anchoredPosition.y - childComponent1.referencesContainer.rectTransform.rect.height / 2
                            - childComponent2.referencesContainer.rectTransform.rect.height / 2, childComponent2.referencesContainer.rectTransform.anchoredPosition.y);
            Assert.AreEqual(childComponent2.referencesContainer.rectTransform.anchoredPosition.y - childComponent2.referencesContainer.rectTransform.rect.height / 2
                            - childComponent3.referencesContainer.rectTransform.rect.height / 2, childComponent3.referencesContainer.rectTransform.anchoredPosition.y);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator HorizontalStackIsAppliedCorrectly()
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

            // Create UIContainerStack
            UIContainerStack uiContainerStack = TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene, CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerStack.id,
                json = JsonUtility.ToJson(new UIContainerStack.Model
                {
                    width = new UIValue(500f),
                    height = new UIValue(300f),
                    stackOrientation = UIContainerStack.StackOrientation.HORIZONTAL
                })
            }));
            yield return uiContainerStack.routine;

            // Check container stack was initialized correctly
            Assert.IsTrue(uiContainerStack != null);

            var layoutGroup = uiContainerStack.referencesContainer.GetComponentInChildren<HorizontalLayoutGroup>();
            Assert.IsTrue(layoutGroup != null);
            Assert.IsFalse(layoutGroup.childControlHeight);
            Assert.IsFalse(layoutGroup.childControlWidth);
            Assert.IsFalse(layoutGroup.childForceExpandWidth);
            Assert.IsFalse(layoutGroup.childForceExpandHeight);

            // Create 1st child object
            UIContainerRect childComponent1 = TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene, CLASS_ID.UI_CONTAINER_RECT);
            yield return childComponent1.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent1.id,
                json = JsonUtility.ToJson(new UIContainerRect.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(130f),
                    height = new UIValue(70f)
                })
            }));
            yield return childComponent1.routine;

            Assert.IsTrue(childComponent1.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            // Create 2nd child object
            UIImage childComponent2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return childComponent2.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent2.id,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(75f),
                    height = new UIValue(35f)
                })
            }));
            yield return childComponent2.routine;

            Assert.IsTrue(childComponent2.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            /// Create 3rd child object
            UIInputText childComponent3 = TestHelpers.SharedComponentCreate<UIInputText, UIInputText.Model>(scene, CLASS_ID.UI_INPUT_TEXT_SHAPE);
            yield return childComponent3.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent3.id,
                json = JsonUtility.ToJson(new UIInputText.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(150f),
                    height = new UIValue(50f)
                })
            }));
            yield return childComponent3.routine;

            Assert.IsTrue(childComponent3.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            // Check stacked components position
            Assert.AreEqual(childComponent1.referencesContainer.rectTransform.rect.width / 2, childComponent1.referencesContainer.rectTransform.anchoredPosition.x);
            Assert.AreEqual(childComponent1.referencesContainer.rectTransform.anchoredPosition.x + childComponent1.referencesContainer.rectTransform.rect.width / 2
                            + childComponent2.referencesContainer.rectTransform.rect.width / 2, childComponent2.referencesContainer.rectTransform.anchoredPosition.x);
            Assert.AreEqual(childComponent2.referencesContainer.rectTransform.anchoredPosition.x + childComponent2.referencesContainer.rectTransform.rect.width / 2
                            + childComponent3.referencesContainer.rectTransform.rect.width / 2, childComponent3.referencesContainer.rectTransform.anchoredPosition.x);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator AdaptSizeIsAppliedCorrectly()
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

            // Create UIContainerStack
            UIContainerStack uiContainerStack = TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene, CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerStack.id,
                json = JsonUtility.ToJson(new UIContainerStack.Model
                {
                    width = new UIValue(500f),
                    height = new UIValue(300f)
                })
            }));
            yield return uiContainerStack.routine;

            // Check container stack was initialized correctly
            Assert.IsTrue(uiContainerStack != null);

            var layoutGroup = uiContainerStack.referencesContainer.GetComponentInChildren<VerticalLayoutGroup>();
            Assert.IsTrue(layoutGroup != null);
            Assert.IsFalse(layoutGroup.childControlHeight);
            Assert.IsFalse(layoutGroup.childControlWidth);
            Assert.IsFalse(layoutGroup.childForceExpandWidth);
            Assert.IsFalse(layoutGroup.childForceExpandHeight);

            // Create 1st child object
            UIContainerRect childComponent1 = TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene, CLASS_ID.UI_CONTAINER_RECT);
            yield return childComponent1.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent1.id,
                json = JsonUtility.ToJson(new UIContainerRect.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(130f),
                    height = new UIValue(70f)
                })
            }));
            yield return childComponent1.routine;

            Assert.IsTrue(childComponent1.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            // Create 2nd child object
            UIImage childComponent2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return childComponent2.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent2.id,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(75f),
                    height = new UIValue(35f)
                })
            }));
            yield return childComponent2.routine;

            Assert.IsTrue(childComponent2.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            // Create 3rd child object
            UIInputText childComponent3 = TestHelpers.SharedComponentCreate<UIInputText, UIInputText.Model>(scene, CLASS_ID.UI_INPUT_TEXT_SHAPE);
            yield return childComponent3.routine;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = childComponent3.id,
                json = JsonUtility.ToJson(new UIInputText.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(150f),
                    height = new UIValue(50f)
                })
            }));
            yield return childComponent3.routine;

            Assert.IsTrue(childComponent3.referencesContainer.transform.parent == uiContainerStack.referencesContainer.childHookRectTransform);

            // Trigger container stack size adaptation
            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiContainerStack.id,
                json = JsonUtility.ToJson(new UIContainerStack.Model
                {
                    adaptHeight = true,
                    adaptWidth = true,
                })
            }));
            yield return uiContainerStack.routine;

            yield return null;

            // Check stacked components position
            Assert.AreEqual(150f, uiContainerStack.childHookRectTransform.rect.width);
            Assert.AreEqual(childComponent1.referencesContainer.rectTransform.rect.height + childComponent2.referencesContainer.rectTransform.rect.height
                + childComponent3.referencesContainer.rectTransform.rect.height, uiContainerStack.childHookRectTransform.rect.height);

            screenSpaceShape.Dispose();
        }
    }
}
