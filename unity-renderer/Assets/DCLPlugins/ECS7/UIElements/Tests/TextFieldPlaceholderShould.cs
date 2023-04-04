using DCL.ECSComponents;
using Decentraland.Common;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.UIElements.Tests
{
    public class TextFieldPlaceholderShould
    {
        private const string PLACEHOLDER = "TEST_PLACEHOLDER";
        private const string NORMAL_TEXT = "TEST_TEXT";

        private static readonly Color4 PLACEHOLDER_COLOR = new Color4() { R = 0.5f, G = 0.5f, B = 0.5f, A = 0.87f };
        private static readonly Color4 NORMAL_COLOR = new Color4() { R = 1f, G = 1f, B = 1f, A = 1f };

        private TextField textField;
        private TextFieldPlaceholder placeholder;

        // we need a panel to operate with events
        private UIDocument uiDocument;

        [SetUp]
        public void SetUp()
        {
            uiDocument = Object.Instantiate(Resources.Load<UIDocument>("ScenesUI"));
            uiDocument.rootVisualElement.Insert(0, textField = new TextField());

            placeholder = new TextFieldPlaceholder(textField);
            placeholder.SetPlaceholder(PLACEHOLDER);
            placeholder.SetPlaceholderColor(PLACEHOLDER_COLOR);
            placeholder.SetNormalColor(NORMAL_COLOR);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(uiDocument);
        }

        [Test]
        public void SetPlaceholderWhenInputIsEmpty()
        {
            ConformToPlaceholderSchema();
        }

        [Test]
        public void ClearInputWhenFocusedIn()
        {
            textField.Focus();
            ConformToNormalSchema(string.Empty);
        }

        [Test]
        public void SetPlaceholderWhenFocusedOut()
        {
            textField.value = NORMAL_TEXT;
            textField.Focus();
            textField.value = string.Empty;
            textField.Blur();
            ConformToPlaceholderSchema();
        }

        [Test]
        public void NotUpdatePlaceholderWhenFocusedIn()
        {
            textField.Focus();
            textField.value = NORMAL_TEXT;
            placeholder.SetPlaceholder(PLACEHOLDER);
            ConformToNormalSchema(NORMAL_TEXT);
        }

        [Test]
        public void IgnoreEventsIfReadonly()
        {
            placeholder.SetReadOnly(true);
            textField.Focus();
            ConformToPlaceholderSchema();
        }

        [Test]
        public void RemovePlaceholderWhenFocusedOut()
        {
            textField.value = NORMAL_TEXT;
            textField.Focus();
            textField.Blur();
            ConformToNormalSchema(NORMAL_TEXT);
        }

        private void ConformToPlaceholderSchema()
        {
            Assert.AreEqual(PLACEHOLDER, textField.value);
            Assert.AreEqual(PLACEHOLDER_COLOR.ToUnityColor(), textField.style.color.value);
        }

        private void ConformToNormalSchema(string expectedValue)
        {
            Assert.AreEqual(expectedValue, textField.value);
            Assert.AreEqual(NORMAL_COLOR.ToUnityColor(), textField.style.color.value);
        }
    }
}
