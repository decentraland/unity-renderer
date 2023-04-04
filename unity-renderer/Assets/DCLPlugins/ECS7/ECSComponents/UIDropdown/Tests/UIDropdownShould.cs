using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements.Tests;
using Decentraland.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIDropdown.Tests
{
    [Category("Flaky")]
    public class UIDropdownShould : UIComponentsShouldBase
    {
        private const int COMPONENT_ID = 1001;
        private const int RESULT_COMPONENT_ID = 1001;

        private UIDropdownHandler handler;

        [SetUp]
        public void CreateHandler()
        {
            handler = new UIDropdownHandler(internalUiContainer, RESULT_COMPONENT_ID, uiInputResultsComponent, AssetPromiseKeeper_Font.i, COMPONENT_ID);
        }

        [Test]
        public void ConformToSchema()
        {
            UpdateComponentModel(true, 2);

            Assert.AreEqual(new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.5f)), handler.uiElement.style.color);
            Assert.AreEqual(new StyleLength(new Length(16)), handler.uiElement.style.fontSize);
            Assert.AreEqual(new StyleEnum<TextAnchor>(TextAnchor.LowerCenter), handler.textField.style.unityTextAlign);
            CollectionAssert.AreEquivalent(new[] { "OPTION1", "OPTION2", "OPTION3", "OPTION4" }, handler.uiElement.choices);
            Assert.AreEqual("OPTION3", handler.uiElement.text);
            Assert.AreEqual(2, handler.uiElement.index);
        }

        [Test]
        public void AllowEmptyValue()
        {
            UpdateComponentModel(true, -1);

            Assert.AreEqual(-1, handler.uiElement.index);
            Assert.AreEqual("EMPTY", handler.uiElement.text);
        }

        [Test]
        public void DisallowEmptyValue()
        {
            UpdateComponentModel(false, -1);

            Assert.AreEqual(0, handler.uiElement.index);
            Assert.AreEqual("OPTION1", handler.uiElement.text);
        }

        private void UpdateComponentModel(bool acceptEmpty, int selectedIndex)
        {
            handler.OnComponentCreated(scene, entity);

            var c = new PBUiDropdown
            {
                AcceptEmpty = acceptEmpty,
                Color = new Color4 { R = 0.5f, G = 0.5f, B = 0.5f, A = 0.5f },
                Disabled = false,
                FontSize = 16,
                TextAlign = TextAlignMode.TamBottomCenter,
                EmptyLabel = "EMPTY",
                Options = { "OPTION1", "OPTION2", "OPTION3", "OPTION4" },
            };

            c.SelectedIndex = selectedIndex;

            handler.OnComponentModelUpdated(scene, entity, c);
        }

        [Test]
        public void EmitResult()
        {
            UpdateComponentModel(true, 1);

            handler.uiElement.index = 2;

            Assert.Contains(
                new InternalUIInputResults.Result(new PBUiDropdownResult {Value = 2}, RESULT_COMPONENT_ID),
                uiInputResults.Results);
        }
    }
}
