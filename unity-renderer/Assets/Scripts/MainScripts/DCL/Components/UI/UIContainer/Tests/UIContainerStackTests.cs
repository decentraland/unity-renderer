using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class UIContainerStackTests : UITestsBase
    {
        [UnityTest]
        public IEnumerator PropertiesAreAppliedCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIContainerStack
            UIContainerStack uiContainerStack =
                TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene,
                    CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;

            UnityEngine.UI.Image image = uiContainerStack.referencesContainer.image;

            // Check default properties are applied correctly
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 0f));
            Assert.IsTrue(uiContainerStack.referencesContainer.canvasGroup.blocksRaycasts);
            Assert.AreEqual(100f, uiContainerStack.childHookRectTransform.rect.width);
            Assert.AreEqual(50f, uiContainerStack.childHookRectTransform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerStack.childHookRectTransform.localPosition);

            yield return TestHelpers.SharedComponentUpdate(uiContainerStack,
                new UIContainerStack.Model
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
                });

            // Check updated properties are applied correctly
            Assert.IsTrue(uiContainerStack.referencesContainer.transform.parent ==
                          screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(image.color == new Color(0.2f, 0.7f, 0.05f, 1f));
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(275f, uiContainerStack.childHookRectTransform.rect.width);
            Assert.AreEqual(130f, uiContainerStack.childHookRectTransform.rect.height);
            Assert.IsTrue(uiContainerStack.referencesContainer.layoutGroup.childAlignment == TextAnchor.LowerRight);

            Vector2 alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiContainerStack.childHookRectTransform.rect, "bottom", "right");
            alignedPosition += new Vector2(-30, -15); // apply offset position

            Assert.AreEqual(alignedPosition.ToString(),
                uiContainerStack.childHookRectTransform.anchoredPosition.ToString());

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator MissingValuesGetDefaultedOnUpdate()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<UIContainerStack.Model, UIContainerStack>(
                scene, CLASS_ID.UI_CONTAINER_STACK);
        }

        [UnityTest]
        public IEnumerator AddedCorrectlyOnInvisibleParent()
        {
            yield return TestHelpers.TestUIElementAddedCorrectlyOnInvisibleParent<UIContainerStack, UIContainerStack.Model>(scene, CLASS_ID.UI_CONTAINER_STACK);
        }

        [UnityTest]
        public IEnumerator NormalizedSize()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIContainerStack
            UIContainerStack uiContainerStack =
                TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene,
                    CLASS_ID.UI_CONTAINER_STACK,
                    new UIContainerStack.Model
                    {
                        parentComponent = screenSpaceShape.id,
                        width = new UIValue(50, UIValue.Unit.PERCENT),
                        height = new UIValue(30, UIValue.Unit.PERCENT)
                    });
            yield return uiContainerStack.routine;

            UnityEngine.UI.Image image = uiContainerStack.childHookRectTransform.GetComponent<UnityEngine.UI.Image>();

            // Check updated properties are applied correctly
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.width * 0.5f,
                uiContainerStack.childHookRectTransform.rect.width, 0.01f);
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.height * 0.3f,
                uiContainerStack.childHookRectTransform.rect.height, 0.01f);

            screenSpaceShape.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestChildrenAreHandledCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIContainerStack
            UIContainerStack uiContainerStack =
                TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene,
                    CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;


            // Create 1st child object
            UIContainerRect childComponent1 = TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(
                scene, CLASS_ID.UI_CONTAINER_RECT,
                new UIContainerRect.Model()
                {
                    parentComponent = uiContainerStack.id
                });

            yield return childComponent1.routine;

            Assert.IsTrue(uiContainerStack.stackContainers.ContainsKey(childComponent1.id));
            Assert.IsTrue(childComponent1.referencesContainer.transform.parent.gameObject ==
                          uiContainerStack.stackContainers[childComponent1.id]);

            // Create 2nd child object
            UIImage childComponent2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene,
                CLASS_ID.UI_IMAGE_SHAPE,
                new UIImage.Model
                {
                    parentComponent = uiContainerStack.id,
                });

            yield return childComponent2.routine;

            Assert.IsTrue(uiContainerStack.stackContainers.ContainsKey(childComponent2.id));
            Assert.IsTrue(childComponent2.referencesContainer.transform.parent.gameObject ==
                          uiContainerStack.stackContainers[childComponent2.id]);

            screenSpaceShape.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator VerticalStackIsAppliedCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIContainerStack
            UIContainerStack uiContainerStack =
                TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene,
                    CLASS_ID.UI_CONTAINER_STACK,
                    new UIContainerStack.Model
                    {
                        width = new UIValue(500f),
                        height = new UIValue(300f)
                    });

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
            UIContainerRect childComponent1 = TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(
                scene, CLASS_ID.UI_CONTAINER_RECT,
                new UIContainerRect.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(130f),
                    height = new UIValue(70f)
                });

            yield return childComponent1.routine;

            // Create 2nd child object
            UIImage childComponent2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene,
                CLASS_ID.UI_IMAGE_SHAPE,
                new UIImage.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(75f),
                    height = new UIValue(35f)
                });

            yield return childComponent2.routine;

            // Create 3rd child object
            UIInputText childComponent3 = TestHelpers.SharedComponentCreate<UIInputText, UIInputText.Model>(scene,
                CLASS_ID.UI_INPUT_TEXT_SHAPE,
                new UIInputText.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(150f),
                    height = new UIValue(50f)
                });

            yield return childComponent3.routine;

            RectTransform child1RT = childComponent1.referencesContainer.transform.parent as RectTransform;
            RectTransform child2RT = childComponent2.referencesContainer.transform.parent as RectTransform;
            RectTransform child3RT = childComponent3.referencesContainer.transform.parent as RectTransform;

            Assert.AreEqual(new Vector2(65, -35).ToString(), child1RT.anchoredPosition.ToString());
            Assert.AreEqual(new Vector2(37.5f, -87.5f).ToString(), child2RT.anchoredPosition.ToString());
            Assert.AreEqual(new Vector2(75, -130).ToString(), child3RT.anchoredPosition.ToString());

            Assert.AreEqual(new Vector2(130, 70).ToString(), child1RT.sizeDelta.ToString());
            Assert.AreEqual(new Vector2(75, 35).ToString(), child2RT.sizeDelta.ToString());
            Assert.AreEqual(new Vector2(150, 50).ToString(), child3RT.sizeDelta.ToString());

            screenSpaceShape.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator HorizontalStackIsAppliedCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIContainerStack
            UIContainerStack uiContainerStack =
                TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene,
                    CLASS_ID.UI_CONTAINER_STACK);
            yield return uiContainerStack.routine;

            yield return TestHelpers.SharedComponentUpdate(uiContainerStack,
                new UIContainerStack.Model
                {
                    width = new UIValue(500f),
                    height = new UIValue(300f),
                    stackOrientation = UIContainerStack.StackOrientation.HORIZONTAL
                });

            // Check container stack was initialized correctly
            Assert.IsTrue(uiContainerStack != null);

            var layoutGroup = uiContainerStack.referencesContainer.GetComponentInChildren<HorizontalLayoutGroup>();
            Assert.IsTrue(layoutGroup != null);
            Assert.IsFalse(layoutGroup.childControlHeight);
            Assert.IsFalse(layoutGroup.childControlWidth);
            Assert.IsFalse(layoutGroup.childForceExpandWidth);
            Assert.IsFalse(layoutGroup.childForceExpandHeight);

            // Create 1st child object
            UIContainerRect childComponent1 = TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(
                scene, CLASS_ID.UI_CONTAINER_RECT,
                new UIContainerRect.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(130f),
                    height = new UIValue(70f)
                });

            yield return childComponent1.routine;

            // Create 2nd child object
            UIImage childComponent2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene,
                CLASS_ID.UI_IMAGE_SHAPE,
                new UIImage.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(75f),
                    height = new UIValue(35f)
                });
            yield return childComponent2.routine;

            /// Create 3rd child object
            UIInputText childComponent3 = TestHelpers.SharedComponentCreate<UIInputText, UIInputText.Model>(scene,
                CLASS_ID.UI_INPUT_TEXT_SHAPE,
                new UIInputText.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(150f),
                    height = new UIValue(50f)
                });

            yield return childComponent3.routine;

            RectTransform child1RT = childComponent1.referencesContainer.transform.parent as RectTransform;
            RectTransform child2RT = childComponent2.referencesContainer.transform.parent as RectTransform;
            RectTransform child3RT = childComponent3.referencesContainer.transform.parent as RectTransform;

            Assert.AreEqual(new Vector2(65, -35).ToString(), child1RT.anchoredPosition.ToString());
            Assert.AreEqual(new Vector2(167.5f, -17.5f).ToString(), child2RT.anchoredPosition.ToString());
            Assert.AreEqual(new Vector2(280, -25).ToString(), child3RT.anchoredPosition.ToString());

            Assert.AreEqual(new Vector2(130, 70).ToString(), child1RT.sizeDelta.ToString());
            Assert.AreEqual(new Vector2(75, 35).ToString(), child2RT.sizeDelta.ToString());
            Assert.AreEqual(new Vector2(150, 50).ToString(), child3RT.sizeDelta.ToString());

            screenSpaceShape.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator AdaptSizeIsAppliedCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIContainerStack
            UIContainerStack uiContainerStack =
                TestHelpers.SharedComponentCreate<UIContainerStack, UIContainerStack.Model>(scene,
                    CLASS_ID.UI_CONTAINER_STACK,
                    new UIContainerStack.Model
                    {
                        width = new UIValue(500f),
                        height = new UIValue(300f)
                    });

            yield return uiContainerStack.routine;

            // Create 1st child object
            UIContainerRect childComponent1 = TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(
                scene, CLASS_ID.UI_CONTAINER_RECT,
                new UIContainerRect.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(130f),
                    height = new UIValue(70f)
                });

            yield return childComponent1.routine;

            // Create 2nd child object
            UIImage childComponent2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene,
                CLASS_ID.UI_IMAGE_SHAPE,
                new UIImage.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(75f),
                    height = new UIValue(35f)
                });
            yield return childComponent2.routine;

            // Create 3rd child object
            UIInputText childComponent3 = TestHelpers.SharedComponentCreate<UIInputText, UIInputText.Model>(scene,
                CLASS_ID.UI_INPUT_TEXT_SHAPE,
                new UIInputText.Model
                {
                    parentComponent = uiContainerStack.id,
                    width = new UIValue(150f),
                    height = new UIValue(50f)
                });
            yield return childComponent3.routine;


            yield return TestHelpers.SharedComponentUpdate(uiContainerStack,
                new UIContainerStack.Model
                {
                    adaptHeight = true,
                    adaptWidth = true,
                });

            yield return null;

            // Check stacked components position
            Assert.AreEqual(150f, uiContainerStack.childHookRectTransform.rect.width, 0.01f);
            Assert.AreEqual(
                childComponent1.referencesContainer.rectTransform.rect.height + childComponent2.referencesContainer
                                                                                  .rectTransform.rect.height
                                                                              + childComponent3.referencesContainer
                                                                                  .rectTransform.rect.height,
                uiContainerStack.childHookRectTransform.rect.height, 0.01f);

            screenSpaceShape.Dispose();
            yield return null;
        }
    }
}
