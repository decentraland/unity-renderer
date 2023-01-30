using NUnit.Framework;

namespace DCL.ConfirmationPopup
{
    public class ConfirmationPopupHUDComponentViewShould
    {
        private ConfirmationPopupHUDComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = ConfirmationPopupHUDComponentView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [TestCase("title", "body", "confirm", "cancel")]
        [TestCase("very long title whats going to heheh hoho meme woooooooooooo",
            "very long description because there are many things i want to say here but im not sure if there is enough space",
            "confirmation maybe or maybe not im not sure", "please dont cancel me i want you to accept this action")]
        public void SetModel(string title, string body, string confirm, string cancel)
        {
            view.SetModel(new ConfirmationPopupHUDViewModel(title, body, cancel, confirm));

            Assert.AreEqual(title, view.titleLabel.text);
            Assert.AreEqual(body, view.bodyLabel.text);
            Assert.AreEqual(confirm, view.confirmLabel.text);
            Assert.AreEqual(cancel, view.cancelLabel.text);
        }

        [Test]
        public void TriggerConfirmationEventWhenButtonClicks()
        {
            var called = false;
            view.OnConfirm += () => called = true;

            view.confirmButton.onClick.Invoke();

            Assert.IsTrue(called);
        }

        [Test]
        public void TriggerCancelEventWhenButtonClicks()
        {
            var called = false;
            view.OnCancel += () => called = true;

            view.cancelButton.onClick.Invoke();

            Assert.IsTrue(called);
        }
    }
}
