using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements.Tests;
using Decentraland.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIInput.Tests
{
    public class UIInputHandlerShould : UIComponentsShouldBase
    {
        private const int COMPONENT_ID = 1001;
        private const int RESULT_COMPONENT_ID = 1001;

        private UIInputHandler handler;

        [SetUp]
        public void CreateHandler()
        {
            handler = new UIInputHandler(internalUiContainer, RESULT_COMPONENT_ID, uiInputResultsComponent, AssetPromiseKeeper_Font.i, COMPONENT_ID);
            UpdateComponentModel();
        }

        [Test]
        public void ConformToSchema()
        {
            Assert.AreEqual(new StyleColor(new Color(0.6f, 0.6f, 0.6f, 0.6f)), handler.uiElement.style.color);
            Assert.AreEqual("PLACEHOLDER", handler.uiElement.text);
            Assert.AreEqual(false, handler.uiElement.isReadOnly);
            Assert.AreEqual(new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft) , handler.uiElement.style.unityTextAlign);
            Assert.AreEqual(new StyleLength(new Length(14, LengthUnit.Pixel)), handler.uiElement.style.fontSize);
        }

        [Test]
        public void EmitResult()
        {
            const string TEST_VALUE = "TEST_TEXT";

            handler.uiElement.value = TEST_VALUE;

            Assert.Contains(
                new InternalUIInputResults.Result(new PBUiInputResult {Value = TEST_VALUE}, RESULT_COMPONENT_ID),
                uiInputResults.Results);
        }

        private void UpdateComponentModel()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBUiInput
            {
                Color = new Color4 { R = 0.1f, G = 0.5f, B = 0.3f, A = 1 },
                PlaceholderColor = new Color4 { R = 0.6f, G = 0.6f, B = 0.6f, A = 0.6f },
                Placeholder = "PLACEHOLDER",
                Disabled = false,
                FontSize = 14,
                TextAlign = TextAlignMode.TamMiddleLeft
            });
        }
    }
}
