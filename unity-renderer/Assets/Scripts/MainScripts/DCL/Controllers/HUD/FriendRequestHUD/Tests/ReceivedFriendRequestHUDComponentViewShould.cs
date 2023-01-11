using NUnit.Framework;
using System;
using System.Globalization;

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
        public void SetBodyMessageCorrectly()
        {
            // Arrange
            var testMessage = "Test message";

            // Act
            view.SetBodyMessage(testMessage);

            // Assert
            Assert.IsTrue(view.bodyMessageInput.text.Contains(testMessage));

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
            Assert.AreEqual(testTimestamp.Date.ToString("MMM dd", new CultureInfo("en-US")).ToUpper(), view.dateLabel.text);
        }

        [Test]
        public void SetRecipientNameCorrectly()
        {
            // Arrange
            string testName = "Test name";

            // Act
            view.SetSenderName(testName);

            // Assert
            Assert.AreEqual(testName, view.nameLabel.text);
            Assert.IsTrue(view.rejectSuccessLabel.text.Contains(testName));
            Assert.IsTrue(view.confirmSuccessLabel.text.Contains(testName));
        }

        [Test]
        [TestCase(ReceivedFriendRequestHUDModel.LayoutState.Default)]
        [TestCase(ReceivedFriendRequestHUDModel.LayoutState.Pending)]
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
                    Assert.IsTrue(view.showHideAnimatorForDefaultState.isVisible);
                    Assert.IsFalse(view.showHideAnimatorForRejectSuccessState.isVisible);
                    Assert.IsFalse(view.showHideAnimatorForConfirmSuccessState.isVisible);
                    foreach (var button in view.buttonsToDisableOnPendingState)
                        Assert.IsTrue(button.interactable);
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.Pending:
                    Assert.IsTrue(view.showHideAnimatorForDefaultState.isVisible);
                    Assert.IsFalse(view.showHideAnimatorForRejectSuccessState.isVisible);
                    Assert.IsFalse(view.showHideAnimatorForConfirmSuccessState.isVisible);
                    foreach (var button in view.buttonsToDisableOnPendingState)
                        Assert.IsFalse(button.interactable);
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.ConfirmSuccess:
                    Assert.IsFalse(view.showHideAnimatorForDefaultState.isVisible);
                    Assert.IsFalse(view.showHideAnimatorForRejectSuccessState.isVisible);
                    Assert.IsTrue(view.showHideAnimatorForConfirmSuccessState.isVisible);
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.RejectSuccess:
                    Assert.IsFalse(view.showHideAnimatorForDefaultState.isVisible);
                    Assert.IsTrue(view.showHideAnimatorForRejectSuccessState.isVisible);
                    Assert.IsFalse(view.showHideAnimatorForConfirmSuccessState.isVisible);
                    break;
            }
        }

        [Test]
        public void ShowCorrectly()
        {
            // Act
            view.Show();

            // Assert
            Assert.IsTrue(view.showHideAnimatorForDefaultState.isVisible);
            Assert.IsFalse(view.showHideAnimatorForRejectSuccessState.isVisible);
            Assert.IsFalse(view.showHideAnimatorForConfirmSuccessState.isVisible);
            foreach (var button in view.buttonsToDisableOnPendingState)
                Assert.IsTrue(button.interactable);
        }
    }
}
