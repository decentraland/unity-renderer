using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UIScrollRectTests : UITestsBase
    {
        [UnityTest]
        public IEnumerator TestPropertiesAreAppliedCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            UIScrollRect scrollRect =
                TestUtils.SharedComponentCreate<UIScrollRect, UIScrollRect.Model>(scene, CLASS_ID.UI_SLIDER_SHAPE);
            yield return scrollRect.routine;

            // Force a new update to pass the first apply
            yield return TestUtils.SharedComponentUpdate(scrollRect, new UIScrollRect.Model
            {
                name = "newName"
            });

            var refC = scrollRect.referencesContainer;

            Assert.IsTrue(refC.canvasGroup.blocksRaycasts);
            //Canvas group is disabled on first apply
            Assert.AreEqual(1, refC.canvasGroup.alpha);

            // Apply padding
            Assert.AreEqual(0, refC.paddingLayoutGroup.padding.bottom);
            Assert.AreEqual(0, refC.paddingLayoutGroup.padding.top);
            Assert.AreEqual(0, refC.paddingLayoutGroup.padding.left);
            Assert.AreEqual(0, refC.paddingLayoutGroup.padding.right);

            Assert.IsFalse(refC.scrollRect.horizontal);
            Assert.IsTrue(refC.scrollRect.vertical);

            Assert.AreEqual(0, refC.HScrollbar.value);
            Assert.AreEqual(0, refC.VScrollbar.value);

            // Update UIScrollRect properties
            yield return TestUtils.SharedComponentUpdate(scrollRect,
                new UIScrollRect.Model
                {
                    parentComponent = screenSpaceShape.id,
                    isPointerBlocker = false,
                    width = new UIValue(275f),
                    height = new UIValue(130f),
                    positionX = new UIValue(-30f),
                    positionY = new UIValue(-15f),
                    hAlign = "right",
                    vAlign = "bottom"
                });

            yield return null;
            yield return new WaitUntil( () => !scrollRect.isLayoutDirty );


            // Check updated properties are applied correctly
            Assert.IsTrue(scrollRect.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsFalse(scrollRect.referencesContainer.canvasGroup.blocksRaycasts);
            Assert.AreEqual(275f, scrollRect.childHookRectTransform.rect.width);
            Assert.AreEqual(130f, scrollRect.childHookRectTransform.rect.height);
            Assert.IsTrue(scrollRect.referencesContainer.layoutGroup.childAlignment == TextAnchor.LowerRight);

            Vector2 alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                scrollRect.childHookRectTransform.rect, "bottom", "right");
            alignedPosition += new Vector2(-30, -15); // apply offset position

            Assert.AreEqual(alignedPosition.ToString(),
                scrollRect.referencesContainer.layoutElementRT.anchoredPosition.ToString());

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestOnClickEvent()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            UIScrollRect scrRect =
                TestUtils.SharedComponentCreate<UIScrollRect, UIScrollRect.Model>(scene, CLASS_ID.UI_SLIDER_SHAPE);
            yield return scrRect.routine;

            // Force a new update to pass the first apply
            yield return TestUtils.SharedComponentUpdate(scrRect, new UIScrollRect.Model
            {
                name = "newName"
            });

            // Update UIScrollRect properties
            yield return TestUtils.SharedComponentUpdate(scrRect,
                new UIScrollRect.Model
                {
                    parentComponent = screenSpaceShape.id,
                    isPointerBlocker = false,
                    width = new UIValue(275f),
                    height = new UIValue(130f),
                    positionX = new UIValue(-30f),
                    positionY = new UIValue(-15f),
                    hAlign = "right",
                    vAlign = "bottom",
                    onClick = "UUIDFakeEventId"
                });

            //------------------------------------------------------------------------
            // Test click events
            bool eventResult = false;

            yield return TestUtils.TestUIClickEventPropagation(
                scene.sceneData.id,
                scrRect.model.onClick,
                scrRect.referencesContainer.childHookRectTransform,
                (bool res) =>
                {
                    // Check image object clicking triggers the correct event
                    eventResult = res;
                });

            Assert.IsTrue(eventResult);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestMissingValuesGetDefaultedOnUpdate()
        {
            //// Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            yield return TestUtils.TestSharedComponentDefaultsOnUpdate<UIScrollRect.Model, UIScrollRect>(scene,
                CLASS_ID.UI_SLIDER_SHAPE);
        }

        [UnityTest]
        public IEnumerator AddedCorrectlyOnInvisibleParent() { yield return TestUtils.TestUIElementAddedCorrectlyOnInvisibleParent<UIScrollRect, UIScrollRect.Model>(scene, CLASS_ID.UI_SLIDER_SHAPE); }

        [UnityTest]
        [Explicit]
        public IEnumerator TestNormalizedSize()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            UIContainerRect uiContainerRectShape = TestUtils.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                CLASS_ID.UI_CONTAINER_RECT);

            yield return uiContainerRectShape.routine;

            // Update UIContainerRectShape properties
            yield return TestUtils.SharedComponentUpdate(uiContainerRectShape,
                new UIContainerRect.Model
                {
                    parentComponent = screenSpaceShape.id,
                    width = new UIValue(50, UIValue.Unit.PERCENT),
                    height = new UIValue(30, UIValue.Unit.PERCENT)
                });

            yield return uiContainerRectShape.routine;

            UnityEngine.UI.Image image =
                uiContainerRectShape.childHookRectTransform.GetComponent<UnityEngine.UI.Image>();

            // Check updated properties are applied correctly
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.width * 0.5f,
                uiContainerRectShape.childHookRectTransform.rect.width, 0.01f);
            Assert.AreEqual(screenSpaceShape.childHookRectTransform.rect.height * 0.3f,
                uiContainerRectShape.childHookRectTransform.rect.height, 0.01f);

            yield return new WaitForAllMessagesProcessed();
            screenSpaceShape.Dispose();
        }
    }
}