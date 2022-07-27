using System;
using DCL.Chat.Channels;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Chat.HUD
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
            controller = new CreateChannelWindowController(chatController);
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
                new Channel("foo", 0, 2, true, false, "", 0));
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
                new Channel("foo", 0, 2, false, false, "", 0));
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
        
        [Test]
        public void DisableCreationButtonWhenTextIsTooLong()
        {
            chatController.GetAllocatedChannel(Arg.Any<string>()).Returns((Channel) null);
            controller.SetVisibility(true);
            view.ClearReceivedCalls();

            view.OnChannelNameUpdated += Raise.Event<Action<string>>("verylongtextmustfail");
            
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
        public void TriggerNavigationEventWhenJoinsChannel()
        {
            controller.SetVisibility(true);
            var navigatedChannel = "";
            controller.OnNavigateToChannelWindow += s => navigatedChannel = s; 
            view.ClearReceivedCalls();
            
            chatController.OnChannelJoined += Raise.Event<Action<Channel>>(
                new Channel("foo", 0, 2, false, false, "", 0));
            
            Assert.AreEqual("foo", navigatedChannel);
        }
        
        [Test]
        public void ShowErrorWhenCannotJoinChannel()
        {   
            controller.SetVisibility(true);
            view.ClearReceivedCalls();
            
            chatController.OnJoinChannelError += Raise.Event<Action<string, string>>(
                "foo", "error!");
            
            view.Received(1).ShowError("error!");
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
    }
}