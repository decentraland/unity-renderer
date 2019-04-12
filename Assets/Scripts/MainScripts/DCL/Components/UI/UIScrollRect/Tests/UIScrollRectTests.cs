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
    public class UIScrollRectTests : TestsBase
    {
        [UnityTest]
        public IEnumerator UIScrollRectPropertiesAreAppliedCorrectly()
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

            UIScrollRect scrRect = TestHelpers.SharedComponentCreate<UIScrollRect, UIScrollRect.Model>(scene, CLASS_ID.UI_SLIDER_SHAPE);
            yield return scrRect.routine;

            var refC = scrRect.referencesContainer;

            Assert.IsTrue(refC.canvasGroup.blocksRaycasts);
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
            TestHelpers.SharedComponentUpdate(scene, scrRect,
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

            yield return scrRect.routine;

            // Check updated properties are applied correctly
            Assert.IsTrue(scrRect.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsFalse(scrRect.referencesContainer.canvasGroup.blocksRaycasts);
            Assert.AreEqual(275f, scrRect.childHookRectTransform.rect.width);
            Assert.AreEqual(130f, scrRect.childHookRectTransform.rect.height);
            Assert.IsTrue(scrRect.referencesContainer.alignmentLayoutGroup.childAlignment == TextAnchor.LowerRight);

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, scrRect.childHookRectTransform.rect, "bottom", "right");
            alignedPosition += new Vector2(-30, -15); // apply offset position

            Assert.AreEqual(alignedPosition.ToString(), scrRect.referencesContainer.layoutElementRT.anchoredPosition.ToString());

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIScrollRectMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            //// Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<UIScrollRect.Model, UIScrollRect>(scene, CLASS_ID.UI_SLIDER_SHAPE);
        }

        [UnityTest]
        public IEnumerator UIScrollRectShapeNormalizedSize()
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
