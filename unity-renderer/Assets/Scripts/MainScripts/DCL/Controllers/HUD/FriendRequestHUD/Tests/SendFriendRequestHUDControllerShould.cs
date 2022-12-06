using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDControllerShould
    {
        private const string RECIPIENT_ID = "userId";
        private const string RECIPIENT_NAME = "userName";
        private const string OWN_ID = "ownId";

        private SendFriendRequestHUDController controller;
        private DataStore dataStore;
        private ISendFriendRequestHUDView view;
        private UserProfile recipientProfile;
        private ISocialAnalytics socialAnalytics;
        private IFriendsController friendsController;

        [SetUp]
        public void SetUp()
        {
            dataStore = new DataStore();

            view = Substitute.For<ISendFriendRequestHUDView>();

            IUserProfileBridge userProfileBridge = Substitute.For<IUserProfileBridge>();

            recipientProfile = ScriptableObject.CreateInstance<UserProfile>();

            recipientProfile.UpdateData(new UserProfileModel
            {
                userId = RECIPIENT_ID,
                name = RECIPIENT_NAME
            });

            var ownProfile = ScriptableObject.CreateInstance<UserProfile>();

            ownProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_ID
            });

            userProfileBridge.GetOwn().Returns(ownProfile);

            userProfileBridge.Get(RECIPIENT_ID).Returns(recipientProfile);

            socialAnalytics = Substitute.For<ISocialAnalytics>();

            friendsController = Substitute.For<IFriendsController>();

            controller = new SendFriendRequestHUDController(
                view,
                dataStore,
                userProfileBridge,
                friendsController,
                socialAnalytics);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void Show()
        {
            dataStore.HUDs.sendFriendRequest.Set(RECIPIENT_ID, notifyEvent: true);

            view.Received(1).SetName(RECIPIENT_NAME);
            view.Received(1).SetProfilePicture(recipientProfile.snapshotObserver);
            view.Received(1).ClearInputField();
            view.Received(1).Show();
        }

        [Test]
        public void Hide()
        {
            view.ClearReceivedCalls();

            dataStore.HUDs.sendFriendRequest.Set(null, notifyEvent: true);

            view.Received(1).Close();
        }

        [TestCase("")]
        [TestCase("hey!")]
        [TestCase("very long message which requires a lot of time to read.. omg so much time")]
        public void SendWithBodyMessage(string bodyMessage)
        {
            dataStore.HUDs.sendFriendRequestSource.Set(0);
            dataStore.HUDs.sendFriendRequest.Set(RECIPIENT_ID, true);
            friendsController.RequestFriendshipAsync(RECIPIENT_ID, Arg.Any<string>())
                             .Returns(UniTask.FromResult(new FriendRequest("frid", 100, OWN_ID, RECIPIENT_ID, bodyMessage)));

            view.OnMessageBodyChanged += Raise.Event<Action<string>>(bodyMessage);
            view.OnSend += Raise.Event<Action>();

            socialAnalytics.Received(1)
                           .SendFriendRequestSent(OWN_ID, RECIPIENT_ID, bodyMessage.Length,
                                PlayerActionSource.Passport);

            friendsController.Received(1).RequestFriendshipAsync(RECIPIENT_ID, bodyMessage);
            view.Received(1).ShowSendSuccess();
        }

        [Test]
        public void FailWithTimeout()
        {
            LogAssert.Expect(LogType.Exception, new Regex("TimeoutException"));
            dataStore.HUDs.sendFriendRequestSource.Set(0);
            dataStore.HUDs.sendFriendRequest.Set(RECIPIENT_ID, true);
            friendsController.RequestFriendshipAsync(RECIPIENT_ID, Arg.Any<string>())
                             .Returns(UniTask.FromException<FriendRequest>(new TimeoutException()));

            view.OnMessageBodyChanged += Raise.Event<Action<string>>("hey");
            view.OnSend += Raise.Event<Action>();

            socialAnalytics.DidNotReceiveWithAnyArgs().SendFriendRequestSent(default, default, default, default);
            view.DidNotReceiveWithAnyArgs().ShowSendSuccess();
            friendsController.Received(1).RequestFriendshipAsync(RECIPIENT_ID, "hey");
            view.Received(1).ShowSendFailed();
        }
    }
}
