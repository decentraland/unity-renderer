using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
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
        private WrappedComponentPool<IWrappedComponent<PBUiInputResult>> pool;

        [SetUp]
        public void CreateHandler()
        {
            pool = new WrappedComponentPool<IWrappedComponent<PBUiInputResult>>(0, () => new ProtobufWrappedComponent<PBUiInputResult>(new PBUiInputResult()));
            handler = new UIInputHandler(
                internalUiContainer,
                RESULT_COMPONENT_ID,
                uiInputResultsComponent,
                AssetPromiseKeeper_Font.i,
                COMPONENT_ID,
                pool
            );
        }

        [TearDown]
        public void TearDown()
        {
            handler.OnComponentRemoved(scene, entity);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ConformToSchema(bool useTextValue)
        {
            UpdateComponentModel(useTextValue);

            Assert.AreEqual(new StyleColor(new Color(0.6f, 0.6f, 0.6f, 0.6f)), handler.uiElement.style.color);
            Assert.AreEqual(false, handler.uiElement.isReadOnly);
            Assert.AreEqual(new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft) , handler.uiElement.style.unityTextAlign);
            Assert.AreEqual(new StyleLength(new Length(14, LengthUnit.Pixel)), handler.uiElement.style.fontSize);

            if(useTextValue)
                Assert.AreEqual("TEXT-VALUE", handler.uiElement.text);
            else
                Assert.AreEqual("PLACEHOLDER", handler.uiElement.text);
        }

        [Test]
        public void EmitOnChangeInputResult()
        {
            const string TEST_VALUE = "TEST_TEXT";

            UpdateComponentModel();

            Assert.IsFalse(ContainsInputResult(TEST_VALUE));

            handler.uiElement.value = TEST_VALUE;

            Assert.IsTrue(ContainsInputResult(TEST_VALUE));
        }

        [Test]
        public void BlockPointerByDefault()
        {
            UpdateComponentModel(false);

            Assert.AreEqual(PickingMode.Position, handler.uiElement.pickingMode);
        }

        private void UpdateComponentModel(bool useTextValueProperty = false)
        {
            handler.OnComponentCreated(scene, entity);

            var model = new PBUiInput
            {
                Color = new Color4 { R = 0.1f, G = 0.5f, B = 0.3f, A = 1 },
                PlaceholderColor = new Color4 { R = 0.6f, G = 0.6f, B = 0.6f, A = 0.6f },
                Placeholder = "PLACEHOLDER",
                Disabled = false,
                FontSize = 14,
                TextAlign = TextAlignMode.TamMiddleLeft
            };

            if (useTextValueProperty)
                model.Value = "TEXT-VALUE";

            handler.OnComponentModelUpdated(scene, entity, model);
        }

        private bool ContainsInputResult(string targetTextValue, bool targetSubmitValue = false)
        {
            foreach (var inputResult in uiInputResults.Results)
            {
                if (inputResult.Message.WrappedComponentBase is IWrappedComponent<PBUiInputResult> comp
                    && inputResult.ComponentId == RESULT_COMPONENT_ID
                    && comp.Model.Value == targetTextValue
                    && comp.Model.IsSubmit == targetSubmitValue) return true;
            }

            return false;
        }
    }
}

