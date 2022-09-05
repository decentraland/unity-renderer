using System;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Chat.HUD
{
    public class LeaveChannelConfirmationWindowControllerShould
    {
        private LeaveChannelConfirmationWindowController leaveChannelWindowController;
        private ILeaveChannelConfirmationWindowComponentView leaveChannelWindowComponentView;
        private IChatController chatController;

        [SetUp]
        public void SetUp()
        {
            leaveChannelWindowComponentView = Substitute.For<ILeaveChannelConfirmationWindowComponentView>();
            chatController = Substitute.For<IChatController>();
            leaveChannelWindowController = new LeaveChannelConfirmationWindowController(chatController);
            leaveChannelWindowController.Initialize(leaveChannelWindowComponentView);
        }

        [TearDown]
        public void TearDown() { leaveChannelWindowController.Dispose(); }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.AreEqual(leaveChannelWindowComponentView, leaveChannelWindowController.joinChannelView);
            Assert.AreEqual(chatController, leaveChannelWindowController.chatController);
        }

        [Test]
        [TestCase("TestId")]
        [TestCase(null)]
        public void SetChannelToLeaveCorrectly(string testChannelId)
        {
            // Act
            leaveChannelWindowController.SetChannelToLeave(testChannelId);

            // Assert
            leaveChannelWindowComponentView.Received(1).SetChannel(testChannelId);
        }

        [Test]
        public void RaiseOnCancelJoinCorrectly()
        {
            // Act
            leaveChannelWindowComponentView.OnCancelLeave += Raise.Event<Action>();

            // Assert
            leaveChannelWindowComponentView.Received(1).Hide();
        }

        [Test]
        public void RaiseOnConfirmJoinCorrectly()
        {
            // Arrange
            string testChannelId = "TestId";

            // Act
            leaveChannelWindowComponentView.OnConfirmLeave += Raise.Event<Action<string>>(testChannelId);

            // Assert
            chatController.Received(1).LeaveChannel(testChannelId);
        }
    }
}