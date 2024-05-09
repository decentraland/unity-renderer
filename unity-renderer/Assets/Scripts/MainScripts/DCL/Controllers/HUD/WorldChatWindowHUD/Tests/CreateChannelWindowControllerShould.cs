using System;
using DCL.Chat.Channels;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Social.Chat
{
    public class CreateChannelWindowControllerShould
    {
        private IChatController chatController;
        private CreateChannelWindowController controller;
        private ICreateChannelWindowView view;

        [SetUp]
        public void SetUp()
        {
            chatController = Substitute.For<IChatController>();
            controller = new CreateChannelWindowController(chatController, new DataStore());
            view = Substitute.For<ICreateChannelWindowView>();
            controller.Initialize(view);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void Show()
        {
            controller.SetVisibility(true);

            view.Received(1).Show();
            view.Received(1).ClearInputText();
            view.Received(1).DisableCreateButton();
            view.Received(1).FocusInputField();
        }

        [Test]
        public void Hide()
        {
            controller.SetVisibility(false);

            view.Received(1).Hide();
        }

        [Test]
        public void UpdateViewWhenItsValidChannel()
        {
            chatController.GetAllocatedChannel("foo").Returns((Channel) null);
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnChannelNameUpdated += Raise.Event<Action<string>>("foo");

            view.Received(1).ClearError();
            view.Received(1).EnableCreateButton();
        }

        [Test]
        public void ShowChannelExistsErrorWithJoinOptionDisabled()
        {
            chatController.GetAllocatedChannel("foo").Returns(
                new Channel("foo", "fooName", 0, 2, true, false, ""));
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnChannelNameUpdated += Raise.Event<Action<string>>("foo");

            view.Received(1).ShowChannelExistsError(false);
            view.Received(1).DisableCreateButton();
        }

        [Test]
        public void ShowChannelExistsErrorWithJoinOptionEnabled()
        {
            chatController.GetAllocatedChannel("foo").Returns(
                new Channel("foo", "fooName", 0, 2, false, false, ""));
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnChannelNameUpdated += Raise.Event<Action<string>>("foo");

            view.Received(1).ShowChannelExistsError(true);
            view.Received(1).DisableCreateButton();
        }

        [Test]
        public void DisableCreationButtonWhenTextIsEmpty()
        {
            chatController.GetAllocatedChannel(Arg.Any<string>()).Returns((Channel) null);
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnChannelNameUpdated += Raise.Event<Action<string>>("");

            view.Received(1).DisableCreateButton();
        }

        [TestCase("cHaN _eL&")]
        [TestCase("#channel")]
        [TestCase("ch")]
        [TestCase("verylongtextmustfail32452632")]
        public void DisableCreationButtonWhenTheNameIsUnsupported(string text)
        {
            chatController.GetAllocatedChannel(Arg.Any<string>()).Returns((Channel) null);
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnChannelNameUpdated += Raise.Event<Action<string>>(text);

            view.Received(1).DisableCreateButton();
        }

        [Test]
        public void CreateChannel()
        {
            chatController.GetAllocatedChannel(Arg.Any<string>()).Returns((Channel) null);
            controller.SetVisibility(true);
            view.OnChannelNameUpdated += Raise.Event<Action<string>>("foo");
            view.ClearReceivedCalls();

            view.OnCreateSubmit += Raise.Event<Action>();

            chatController.Received(1).CreateChannel("foo");
            view.Received(1).DisableCreateButton();
        }

        [Test]
        public void JoinChannel()
        {
            chatController.GetAllocatedChannel(Arg.Any<string>()).Returns((Channel) null);
            controller.SetVisibility(true);
            view.OnChannelNameUpdated += Raise.Event<Action<string>>("foo");
            view.ClearReceivedCalls();

            view.OnJoinChannel += Raise.Event<Action>();

            chatController.Received(1).JoinOrCreateChannel("foo");
        }

        [Test]
        public void TriggerNavigationEventWhenJoinsChannel()
        {
            controller.SetVisibility(true);
            var navigatedChannel = "";
            controller.OnNavigateToChannelWindow += s => navigatedChannel = s;
            view.ClearReceivedCalls();

            chatController.OnChannelJoined += Raise.Event<Action<Channel>>(
                new Channel("foo", "fooName", 0, 2, false, false, ""));

            Assert.AreEqual("foo", navigatedChannel);
        }

        [Test]
        public void ShowWrongFormatErrorWhenJoiningChannel()
        {
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            chatController.OnJoinChannelError += Raise.Event<Action<string, ChannelErrorCode>>(
                "foo", ChannelErrorCode.WrongFormat);

            view.Received(1).ShowWrongFormatError();
            view.Received(1).DisableCreateButton();
        }

        [Test]
        public void ShowChannelsExceededWhenJoiningChannel()
        {
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            chatController.OnJoinChannelError += Raise.Event<Action<string, ChannelErrorCode>>(
                "foo", ChannelErrorCode.LimitExceeded);

            view.Received(1).ShowChannelsExceededError();
            view.Received(1).DisableCreateButton();
        }

        [Test]
        public void ShowUnknownError()
        {
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            chatController.OnJoinChannelError += Raise.Event<Action<string, ChannelErrorCode>>(
                "foo", ChannelErrorCode.Unknown);

            view.Received(1).ShowUnknownError();
            view.Received(1).EnableCreateButton();
        }

        [Test]
        public void ShowReservedNameError()
        {
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            chatController.OnJoinChannelError += Raise.Event<Action<string, ChannelErrorCode>>(
                "foo", ChannelErrorCode.ReservedName);

            view.Received(1).ShowChannelExistsError(false);
            view.Received(1).DisableCreateButton();
        }

        [Test]
        public void ShowAlreadyExistsError()
        {
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            chatController.OnJoinChannelError += Raise.Event<Action<string, ChannelErrorCode>>(
                "foo", ChannelErrorCode.AlreadyExists);

            view.Received(1).ShowChannelExistsError(true);
            view.Received(1).DisableCreateButton();
        }

        [Test]
        public void HideWhenClosesView()
        {
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnClose += Raise.Event<Action>();

            view.Received(1).Hide();
        }

        [Test]
        public void ShowTooShortError()
        {
            chatController.GetAllocatedChannel(Arg.Any<string>()).Returns((Channel) null);
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnChannelNameUpdated += Raise.Event<Action<string>>("df");

            view.Received(1).ShowTooShortError();
            view.Received(1).DisableCreateButton();
        }

        [TestCase("cha nnel")]
        [TestCase("cha$nnel")]
        [TestCase("channel_")]
        [TestCase("#channel")]
        public void ShowFormatError(string name)
        {
            chatController.GetAllocatedChannel(Arg.Any<string>()).Returns((Channel) null);
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnChannelNameUpdated += Raise.Event<Action<string>>(name);

            view.Received(1).ShowWrongFormatError();
            view.Received(1).DisableCreateButton();
        }
    }
}
