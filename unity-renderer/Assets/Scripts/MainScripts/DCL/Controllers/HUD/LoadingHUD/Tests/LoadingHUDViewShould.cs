using LoadingHUD;
using NUnit.Framework;
using UnityEngine;

namespace Tests.LoadingHUD
{
    public class LoadingHUDViewShould
    {
        private LoadingHUDView hudView;

        [SetUp]
        public void SetUp() 
        { 
            hudView = Object.Instantiate(Resources.Load<GameObject>("LoadingHUD")).GetComponent<LoadingHUDView>(); 
            hudView.Initialize(); 
        }

        [Test]
        public void AwakeProperly()
        {
            Assert.AreEqual("", hudView.text.text);
            Assert.AreEqual(0, hudView.loadingBar.transform.localScale.x);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityProperly(bool visible)
        {
            hudView.SetVisible(visible, true);
            Assert.AreEqual(visible, hudView.showHideAnimator.isVisible);
        }

        [Test]
        public void SetMessageProperly()
        {
            hudView.SetMessage("the_new_message");
            Assert.AreEqual("the_new_message", hudView.text.text);
        }

        [Test]
        public void SetPercentageProperly()
        {
            hudView.SetPercentage(0.7f);
            Assert.AreEqual(0.7f, hudView.loadingBar.transform.localScale.x);
        }

        [Test]
        public void SetTipsProperly_True()
        {
            hudView.SetTips(true);
            Assert.IsTrue(hudView.tipsContainer.activeSelf);
            Assert.IsFalse(hudView.noTipsContainer.activeSelf);
        }

        [Test]
        public void SetTipsProperly_False()
        {
            hudView.SetTips(false);
            Assert.IsFalse(hudView.tipsContainer.activeSelf);
            Assert.IsTrue(hudView.noTipsContainer.activeSelf);
        }
    }
}