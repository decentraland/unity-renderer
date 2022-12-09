using NSubstitute;
using NUnit.Framework;
using System;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDComponentViewShould
    {
        private ReceivedFriendRequestHUDComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = ReceivedFriendRequestHUDComponentView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ClickOnCloseButtonsCorrectly(int closeButtonIndex)
        {
            // Arrange
            var closeEventTriggered = false;
            view.OnClose += () => closeEventTriggered = true;

            // Act
            view.closeButtons[closeButtonIndex].onClick.Invoke();

            // Assert
            Assert.IsTrue(closeEventTriggered);
        }

        [Test]
        public void ClickOnOpenPassportButtonCorrectly()
        {
            // Arrange
            bool openProfileTriggered = false;
            view.OnOpenProfile += () => openProfileTriggered = true;

            // Act
            view.openPassportButton.onClick.Invoke();

            // Assert
            Assert.IsTrue(openProfileTriggered);
        }

        [Test]
        public void ClickOnRejectButtonCorrectly()
        {
            // Arrange
            var rejectFriendRequestTriggered = false;
            view.OnRejectFriendRequest += () => rejectFriendRequestTriggered = true;

            // Act
            view.rejectButton.onClick.Invoke();

            // Assert
            Assert.IsTrue(rejectFriendRequestTriggered);
        }

        [Test]
        public void ClickOnConfirmButtonCorrectly()
        {
            // Arrange
            var confirmFriendRequestTriggered = false;
            view.OnConfirmFriendRequest += () => confirmFriendRequestTriggered = true;

            // Act
            view.confirmButton.onClick.Invoke();

            // Assert
            Assert.IsTrue(confirmFriendRequestTriggered);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClickOnRetryButtonCorrectly(bool lastTryWasConfirm)
        {
            // Arrange
            if (lastTryWasConfirm)
                view.confirmButton.onClick.Invoke();
            else
                view.rejectButton.onClick.Invoke();

            bool confirmFriendRequestTriggered = false;
            view.OnConfirmFriendRequest += () => confirmFriendRequestTriggered = true;
            bool rejectFriendRequestTriggered = false;
            view.OnRejectFriendRequest += () => rejectFriendRequestTriggered = true;

            // Act
            view.retryButton.onClick.Invoke();

            // Assert
            if (lastTryWasConfirm)
                Assert.IsTrue(confirmFriendRequestTriggered);
            else
                Assert.IsTrue(rejectFriendRequestTriggered);
        }

        [Test]
        public void SetBodyMessageCorrectly()
        {
            // Arrange
            var testMessage = "Test message";

            // Act
            view.SetBodyMessage(testMessage);

            // Assert
            Assert.AreEqual(testMessage, view.bodyMessageInput.text);

            if (!string.IsNullOrEmpty(testMessage))
                Assert.IsTrue(view.bodyMessageContainer.activeSelf);
            else
                Assert.IsFalse(view.bodyMessageContainer.activeSelf);
        }

        [Test]
        public void SetTimestampCorrectly()
        {
            // Arrange
            var testTimestamp = DateTime.Now;

            // Act
            view.SetTimestamp(testTimestamp);

            // Assert
            Assert.AreEqual(testTimestamp.Date.ToString("MMM dd").ToUpper(), view.dateLabel.text);
        }

        [Test]
        public void SetRecipientNameCorrectly()
        {
            // Arrange
            string testName = "Test name";

            // Act
            view.SetRecipientName(testName);

            // Assert
            Assert.AreEqual(testName, view.nameLabel.text);
            Assert.IsTrue(view.rejectSuccessLabel.text.Contains(testName));
            Assert.IsTrue(view.confirmSuccessLabel.text.Contains(testName));
        }

        [Test]
        [TestCase(ReceivedFriendRequestHUDModel.LayoutState.Default)]
        [TestCase(ReceivedFriendRequestHUDModel.LayoutState.Pending)]
        [TestCase(ReceivedFriendRequestHUDModel.LayoutState.Failed)]
        [TestCase(ReceivedFriendRequestHUDModel.LayoutState.RejectSuccess)]
        [TestCase(ReceivedFriendRequestHUDModel.LayoutState.ConfirmSuccess)]
        public void SetStateCorrectly(ReceivedFriendRequestHUDModel.LayoutState state)
        {
            // Arrange
            

            // Act
            view.SetState(state);

            // Assert
            switch (state)
            {
                case ReceivedFriendRequestHUDModel.LayoutState.Default:
                    Assert.IsTrue(view.defaultContainer.activeSelf);
                    Assert.IsFalse(view.failedContainer.activeSelf);
                    Assert.IsFalse(view.rejectSuccessContainer.activeSelf);
                    Assert.IsFalse(view.confirmSuccessContainer.activeSelf);
                    foreach (var button in view.buttonsToDisableOnPendingState)
                        Assert.IsTrue(button.interactable);
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.Pending:
                    Assert.IsTrue(view.defaultContainer.activeSelf);
                    Assert.IsFalse(view.failedContainer.activeSelf);
                    Assert.IsFalse(view.rejectSuccessContainer.activeSelf);
                    Assert.IsFalse(view.confirmSuccessContainer.activeSelf);
                    foreach (var button in view.buttonsToDisableOnPendingState)
                        Assert.IsFalse(button.interactable);
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.Failed:
                    Assert.IsFalse(view.defaultContainer.activeSelf);
                    Assert.IsTrue(view.failedContainer.activeSelf);
                    Assert.IsFalse(view.rejectSuccessContainer.activeSelf);
                    Assert.IsFalse(view.confirmSuccessContainer.activeSelf);
                    foreach (var button in view.buttonsToDisableOnPendingState)
                        Assert.IsTrue(button.interactable);
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.ConfirmSuccess:
                    Assert.IsFalse(view.defaultContainer.activeSelf);
                    Assert.IsFalse(view.failedContainer.activeSelf);
                    Assert.IsFalse(view.rejectSuccessContainer.activeSelf);
                    Assert.IsTrue(view.confirmSuccessContainer.activeSelf);
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.RejectSuccess:
                    Assert.IsFalse(view.defaultContainer.activeSelf);
                    Assert.IsFalse(view.failedContainer.activeSelf);
                    Assert.IsTrue(view.rejectSuccessContainer.activeSelf);
                    Assert.IsFalse(view.confirmSuccessContainer.activeSelf);
                    break;
            }
        }

        [Test]
        public void ShowCorrectly()
        {
            // Act
            view.Show();

            // Assert
            Assert.IsTrue(view.defaultContainer.activeSelf);
            Assert.IsFalse(view.failedContainer.activeSelf);
            Assert.IsFalse(view.rejectSuccessContainer.activeSelf);
            Assert.IsFalse(view.confirmSuccessContainer.activeSelf);
            foreach (var button in view.buttonsToDisableOnPendingState)
                Assert.IsTrue(button.interactable);
            Assert.IsTrue(view.gameObject.activeSelf);
        }

        [Test]
        public void CloseCorrectly()
        {
            // Act
            view.Close();

            // Assert
            Assert.IsFalse(view.gameObject.activeSelf);
        }
    }
}
