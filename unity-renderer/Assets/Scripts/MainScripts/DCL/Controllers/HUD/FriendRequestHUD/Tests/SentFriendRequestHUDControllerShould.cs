using Cysharp.Threading.Tasks;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Friends
{
    public class SentFriendRequestHUDControllerShould
    {
        private const string FRIEND_REQ_ID = "friendRequest";
        private const string RECIPIENT_ID = "recipientId";
        private const string RECIPIENT_NAME = "recipientName";
        private const string OWN_ID = "ownId";

        private SentFriendRequestHUDController controller;
        private ISentFriendRequestHUDView view;
        private DataStore dataStore;
        private IFriendsController friendsController;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<ISentFriendRequestHUDView>();
            dataStore = new DataStore();

            friendsController = Substitute.For<IFriendsController>();

            friendsController.TryGetAllocatedFriendRequest(FRIEND_REQ_ID, out Arg.Any<FriendRequest>())
                             .Returns((args) =>
                              {
                                  args[1] = new FriendRequest(FRIEND_REQ_ID, new DateTime(621355968001000000), OWN_ID, RECIPIENT_ID, "hey");
                                  return true;
                              });

            IUserProfileBridge userProfileBridge = Substitute.For<IUserProfileBridge>();
            var recipientProfile = ScriptableObject.CreateInstance<UserProfile>();

            recipientProfile.UpdateData(new UserProfileModel
            {
                userId = RECIPIENT_ID,
                name = RECIPIENT_NAME,
            });

            userProfileBridge.Get(RECIPIENT_ID).Returns(recipientProfile);

            var ownProfile = ScriptableObject.CreateInstance<UserProfile>();

            ownProfile.UpdateData(new UserProfileModel
            {
                userId = RECIPIENT_ID,
                name = RECIPIENT_NAME,
            });

            userProfileBridge.GetOwn().Returns(ownProfile);

            controller = new SentFriendRequestHUDController(view,
                dataStore,
                userProfileBridge,
                friendsController,
                Substitute.For<ISocialAnalytics>());

            view.ClearReceivedCalls();
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void Show()
        {
            WhenRequestedToShow();

            view.Received(1).SetBodyMessage("hey");
            view.Received(1).SetTimestamp(Arg.Is<DateTime>(d => d.Ticks == 621355968001000000));
            view.Received(1).SetRecipientName(RECIPIENT_NAME);
            view.Received(1).SetRecipientProfilePicture(Arg.Any<ILazyTextureObserver>());
            view.Received(1).SetSenderProfilePicture(Arg.Any<ILazyTextureObserver>());
        }

        [Test]
        public void Hide()
        {
            dataStore.HUDs.openSentFriendRequestDetail.Set(null, true);

            view.Received(1).Close();
        }

        [Test]
        public void CancelFriendship()
        {
            friendsController.CancelRequestAsync(FRIEND_REQ_ID, Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(
                                  new FriendRequest("friendReqId", new DateTime(200), OWN_ID, RECIPIENT_ID, "woah")));

            WhenRequestedToShow();
            view.OnCancel += Raise.Event<Action>();

            view.Received(1).ShowPendingToCancel();
            view.Received(1).Close();
            friendsController.Received(1).CancelRequestAsync(FRIEND_REQ_ID, Arg.Any<CancellationToken>());
        }

        [Test]
        public void ShowFailWhenTimeout()
        {
            LogAssert.Expect(LogType.Exception, new Regex("TimeoutException"));

            friendsController.CancelRequestAsync(FRIEND_REQ_ID, Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromException<FriendRequest>(new TimeoutException()));

            WhenRequestedToShow();
            view.OnCancel += Raise.Event<Action>();

            view.Received(1).ShowPendingToCancel();
            friendsController.Received(1).CancelRequestAsync(FRIEND_REQ_ID, Arg.Any<CancellationToken>());
        }

        private void WhenRequestedToShow()
        {
            dataStore.HUDs.openSentFriendRequestDetail.Set(FRIEND_REQ_ID, true);
        }
    }
}
