using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UITextTests : UITestsBase
    {
        [UnityTest]
        public IEnumerator TestPropertiesAreAppliedCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UITextShape
            UIText uiTextShape = TestHelpers.SharedComponentCreate<UIText, UIText.Model>(scene, CLASS_ID.UI_TEXT_SHAPE,
                new UIText.Model { });

            yield return uiTextShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiTextShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(uiTextShape.referencesContainer.canvasGroup.blocksRaycasts);
            Assert.AreEqual(100f, uiTextShape.childHookRectTransform.rect.width);
            Assert.AreEqual(50f, uiTextShape.childHookRectTransform.rect.height);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enabled);
            Assert.AreEqual(Color.white, uiTextShape.referencesContainer.text.color);
            Assert.AreEqual(100f, uiTextShape.referencesContainer.text.fontSize);
            Assert.AreEqual("", uiTextShape.referencesContainer.text.text);
            Assert.AreEqual(int.MaxValue, uiTextShape.referencesContainer.text.maxVisibleLines);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.lineSpacing);
            Assert.IsFalse(uiTextShape.referencesContainer.text.enableAutoSizing);
            Assert.IsFalse(uiTextShape.referencesContainer.text.enableWordWrapping);
            Assert.IsFalse(uiTextShape.referencesContainer.text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"));

            Vector2 alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiTextShape.childHookRectTransform.rect);
            Assert.AreEqual(alignedPosition.ToString(), uiTextShape.childHookRectTransform.anchoredPosition.ToString());

            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.x);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.y);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.z);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.w);

            // Update UITextShape
            yield return TestHelpers.SharedComponentUpdate<UIText>(uiTextShape,
                new UIText.Model
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
                });

            // Check default properties are applied correctly
            Assert.IsTrue(uiTextShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(uiTextShape.referencesContainer.text.raycastTarget);
            Assert.AreEqual(100f, uiTextShape.childHookRectTransform.rect.width);
            Assert.AreEqual(50f, uiTextShape.childHookRectTransform.rect.height);
            Assert.AreEqual("hello world", uiTextShape.referencesContainer.text.text);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enabled);
            Assert.AreEqual(new Color(0f, 1f, 0f, 0.5f), uiTextShape.referencesContainer.text.color);
            Assert.AreEqual(35f, uiTextShape.referencesContainer.text.fontSize);
            Assert.AreEqual(3, uiTextShape.referencesContainer.text.maxVisibleLines);
            Assert.AreEqual(0.1f, uiTextShape.referencesContainer.text.lineSpacing);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enableWordWrapping);
            Assert.IsTrue(uiTextShape.referencesContainer.text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"));
            Assert.AreEqual(Color.yellow, uiTextShape.referencesContainer.text.fontMaterial.GetColor("_UnderlayColor"));

            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiTextShape.childHookRectTransform.rect, "bottom", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiTextShape.childHookRectTransform.anchoredPosition.ToString());

            Assert.AreEqual(15f, uiTextShape.referencesContainer.text.margin.x);
            Assert.AreEqual(10f, uiTextShape.referencesContainer.text.margin.y);
            Assert.AreEqual(30f, uiTextShape.referencesContainer.text.margin.z);
            Assert.AreEqual(20f, uiTextShape.referencesContainer.text.margin.w);
            
            yield return TestHelpers.SharedComponentUpdate<UIText>(uiTextShape,
                new UIText.Model
                {

                    positionX = new UIValue(80),
                    positionY = new UIValue(80),
                });
                
            Assert.AreEqual(80f, uiTextShape.model.positionX.value);
            Assert.AreEqual(80f, uiTextShape.model.positionY.value);
        }

        [UnityTest]
        public IEnumerator TestOnClickEvent()
        {
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            // Create UITextShape
            UIText uiTextShape = TestHelpers.SharedComponentCreate<UIText, UIText.Model>(scene, CLASS_ID.UI_TEXT_SHAPE,
                new UIText.Model { });

            yield return uiTextShape.routine;

            // Update UITextShape
            yield return TestHelpers.SharedComponentUpdate(
                uiTextShape,
                new UIText.Model
                {
                    isPointerBlocker = true,
                    hAlign = "left",
                    vAlign = "bottom",
                    onClick = "UUIDFakeEventId",
                    textModel = new TextShape.Model
                    {
                        value = "hello world",
                        color = Color.green,
                        opacity = 0.5f,
                        fontSize = 35f,
                        paddingTop = 10f,
                        paddingRight = 30f,
                        paddingBottom = 20f,
                        paddingLeft = 15,
                        lineSpacing = 0.1f,
                        lineCount = 3,
                        shadowOffsetX = 0.1f,
                        shadowOffsetY = 0.1f,
                        shadowColor = Color.yellow,
                        textWrapping = true,
                    }
                });

            //------------------------------------------------------------------------
            // Test click events
            bool eventResult = false;

            yield return TestHelpers.TestUIClickEventPropagation(
                scene.sceneData.id,
                uiTextShape.model,
                uiTextShape.referencesContainer.childHookRectTransform,
                (bool res) =>
                {
                    // Check image object clicking triggers the correct event
                    eventResult = res;
                });

            Assert.IsTrue(eventResult);
        }

        [UnityTest]
        public IEnumerator AddedCorrectlyOnInvisibleParent()
        {
            yield return TestHelpers.TestUIElementAddedCorrectlyOnInvisibleParent<UIText, UIText.Model>(scene, CLASS_ID.UI_TEXT_SHAPE);
        }
    }
}
