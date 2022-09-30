using System;
using DCL.ECSComponents;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests
{
    public class ECSInteractionHoverMonoBehaviorShould
    {
        private ECSInteractionHoverMonoBehavior interactionHoverCanvas;

        [SetUp]
        public void SetUp()
        {
            var canvas = Resources.Load<GameObject>("ECSInteractionHoverCanvas");
            var go = Object.Instantiate(canvas);
            interactionHoverCanvas = go.GetComponent<ECSInteractionHoverMonoBehavior>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(interactionHoverCanvas.gameObject);
        }

        [Test]
        public void SetInputTooltip()
        {
            interactionHoverCanvas.SetTooltipInput(0, ActionButton.Pointer);
            Assert.AreEqual(interactionHoverCanvas._icons[0], interactionHoverCanvas._tooltips[0]._icon.sprite);
            Assert.IsTrue(interactionHoverCanvas._tooltips[0]._iconGameObject.activeSelf);
            Assert.IsFalse(interactionHoverCanvas._tooltips[0]._inputTextGameObject.activeSelf);

            interactionHoverCanvas.SetTooltipInput(0, ActionButton.Any);
            Assert.AreEqual(interactionHoverCanvas._icons[1], interactionHoverCanvas._tooltips[0]._icon.sprite);
            Assert.IsTrue(interactionHoverCanvas._tooltips[0]._iconGameObject.activeSelf);
            Assert.IsFalse(interactionHoverCanvas._tooltips[0]._inputTextGameObject.activeSelf);

            for (int i = 0; i < Enum.GetNames(typeof(ActionButton)).Length; i++)
            {
                ActionButton button = (ActionButton)i;
                if (button == ActionButton.Any || button == ActionButton.Pointer)
                    continue;

                interactionHoverCanvas.SetTooltipInput(0, button);
                Assert.AreEqual(interactionHoverCanvas.inputText[i], interactionHoverCanvas._tooltips[0]._inputText.text);
                Assert.IsTrue(interactionHoverCanvas._tooltips[0]._inputTextGameObject.activeSelf);
                Assert.IsFalse(interactionHoverCanvas._tooltips[0]._iconGameObject.activeSelf);
            }
        }

        [Test]
        public void SetTooltipText()
        {
            interactionHoverCanvas.SetTooltipText(0, "temptation");
            Assert.AreEqual("temptation", interactionHoverCanvas._tooltips[0]._text.text);

            // check for tooltip text lenght cap
            interactionHoverCanvas.SetTooltipText(0, "123456789012345678901234567890");
            Assert.AreEqual("1234567890123456789012345", interactionHoverCanvas._tooltips[0]._text.text);
        }

        [Test]
        public void SetTooltipActive()
        {
            interactionHoverCanvas.SetTooltipActive(0, true);
            Assert.IsTrue(interactionHoverCanvas._tooltips[0].gameObject.activeSelf);

            interactionHoverCanvas.SetTooltipActive(0, false);
            Assert.IsFalse(interactionHoverCanvas._tooltips[0].gameObject.activeSelf);
        }

        [Test]
        public void ShowAndHide()
        {
            Assert.IsTrue(interactionHoverCanvas._showHideAnimator.hideOnEnable);

            interactionHoverCanvas.Show();
            Assert.IsTrue(interactionHoverCanvas._showHideAnimator.isVisible);

            interactionHoverCanvas.Hide();
            Assert.IsFalse(interactionHoverCanvas._showHideAnimator.isVisible);
        }
    }
}