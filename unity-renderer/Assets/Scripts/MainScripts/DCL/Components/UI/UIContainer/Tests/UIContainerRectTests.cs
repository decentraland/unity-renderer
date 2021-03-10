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
    public class UIContainerRectTests : UITestsBase
    {
        [UnityTest]
        public IEnumerator TestPropertiesAreAppliedCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            UIContainerRect uiContainerRectShape =
                TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                    CLASS_ID.UI_CONTAINER_RECT);

            yield return uiContainerRectShape.routine;

            UnityEngine.UI.Image image = uiContainerRectShape.referencesContainer.image;

            // Check default properties are applied correctly
            Assert.AreEqual(0f, uiContainerRectShape.referencesContainer.canvasGroup.alpha);
            Assert.IsTrue(image.GetComponent<Outline>() == null);
            Assert.IsTrue(image.color == new Color(0f, 0f, 0f, 0f));
            Assert.IsTrue(uiContainerRectShape.referencesContainer.canvasGroup.blocksRaycasts);
            Assert.AreEqual(100f, uiContainerRectShape.childHookRectTransform.rect.width);
            Assert.AreEqual(50f, uiContainerRectShape.childHookRectTransform.rect.height);
            Assert.AreEqual(Vector3.zero, uiContainerRectShape.childHookRectTransform.localPosition);

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(uiContainerRectShape.id, JsonUtility.ToJson(new UIContainerRect.Model
            {
                parentComponent = screenSpaceShape.id,
                thickness = 5,
                color = new Color(0.2f, 0.7f, 0.05f, 1f),
                isPointerBlocker = false,
                width = new UIValue(275f),
                height = new UIValue(130f),
                positionX = new UIValue(-30f),
                positionY = new UIValue(-15f),
                hAlign = "right",
                vAlign = "bottom"
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            // Check updated properties are applied correctly
            Assert.AreEqual(1f, uiContainerRectShape.referencesContainer.canvasGroup.alpha);
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(image.GetComponent<Outline>() != null);
            Assert.IsTrue(image.color == new Color(0.2f, 0.7f, 0.05f, 1f));
            Assert.IsFalse(uiContainerRectShape.referencesContainer.canvasGroup.blocksRaycasts);
            Assert.AreEqual(275f, uiContainerRectShape.childHookRectTransform.rect.width);
            Assert.AreEqual(130f, uiContainerRectShape.childHookRectTransform.rect.height);
            Assert.IsTrue(uiContainerRectShape.referencesContainer.layoutGroup.childAlignment == TextAnchor.LowerRight);

            Vector2 alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiContainerRectShape.childHookRectTransform.rect, "bottom", "right");
            alignedPosition += new Vector2(-30, -15); // apply offset position

            Assert.AreEqual(alignedPosition.ToString(),
                uiContainerRectShape.childHookRectTransform.anchoredPosition.ToString());

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestParenting()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            UIContainerRect uiContainerRectShape =
                TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                    CLASS_ID.UI_CONTAINER_RECT);
            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape parent
            scene.SharedComponentUpdate(uiContainerRectShape.id, JsonUtility.ToJson(new UIContainerRect.Model
            {
                parentComponent = screenSpaceShape.id,
            }));
            yield return uiContainerRectShape.routine;

            // Check updated parent
            Assert.IsTrue(uiContainerRectShape.referencesContainer.transform.parent ==
                          screenSpaceShape.childHookRectTransform);

            // Create 2nd UIContainerRectShape
            UIContainerRect uiContainerRectShape2 =
                TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                    CLASS_ID.UI_CONTAINER_RECT);
            yield return uiContainerRectShape2.routine;

            // Update UIContainerRectShape parent to the previous container
            scene.SharedComponentUpdate(uiContainerRectShape2.id, JsonUtility.ToJson(new UIContainerRect.Model
            {
                parentComponent = uiContainerRectShape.id,
            }));
            yield return uiContainerRectShape2.routine;

            // Check updated parent
            Assert.IsTrue(uiContainerRectShape2.referencesContainer.transform.parent ==
                          uiContainerRectShape.childHookRectTransform);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestMissingValuesGetDefaultedOnUpdate()
        {
            //// Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<UIContainerRect.Model, UIContainerRect>(scene,
                CLASS_ID.UI_CONTAINER_RECT);
        }

        [UnityTest]
        public IEnumerator AddedCorrectlyOnInvisibleParent()
        {
            yield return TestHelpers.TestUIElementAddedCorrectlyOnInvisibleParent<UIContainerRect, UIContainerRect.Model>(scene, CLASS_ID.UI_CONTAINER_RECT);
        }

        [UnityTest]
        public IEnumerator TestNormalizedSize()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            UIContainerRect uiContainerRectShape =
                TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                    CLASS_ID.UI_CONTAINER_RECT);
            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(uiContainerRectShape.id, JsonUtility.ToJson(new UIContainerRect.Model
            {
                parentComponent = screenSpaceShape.id,
                width = new UIValue(50, UIValue.Unit.PERCENT),
                height = new UIValue(30, UIValue.Unit.PERCENT)
            })) as UIContainerRect;
            yield return uiContainerRectShape.routine;

            UnityEngine.UI.Image image =
                uiContainerRectShape.childHookRectTransform.GetComponent<UnityEngine.UI.Image>();

            // Check updated properties are applied correctly
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.width * 0.5f,
                uiContainerRectShape.childHookRectTransform.rect.width, 0.01f);
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.height * 0.3f,
                uiContainerRectShape.childHookRectTransform.rect.height, 0.01f);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestOnClickEvent()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            UIContainerRect uiContainerRectShape =
                TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                    CLASS_ID.UI_CONTAINER_RECT);

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            uiContainerRectShape = scene.SharedComponentUpdate(uiContainerRectShape.id, JsonUtility.ToJson(new UIContainerRect.Model
            {
                parentComponent = screenSpaceShape.id,
                thickness = 5,
                color = new Color(0.2f, 0.7f, 0.05f, 1f),
                isPointerBlocker = false,
                width = new UIValue(275f),
                height = new UIValue(130f),
                positionX = new UIValue(-30f),
                positionY = new UIValue(-15f),
                hAlign = "right",
                vAlign = "bottom",
                onClick = "UUIDFakeEventId"
            })) as UIContainerRect;

            yield return uiContainerRectShape.routine;

            //------------------------------------------------------------------------
            // Test click events
            bool eventResult = false;

            yield return TestHelpers.TestUIClickEventPropagation(
                scene.sceneData.id,
                uiContainerRectShape.model.onClick,
                uiContainerRectShape.referencesContainer.childHookRectTransform,
                (bool res) =>
                {
                    // Check image object clicking triggers the correct event
                    eventResult = res;
                });

            Assert.IsTrue(eventResult);
        }
    }
}
