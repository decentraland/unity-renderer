using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Color = UnityEngine.Color;

namespace Tests
{
    public class UIInputTextTests : IntegrationTestSuite_Legacy
    {
        UIScreenSpace ssshape;
        UIInputText textInput;
        Camera mockCamera;


        public IEnumerator InputTextCreate()
        {
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

                    isPointerBlocker = true,
                    placeholder = "Chat here!",
                    placeholderColor = Color.grey,
                    focusedBackground = Color.black,
                    parentComponent = ssshape.id,
                    positionX = new UIValue(0.5f, UIValue.Unit.PERCENT),
                    positionY = new UIValue(0.5f, UIValue.Unit.PERCENT),
                    height = new UIValue(100),
                    width = new UIValue(100),
                    onClick = "UUIDFakeEventId"
                });

            yield return textInput.routine;
            yield return null;

            if (mockCamera != null)
                Object.Destroy(mockCamera.gameObject);
        }

        [UnityTest]
        public IEnumerator TestOnClickEvent()
        {
            yield return InputTextCreate();

            //------------------------------------------------------------------------
            // Test click events
            TMPro.TMP_InputField inputField = textInput.referencesContainer.inputField;

            string targetEventType = "SceneEvent";

            var onClickEvent = new WebInterface.OnClickEvent();
            onClickEvent.uuid = textInput.model.onClick;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnClickEvent>();
            sceneEvent.sceneId = scene.sceneData.id;
            sceneEvent.payload = onClickEvent;
            sceneEvent.eventType = "uuidEvent";
            string eventJSON = JsonUtility.ToJson(sceneEvent);

            bool eventTriggered = false;


            yield return TestHelpers.WaitForMessageFromEngine(targetEventType, eventJSON,
                () =>
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                    inputField.Select();
                },
                () => { eventTriggered = true; });

            yield return null;

            Assert.IsTrue(eventTriggered);
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
        public IEnumerator AddedCorrectlyOnInvisibleParent()
        {
            yield return TestHelpers.TestUIElementAddedCorrectlyOnInvisibleParent<UIInputText, UIInputText.Model>(scene, CLASS_ID.UI_INPUT_TEXT_SHAPE);
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