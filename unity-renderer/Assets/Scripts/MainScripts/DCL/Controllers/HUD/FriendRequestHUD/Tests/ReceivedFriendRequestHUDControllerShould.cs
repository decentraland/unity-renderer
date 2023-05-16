using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDControllerShould
    {
        private const string FRIEND_REQUEST_ID = "friendRequestId";
        private const string OWN_ID = "ownId";
        private const string SENDER_ID = "recipientId";

        private ReceivedFriendRequestHUDController controller;
        private IReceivedFriendRequestHUDView view;
        private IFriendsController friendsController;
        private IUserProfileBridge userProfileBridge;
        private DataStore dataStore;
        private IFriendRequestHUDView friendRequestHUDView;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IReceivedFriendRequestHUDView>();
            friendsController = Substitute.For<IFriendsController>();

            friendsController.TryGetAllocatedFriendRequest(FRIEND_REQUEST_ID, out Arg.Any<FriendRequest>())
                             .Returns(
                                  (args) =>
                                  {
                                      args[1] = new FriendRequest(FRIEND_REQUEST_ID, 100, SENDER_ID, OWN_ID, "hey");
                                      return true;
                                  });

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            var ownProfile = ScriptableObject.CreateInstance<UserProfile>();

            ownProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_ID,
                name = "ownName",
                snapshots = new UserProfileModel.Snapshots
                {
                    face256 = "ownFaceUrl"
                }
            });

            var recipientProfile = ScriptableObject.CreateInstance<UserProfile>();

            recipientProfile.UpdateData(new UserProfileModel
            {
                userId = SENDER_ID,
                name = "senderName",
                snapshots = new UserProfileModel.Snapshots
                {
                    face256 = "senderFaceUrl"
                }
            });

            userProfileBridge.GetOwn().Returns(ownProfile);
            userProfileBridge.Get(SENDER_ID).Returns(recipientProfile);
            dataStore = new DataStore();

            friendRequestHUDView = Substitute.For<IFriendRequestHUDView>();

            controller = new ReceivedFriendRequestHUDController(dataStore,
                view,
                new FriendRequestHUDController(friendRequestHUDView),
                friendsController,
                userProfileBridge,
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
            WhenShow();

            view.Received(1).SetBodyMessage("hey");
            view.Received(1).SetTimestamp(Arg.Is<DateTime>(d => d.Ticks == 621355968001000000));
            view.Received(1).SetSenderName("senderName");
            view.Received(1).SetSenderProfilePicture("senderFaceUrl");
            view.Received(1).SetRecipientProfilePicture("ownFaceUrl");
            view.Received(1).Show();
        }

        [Test]
        public void Hide()
        {
            WhenShow();
            dataStore.HUDs.openReceivedFriendRequestDetail.Set(null, true);

            friendRequestHUDView.Received(1).Close();
        }

        [Test]
        public void HideWhenViewCloses()
        {
            WhenShow();

            view.OnClose += Raise.Event<Action>();

            friendRequestHUDView.Received(1).Close();
        }

        [Test]
        public void OpenProfile()
        {
            WhenShow();

            view.OnOpenProfile += Raise.Event<Action>();

            Assert.AreEqual(SENDER_ID, dataStore.HUDs.currentPlayerId.Get().playerId);
        }

        [UnityTest]
        public IEnumerator RejectFriendRequest()
        {
            WhenShow();

            friendsController.RejectFriendshipAsync(FRIEND_REQUEST_ID, Arg.Any<CancellationToken>())
                             .Returns(
                                  UniTask.FromResult(new FriendRequest(FRIEND_REQUEST_ID, 100, SENDER_ID, OWN_ID, "hey")));

            view.OnRejectFriendRequest += Raise.Event<Action>();

            yield return new WaitForSeconds(3.5f);

            Received.InOrder(() =>
            {
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.RejectSuccess);
                friendRequestHUDView.Close();
            });
        }

        [Test]
        public void FailWhenRejectTimeouts()
        {
            LogAssert.Expect(LogType.Exception, new Regex("TimeoutException"));

            friendsController.RejectFriendshipAsync(FRIEND_REQUEST_ID, Arg.Any<CancellationToken>())
                             .Returns(
                                  UniTask.FromException<FriendRequest>(new TimeoutException()));

            WhenShow();
            view.ClearReceivedCalls();

            view.OnRejectFriendRequest += Raise.Event<Action>();

            Received.InOrder(() =>
            {
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Default);
            });
        }

        [UnityTest]
        public IEnumerator ConfirmFriendship()
        {
            friendsController.AcceptFriendshipAsync(FRIEND_REQUEST_ID, Arg.Any<CancellationToken>())
                             .Returns(
                                  UniTask.FromResult(new FriendRequest(FRIEND_REQUEST_ID, 100, SENDER_ID, OWN_ID, "hey")));

            WhenShow();
            view.ClearReceivedCalls();

            view.OnConfirmFriendRequest += Raise.Event<Action>();

            yield return new WaitForSeconds(3.5f);

            Received.InOrder(() =>
            {
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.ConfirmSuccess);
                friendRequestHUDView.Close();
            });
        }

        [Test]
        public void FailWhenConfirmFriendshipTimeouts()
        {
            LogAssert.Expect(LogType.Exception, new Regex("TimeoutException"));

            friendsController.AcceptFriendshipAsync(FRIEND_REQUEST_ID, Arg.Any<CancellationToken>())
                             .Returns(
                                  UniTask.FromException<FriendRequest>(new TimeoutException()));

            WhenShow();
            view.ClearReceivedCalls();

            view.OnConfirmFriendRequest += Raise.Event<Action>();

            Received.InOrder(() =>
            {
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Pending);
                view.SetState(ReceivedFriendRequestHUDModel.LayoutState.Default);
            });
        }

        private void WhenShow()
        {
            dataStore.HUDs.openReceivedFriendRequestDetail.Set(FRIEND_REQUEST_ID, true);
        }
    }
}
