using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class UnpublishPopupShould
    {
        private UnpublishPopupView view;
        private UnpublishPopupController controller;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<UnpublishPopupView>("UnpublishPopup/UnpublishPopupView");
            view = UnityEngine.Object.Instantiate(prefab);
            controller = new UnpublishPopupController(BIWTestUtils.CreateMockedContext(), view);

            // This is needed because BuilderMainPanelController uses the Analytics utils, which in turn use
            // Environment.i.serviceProviders.analytics
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            Environment.Setup(serviceLocator);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.Dispose();
            controller.Dispose();
        }

        [Test]
        public void ShowConfirmationPopupCorrectly()
        {
            controller.Show(Vector2Int.zero,Vector2Int.zero);
            Assert.IsTrue(view.gameObject.activeSelf);

            Assert.IsTrue(view.cancelButton.gameObject.activeSelf);
            Assert.IsTrue(view.unpublishButton.gameObject.activeSelf);
            Assert.IsTrue(view.infoText.gameObject.activeSelf);
            Assert.IsTrue(view.closeButton.gameObject.activeSelf);
            Assert.IsFalse(view.doneButton.gameObject.activeSelf);
            Assert.IsFalse(view.errorText.gameObject.activeSelf);
            Assert.IsFalse(view.loadingBarContainer.gameObject.activeSelf);
        }

        [Test]
        public void ShowProgressCorrectly()
        {
            IUnpublishPopupView iview = view;
            iview.SetProgress("", 0);
            Assert.AreEqual("0%", view.loadingText.text);
            iview.SetProgress("", 0.5f);
            Assert.AreEqual("50%", view.loadingText.text);

            Assert.IsTrue(view.loadingBarContainer.gameObject.activeSelf);
            Assert.IsFalse(view.cancelButton.gameObject.activeSelf);
            Assert.IsFalse(view.unpublishButton.gameObject.activeSelf);
            Assert.IsFalse(view.infoText.gameObject.activeSelf);
            Assert.IsFalse(view.doneButton.gameObject.activeSelf);
            Assert.IsFalse(view.errorText.gameObject.activeSelf);
            Assert.IsFalse(view.closeButton.gameObject.activeSelf);
        }

        [Test]
        public void ShowErrorCorrectly()
        {
            const string error = "Some Error";

            IUnpublishPopupView iview = view;
            iview.SetError("", error);
            Assert.AreEqual(error, view.errorText.text);

            Assert.IsFalse(view.loadingBarContainer.gameObject.activeSelf);
            Assert.IsFalse(view.cancelButton.gameObject.activeSelf);
            Assert.IsFalse(view.unpublishButton.gameObject.activeSelf);
            Assert.IsFalse(view.infoText.gameObject.activeSelf);
            Assert.IsTrue(view.doneButton.gameObject.activeSelf);
            Assert.IsTrue(view.errorText.gameObject.activeSelf);
            Assert.IsTrue(view.closeButton.gameObject.activeSelf);
        }

        [Test]
        public void ShowSuccessCorrectly()
        {
            const string success = "Some Success Message";

            IUnpublishPopupView iview = view;
            iview.SetSuccess("", success);
            Assert.AreEqual(success, view.infoText.text);

            Assert.IsTrue(view.infoText.gameObject.activeSelf);
            Assert.IsTrue(view.doneButton.gameObject.activeSelf);
            Assert.IsFalse(view.loadingBarContainer.gameObject.activeSelf);
            Assert.IsFalse(view.cancelButton.gameObject.activeSelf);
            Assert.IsFalse(view.unpublishButton.gameObject.activeSelf);
            Assert.IsFalse(view.errorText.gameObject.activeSelf);
            Assert.IsTrue(view.closeButton.gameObject.activeSelf);
        }

        [Test]
        public void ControllerSuccessFlowCorrectly()
        {
            controller.Show(Vector2Int.zero,Vector2Int.zero);
            Assert.AreEqual(UnpublishPopupView.State.CONFIRM_UNPUBLISH, view.state);

            view.unpublishButton.onClick.Invoke();
            Assert.AreEqual(UnpublishPopupView.State.UNPUBLISHING, view.state);

            DataStore.i.builderInWorld.unpublishSceneResult.Set(new PublishSceneResultPayload() { ok = true }, true);
            Assert.AreEqual(UnpublishPopupView.State.DONE_UNPUBLISH, view.state);
        }

        [Test]
        public void ControllerErrorFlowCorrectly()
        {
            controller.Show(Vector2Int.zero, Vector2Int.zero);
            Assert.AreEqual(UnpublishPopupView.State.CONFIRM_UNPUBLISH, view.state);

            view.unpublishButton.onClick.Invoke();
            Assert.AreEqual(UnpublishPopupView.State.UNPUBLISHING, view.state);

            DataStore.i.builderInWorld.unpublishSceneResult.Set(new PublishSceneResultPayload() { ok = false }, true);
            Assert.AreEqual(UnpublishPopupView.State.ERROR_UNPUBLISH, view.state);
        }
    }
}