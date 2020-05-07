using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

namespace Tests
{
    public class FontTests : TestsBase
    {
        const string TEST_BUILTIN_FONT_NAME = "builtin:SF-UI-Text-Regular SDF";

        [UnityTest]
        public IEnumerator BuiltInFontCreateAndLoadTest()
        {
            DCLFont font =
                TestHelpers.SharedComponentCreate<DCLFont, DCLFont.Model>(scene, CLASS_ID.FONT, new DCLFont.Model() { src = TEST_BUILTIN_FONT_NAME });
            yield return font.routine;

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            TextShape textShape =
                TestHelpers.EntityComponentCreate<TextShape, TextShape.Model>(scene, entity, new TextShape.Model() { font = font.id });
            yield return textShape.routine;

            Assert.IsTrue(font.loaded, "Built-in font didn't load");
            Assert.IsFalse(font.error, "Built-in font has error");

            TextMeshPro tmpro = textShape.GetComponentInChildren<TextMeshPro>();
            Assert.IsTrue(font.fontAsset == tmpro.font, "Built-in font didn't apply correctly");
        }

        [UnityTest]
        public IEnumerator BuiltInFontHandleErrorProperly()
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            TextShape textShape =
                TestHelpers.EntityComponentCreate<TextShape, TextShape.Model>(scene, entity, new TextShape.Model());
            yield return textShape.routine;

            TMP_FontAsset defaultFont = textShape.GetComponentInChildren<TextMeshPro>().font;

            DCLFont font =
                TestHelpers.SharedComponentCreate<DCLFont, DCLFont.Model>(scene, CLASS_ID.FONT, new DCLFont.Model() { src = "no-valid-font" });
            yield return font.routine;

            scene.EntityComponentUpdate(entity, CLASS_ID_COMPONENT.TEXT_SHAPE,
                            JsonUtility.ToJson(new TextShape.Model { font = font.id }));
            yield return textShape.routine;

            Assert.IsTrue(font.error, "Built-in font error has not araise properly");
            Assert.IsTrue(textShape.GetComponentInChildren<TextMeshPro>().font == defaultFont, "Built-in font didn't apply correctly");
        }

        [UnityTest]
        public IEnumerator BuiltInFontAttachCorrectlyOnTextComponentUpdate()
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            TextShape textShape =
                TestHelpers.EntityComponentCreate<TextShape, TextShape.Model>(scene, entity, new TextShape.Model());
            yield return textShape.routine;

            DCLFont font =
                TestHelpers.SharedComponentCreate<DCLFont, DCLFont.Model>(scene, CLASS_ID.FONT, new DCLFont.Model() { src = TEST_BUILTIN_FONT_NAME });
            yield return font.routine;

            scene.EntityComponentUpdate(entity, CLASS_ID_COMPONENT.TEXT_SHAPE,
                            JsonUtility.ToJson(new TextShape.Model { font = font.id }));
            yield return textShape.routine;

            Assert.IsTrue(font.loaded, "Built-in font didn't load");
            Assert.IsFalse(font.error, "Built-in font has error");

            TextMeshPro tmpro = textShape.GetComponentInChildren<TextMeshPro>();
            Assert.IsTrue(font.fontAsset == tmpro.font, "Built-in font didn't apply correctly");
        }
    }
}