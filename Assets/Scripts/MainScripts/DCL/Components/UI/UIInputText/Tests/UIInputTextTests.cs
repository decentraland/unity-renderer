using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UIInputTextTests : TestsBase
    {
        UIScreenSpace ssshape;
        UIInputText textInput;
        Camera mockCamera;

        public IEnumerator InputTextCreate()
        {
            yield return InitScene(spawnCharController: false);

            ssshape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(
                scene,
                DCL.Models.CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return ssshape.routine;

            if (mockCamera == null)
            {
                GameObject go = new GameObject("Mock camera");
                mockCamera = go.AddComponent<Camera>();
                mockCamera.clearFlags = CameraClearFlags.Color;
                mockCamera.backgroundColor = Color.black;
            }

            textInput = TestHelpers.SharedComponentCreate<UIInputText, UIInputText.Model>(
                scene,
                DCL.Models.CLASS_ID.UI_INPUT_TEXT_SHAPE,
                new UIInputText.Model()
                {
                    textModel = new DCL.Components.TextShape.Model()
                    {
                        color = Color.white,
                        opacity = 1,
                    },

                    placeholder = "Chat here!",
                    placeholderColor = Color.grey,
                    focusedBackground = Color.black,
                    parentComponent = ssshape.id,
                    positionX = new UIValue(0.5f, UIValue.Unit.PERCENT),
                    positionY = new UIValue(0.5f, UIValue.Unit.PERCENT),
                    height = new UIValue(100),
                    width = new UIValue(100),
                });

            yield return textInput.routine;
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPropertiesAreAppliedCorrectly()
        {
            yield return InputTextCreate();

            Assert.AreEqual(Color.white, textInput.model.textModel.color);
            Assert.AreEqual(1, textInput.model.textModel.opacity);
            Assert.AreEqual("Chat here!", textInput.inputField.text);
            Assert.IsTrue(textInput.referencesContainer != null, "Ref container is null?!");
            Assert.AreEqual(textInput.referencesContainer.transform.parent, ssshape.childHookRectTransform);
            Assert.AreEqual(textInput.model.focusedBackground.r, textInput.referencesContainer.bgImage.color.r);
            Assert.AreEqual(textInput.model.focusedBackground.g, textInput.referencesContainer.bgImage.color.g);
            Assert.AreEqual(textInput.model.focusedBackground.b, textInput.referencesContainer.bgImage.color.b);
            Assert.AreEqual(textInput.model.textModel.opacity, textInput.referencesContainer.bgImage.color.a);

            ssshape.Dispose();
            textInput.Dispose();
            Object.DestroyImmediate(mockCamera.gameObject);
        }

        [UnityTest]
        public IEnumerator TestOnFocus()
        {
            yield return InputTextCreate();

            textInput.OnFocus("");

            Assert.IsTrue(textInput.inputField.caretColor == Color.white);
            Assert.IsTrue(textInput.inputField.text == "");

            textInput.OnBlur("");

            Assert.IsTrue(textInput.inputField.text == textInput.model.placeholder);
            Assert.IsTrue(textInput.inputField.caretColor == Color.clear);

            ssshape.Dispose();
            textInput.Dispose();
            Object.DestroyImmediate(mockCamera.gameObject);

        }


        [UnityTest]
        public IEnumerator TestOnSubmit()
        {
            yield return InputTextCreate();

            textInput.inputField.text = "hello world";
            textInput.OnSubmit("");

            Assert.IsTrue(textInput.inputField.text == "");
            Assert.IsTrue(textInput.inputField.caretColor == Color.white);

            ssshape.Dispose();
            textInput.Dispose();
            Object.DestroyImmediate(mockCamera.gameObject);
        }
    }
}
