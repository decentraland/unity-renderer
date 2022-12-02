using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
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

        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;
        private UIComponentsPlugin uiComponentsPlugin;

        protected override List<GameObject> SetUp_LegacySystems()
        {
            List<GameObject> result = new List<GameObject>();
            result.Add(MainSceneFactory.CreateEventSystem());
            return result;
        }


        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            coreComponentsPlugin = new CoreComponentsPlugin();
            uiComponentsPlugin = new UIComponentsPlugin();
            scene = TestUtils.CreateTestScene();
        }

        protected override IEnumerator TearDown()
        {
            coreComponentsPlugin.Dispose();
            uiComponentsPlugin.Dispose();
            yield return base.TearDown();
        }

        public IEnumerator InputTextCreate()
        {
            ssshape = TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(
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

            textInput = TestUtils.SharedComponentCreate<UIInputText, UIInputText.Model>(
                scene,
                DCL.Models.CLASS_ID.UI_INPUT_TEXT_SHAPE,
                new UIInputText.Model()
                {
                    color = Color.white,
                    opacity = 1,
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
            sceneEvent.sceneNumber = scene.sceneData.sceneNumber;
            sceneEvent.payload = onClickEvent;
            sceneEvent.eventType = "uuidEvent";
            string eventJSON = JsonUtility.ToJson(sceneEvent);

            bool eventTriggered = false;


            yield return TestUtils.WaitForMessageFromEngine(targetEventType, eventJSON,
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

            Assert.AreEqual(Color.white, textInput.model.color);
            Assert.AreEqual(1, textInput.model.opacity);
            Assert.AreEqual("Chat here!", textInput.inputField.text);
            Assert.IsTrue(textInput.referencesContainer != null, "Ref container is null?!");
            Assert.AreEqual(textInput.referencesContainer.transform.parent, ssshape.childHookRectTransform);
            Assert.AreEqual(textInput.model.focusedBackground.r, textInput.referencesContainer.bgImage.color.r);
            Assert.AreEqual(textInput.model.focusedBackground.g, textInput.referencesContainer.bgImage.color.g);
            Assert.AreEqual(textInput.model.focusedBackground.b, textInput.referencesContainer.bgImage.color.b);
            Assert.AreEqual(textInput.model.opacity, textInput.referencesContainer.bgImage.color.a);

            ssshape.Dispose();
            textInput.Dispose();
            Object.DestroyImmediate(mockCamera.gameObject);
        }

        [UnityTest]
        public IEnumerator AddedCorrectlyOnInvisibleParent() { yield return TestUtils.TestUIElementAddedCorrectlyOnInvisibleParent<UIInputText, UIInputText.Model>(scene, CLASS_ID.UI_INPUT_TEXT_SHAPE); }

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

            const string testValue = "hello world";
            const string uuid1 = "submit1";
            const string uuid2 = "submit2";

            textInput.model.onTextChanged = uuid1;
            textInput.model.onTextSubmit = uuid2;

            // NOTE: test ReportOnTextInputChangedTextEvent
            var submitEvent = new WebInterface.SceneEvent<WebInterface.UUIDEvent<WebInterface.OnTextInputChangeTextEventPayload>>();
            submitEvent.sceneNumber = scene.sceneData.sceneNumber;
            submitEvent.payload = new WebInterface.UUIDEvent<WebInterface.OnTextInputChangeTextEventPayload>()
            {
                payload = new WebInterface.OnTextInputChangeTextEventPayload()
                {
                    value = new WebInterface.OnTextInputChangeTextEventPayload.Payload()
                    {
                        value = testValue,
                        isSubmit = true
                    }
                },
                uuid = uuid1
            };
            submitEvent.eventType = "uuidEvent";

            bool eventTriggered = false;
            yield return TestUtils.WaitForMessageFromEngine("SceneEvent", JsonUtility.ToJson(submitEvent),
                () =>
                {
                    textInput.inputField.text = testValue;
                    textInput.inputField.onSubmit.Invoke(testValue);
                },
                () => { eventTriggered = true; });

            yield return null;

            Assert.IsTrue(eventTriggered);

            // NOTE: test ReportOnTextSubmitEvent
            var submitEvent2 = new WebInterface.SceneEvent<WebInterface.UUIDEvent<WebInterface.OnTextSubmitEventPayload>>();
            submitEvent2.sceneNumber = scene.sceneData.sceneNumber;
            submitEvent2.payload = new WebInterface.UUIDEvent<WebInterface.OnTextSubmitEventPayload>()
            {
                payload = new WebInterface.OnTextSubmitEventPayload()
                {
                    text = testValue
                },
                uuid = uuid2
            };
            submitEvent2.eventType = "uuidEvent";

            eventTriggered = false;
            yield return TestUtils.WaitForMessageFromEngine("SceneEvent", JsonUtility.ToJson(submitEvent2),
                () =>
                {
                    textInput.inputField.text = testValue;
                    textInput.inputField.onSubmit.Invoke(testValue);
                },
                () => { eventTriggered = true; });

            yield return null;

            Assert.IsTrue(eventTriggered);

            ssshape.Dispose();
            textInput.Dispose();

            if (mockCamera != null)
            {
                Object.DestroyImmediate(mockCamera.gameObject);
            }
        }
    }
}