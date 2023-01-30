using NSubstitute;
using NUnit.Framework;
using System;

namespace DCL.ConfirmationPopup
{
    public class ConfirmationPopupHUDControllerShould
    {
        private DataStore_Notifications dataStore;
        private IConfirmationPopupHUDView view;
        private ConfirmationPopupHUDController controller;

        [SetUp]
        public void SetUp()
        {
            dataStore = new DataStore_Notifications();

            view = Substitute.For<IConfirmationPopupHUDView>();

            controller = new ConfirmationPopupHUDController(view,
                dataStore);
            view.ClearReceivedCalls();
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void ShowWhenDataHasValidValue()
        {
            dataStore.GenericConfirmation.Set(new GenericConfirmationNotificationData("title", "description",
                "cancel", "confirm", null, null), true);

            view.Received(1).SetModel(Arg.Is<ConfirmationPopupHUDViewModel>(c => c.Title == "title"
                && c.Body == "description" && c.CancelButton == "cancel" && c.ConfirmButton == "confirm"));
            view.Received(1).Show();
        }

        [Test]
        public void HideWhenDataHasInvalidValue()
        {
            dataStore.GenericConfirmation.Set(null, true);

            view.Received(1).Hide();
        }

        [Test]
        public void ExecuteConfirmAction()
        {
            var called = false;

            dataStore.GenericConfirmation.Set(new GenericConfirmationNotificationData("tt", "bb",
                "c", "cc", null, () => called = true), true);

            view.OnConfirm += Raise.Event<Action>();

            Assert.IsTrue(called);
        }

        [Test]
        public void ExecuteCancelAction()
        {
            var called = false;

            dataStore.GenericConfirmation.Set(new GenericConfirmationNotificationData("tt", "bb",
                "c", "cc", () => called = true, null), true);

            view.OnCancel += Raise.Event<Action>();

            Assert.IsTrue(called);
        }
    }
}
