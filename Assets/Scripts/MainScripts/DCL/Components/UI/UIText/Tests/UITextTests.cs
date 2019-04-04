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
    public class UITextTests : TestsBase
    {
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
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UITextShape
            UIText uiTextShape = TestHelpers.SharedComponentCreate<UIText, UIText.Model>(scene, CLASS_ID.UI_TEXT_SHAPE,
            new UIText.Model { });

            yield return uiTextShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiTextShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsFalse(uiTextShape.referencesContainer.text.raycastTarget);
            Assert.AreEqual(100f, uiTextShape.childHookRectTransform.rect.width);
            Assert.AreEqual(100f, uiTextShape.childHookRectTransform.rect.height);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enabled);
            Assert.AreEqual(Color.white, uiTextShape.referencesContainer.text.color);
            Assert.AreEqual(100f, uiTextShape.referencesContainer.text.fontSize);
            Assert.AreEqual("", uiTextShape.referencesContainer.text.text);
            Assert.AreEqual(1, uiTextShape.referencesContainer.text.maxVisibleLines);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.lineSpacing);
            Assert.IsFalse(uiTextShape.referencesContainer.text.enableAutoSizing);
            Assert.IsFalse(uiTextShape.referencesContainer.text.enableWordWrapping);
            Assert.IsFalse(uiTextShape.referencesContainer.text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"));

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiTextShape.childHookRectTransform.rect);
            Assert.AreEqual(alignedPosition.ToString(), uiTextShape.childHookRectTransform.anchoredPosition.ToString());

            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.x);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.y);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.z);
            Assert.AreEqual(0, uiTextShape.referencesContainer.text.margin.w);

            // Update UITextShape
            uiTextShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiTextShape.id,
                json = JsonUtility.ToJson(new UIText.Model
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
            })) as UIText;

            yield return uiTextShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiTextShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(uiTextShape.referencesContainer.text.raycastTarget);
            Assert.AreEqual(100f, uiTextShape.childHookRectTransform.rect.width);
            Assert.AreEqual(100f, uiTextShape.childHookRectTransform.rect.height);
            Assert.AreEqual("hello world", uiTextShape.referencesContainer.text.text);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enabled);
            Assert.AreEqual(new Color(0f, 1f, 0f, 0.5f), uiTextShape.referencesContainer.text.color);
            Assert.AreEqual(35f, uiTextShape.referencesContainer.text.fontSize);
            Assert.AreEqual(3, uiTextShape.referencesContainer.text.maxVisibleLines);
            Assert.AreEqual(0.1f, uiTextShape.referencesContainer.text.lineSpacing);
            Assert.IsTrue(uiTextShape.referencesContainer.text.enableWordWrapping);
            Assert.IsTrue(uiTextShape.referencesContainer.text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"));
            Assert.AreEqual(Color.yellow, uiTextShape.referencesContainer.text.fontMaterial.GetColor("_UnderlayColor"));

            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiTextShape.childHookRectTransform.rect, "bottom", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiTextShape.childHookRectTransform.anchoredPosition.ToString());

            Assert.AreEqual(15f, uiTextShape.referencesContainer.text.margin.x);
            Assert.AreEqual(10f, uiTextShape.referencesContainer.text.margin.y);
            Assert.AreEqual(30f, uiTextShape.referencesContainer.text.margin.z);
            Assert.AreEqual(20f, uiTextShape.referencesContainer.text.margin.w);
        }
    }
}
