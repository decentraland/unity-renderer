using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCl.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Friends
{
    public class FriendsHUDComponentViewShould
    {
        private FriendsHUDComponentView view;

        [SetUp]
        public void Setup()
        {
            // we need to add friends controller because the badge internally uses FriendsController.i
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<FriendsHUDComponentView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/FriendsHUD.prefab"));

            var serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            Environment.Setup(serviceLocator);

            var friendsController = serviceLocator.Get<IFriendsController>();
            friendsController.GetAllocatedFriends().Returns(new Dictionary<string, UserStatus>());

            view.Initialize(Substitute.For<IChatController>(),
                friendsController, Substitute.For<ISocialAnalytics>());
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        public void Show()
        {
            view.Show();

            Assert.IsTrue(view.gameObject.activeSelf);
            Assert.IsTrue(view.IsActive());
        }

        [Test]
        public void Hide()
        {
            view.Hide();

            Assert.IsFalse(view.gameObject.activeSelf);
            Assert.IsFalse(view.IsActive());
        }

        [Test]
        public void HideSpinner()
        {
            view.HideLoadingSpinner();

            Assert.IsFalse(view.loadingSpinner.activeSelf);
        }

        [Test]
        public void ShowSpinner()
        {
            view.ShowLoadingSpinner();

            Assert.IsTrue(view.loadingSpinner.activeSelf);
        }

        [UnityTest]
        public IEnumerator AddApprovedFriendshipAction()
        {
            const string userId = "userId";

            GivenFriendListTabFocused();
            GivenApprovedFriend(userId);

            Assert.IsNull(view.friendRequestsTab.Get(userId));
            Assert.AreEqual(1, view.friendsTab.Count);

            yield return null;

            Assert.IsTrue(view.ContainsFriend(userId));
            Assert.IsFalse(view.ContainsFriendRequest(userId));
            Assert.IsNotNull(view.GetEntry(userId));
            Assert.IsNotNull(view.friendsTab.Get(userId));
            Assert.AreEqual(0, view.FriendRequestCount);
            Assert.AreEqual(1, view.FriendCount);
        }

        [UnityTest]
        public IEnumerator RemoveFriendByFriendshipAction()
        {
            const string userId = "userId";

            GivenFriendListTabFocused();
            GivenApprovedFriend(userId);
            yield return null;
            GivenRemovedFriend(userId);
            yield return null;

            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsFalse(view.ContainsFriendRequest(userId));
            Assert.IsNull(view.GetEntry(userId));
            Assert.IsNull(view.friendRequestsTab.Get(userId));
            Assert.IsNull(view.friendsTab.Get(userId));
            Assert.AreEqual(0, view.FriendRequestCount);
            Assert.AreEqual(0, view.FriendCount);
        }

        [UnityTest]
        public IEnumerator AddAndRemoveFriendInstantly()
        {
            const string userId = "userId";

            GivenFriendListTabFocused();
            GivenApprovedFriend(userId);
            GivenRemovedFriend(userId);

            yield return null;

            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsFalse(view.ContainsFriendRequest(userId));
            Assert.IsNull(view.GetEntry(userId));
            Assert.IsNull(view.friendRequestsTab.Get(userId));
            Assert.IsNull(view.friendsTab.Get(userId));
            Assert.AreEqual(0, view.FriendRequestCount);
            Assert.AreEqual(0, view.FriendCount);
        }

        [UnityTest]
        public IEnumerator RejectFriendByFriendshipAction()
        {
            const string userId = "userId";

            GivenFriendListTabFocused();
            GivenApprovedFriend(userId);
            yield return null;
            GivenRemovedFriend(userId);
            yield return null;

            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsFalse(view.ContainsFriendRequest(userId));
            Assert.IsNull(view.GetEntry(userId));
            Assert.IsNull(view.friendRequestsTab.Get(userId));
            Assert.IsNull(view.friendsTab.Get(userId));
            Assert.AreEqual(0, view.FriendCount);
            Assert.AreEqual(0, view.FriendRequestCount);
        }

        [UnityTest]
        public IEnumerator AddReceivedRequestByFriendshipAction()
        {
            const string userId = "userId";

            GivenRequestTabFocused();
            GivenFriendRequestReceived(userId);
            yield return null;

            Assert.IsTrue(view.ContainsFriendRequest(userId));
            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsNotNull(view.GetEntry(userId));
            Assert.IsNotNull(view.friendRequestsTab.Get(userId));
            Assert.AreEqual(1, view.FriendRequestCount);
            Assert.AreEqual(0, view.FriendCount);
            Assert.IsNull(view.friendsTab.Get(userId));
        }

        [UnityTest]
        public IEnumerator AddSentRequestByFriendshipAction()
        {
            const string userId = "userId";

            GivenRequestTabFocused();
            GivenSentFriendRequest(userId);
            yield return null;

            Assert.IsTrue(view.ContainsFriendRequest(userId));
            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsNotNull(view.GetEntry(userId));
            Assert.IsNotNull(view.friendRequestsTab.Get(userId));
            Assert.AreEqual(1, view.FriendRequestCount);
            Assert.AreEqual(0, view.FriendCount);
            Assert.IsNull(view.friendsTab.Get(userId));
        }

        [UnityTest]
        public IEnumerator CancelRequestByFriendshipAction()
        {
            const string userId = "userId";

            GivenRequestTabFocused();
            GivenRemovedFriend(userId);
            yield return null;

            Assert.IsFalse(view.ContainsFriendRequest(userId));
            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsNull(view.GetEntry(userId));
            Assert.IsNull(view.friendRequestsTab.Get(userId));
            Assert.AreEqual(0, view.FriendRequestCount);
            Assert.AreEqual(0, view.FriendCount);
            Assert.IsNull(view.friendsTab.Get(userId));
        }

        [UnityTest]
        public IEnumerator UpdateFriendshipStatusToFriend()
        {
            const string userId = "userId";

            GivenFriendListTabFocused();

            view.Set(userId, new FriendEntryModel
            {
                blocked = false,
                coords = Vector2.one,
                avatarSnapshotObserver = Substitute.For<ILazyTextureObserver>(),
                status = PresenceStatus.OFFLINE,
                userId = userId,
                userName = "name"
            });

            yield return null;

            Assert.IsTrue(view.ContainsFriend(userId));
            Assert.IsFalse(view.ContainsFriendRequest(userId));
            Assert.IsNotNull(view.GetEntry(userId));
            Assert.IsNull(view.friendRequestsTab.Get(userId));
            Assert.AreEqual(0, view.FriendRequestCount);
            Assert.AreEqual(1, view.FriendCount);
            Assert.IsNotNull(view.friendsTab.Get(userId));
        }

        [UnityTest]
        public IEnumerator RemoveFriendship()
        {
            const string userId = "userId";

            GivenFriendListTabFocused();
            view.Remove(userId);
            yield return null;

            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsFalse(view.ContainsFriendRequest(userId));
            Assert.IsNull(view.GetEntry(userId));
            Assert.IsNull(view.friendRequestsTab.Get(userId));
            Assert.AreEqual(0, view.FriendRequestCount);
            Assert.AreEqual(0, view.FriendCount);
            Assert.IsNull(view.friendsTab.Get(userId));
        }

        [UnityTest]
        public IEnumerator UpdateFriendshipStatusToSentFriendship()
        {
            const string userId = "userId";

            GivenRequestTabFocused();

            view.Set(userId, new FriendRequestEntryModel
            {
                blocked = false,
                coords = Vector2.one,
                avatarSnapshotObserver = Substitute.For<ILazyTextureObserver>(),
                status = PresenceStatus.OFFLINE,
                userId = userId,
                userName = "name",
                isReceived = false
            });

            yield return null;

            Assert.IsTrue(view.ContainsFriendRequest(userId));
            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsNotNull(view.GetEntry(userId));
            Assert.IsNotNull(view.friendRequestsTab.Get(userId));
            Assert.AreEqual(1, view.FriendRequestCount);
            Assert.AreEqual(0, view.FriendCount);
            Assert.IsNull(view.friendsTab.Get(userId));
        }

        [UnityTest]
        public IEnumerator UpdateFriendshipStatusToReceivedFriendship()
        {
            const string userId = "userId";

            GivenRequestTabFocused();

            view.Set(userId, new FriendRequestEntryModel
            {
                blocked = false,
                coords = Vector2.one,
                avatarSnapshotObserver = Substitute.For<ILazyTextureObserver>(),
                status = PresenceStatus.OFFLINE,
                userId = userId,
                userName = "name",
                isReceived = true
            });

            yield return null;

            Assert.IsTrue(view.ContainsFriendRequest(userId));
            Assert.IsFalse(view.ContainsFriend(userId));
            Assert.IsNotNull(view.GetEntry(userId));
            Assert.IsNotNull(view.friendRequestsTab.Get(userId));
            Assert.AreEqual(1, view.FriendRequestCount);
            Assert.AreEqual(0, view.FriendCount);
            Assert.IsNull(view.friendsTab.Get(userId));
        }

        [Test]
        public void ShowMoreFriendsToLoadHint()
        {
            view.ShowMoreFriendsToLoadHint(3);

            Assert.IsTrue(view.friendsTab.loadMoreEntriesContainer.activeSelf);
            Assert.AreEqual("3 friends hidden. Use the search bar to find them or scroll down to show more.", view.friendsTab.loadMoreEntriesLabel.text);
        }

        [Test]
        public void ShowMoreRequestsToLoadHint()
        {
            view.ShowMoreRequestsToLoadHint(7);

            Assert.IsTrue(view.friendRequestsTab.loadMoreEntriesContainer.activeSelf);
            Assert.AreEqual("7 requests hidden. Scroll down to show more.", view.friendRequestsTab.loadMoreEntriesLabel.text);
        }

        [UnityTest]
        public IEnumerator RequireMoreFriendEntries()
        {
            var called = false;
            view.OnRequireMoreFriends += () => called = true;
            GivenFriendListTabFocused();
            GivenApprovedFriend("bleh");
            view.ShowMoreFriendsToLoadHint(6);

            // wait until queued entry is created
            yield return null;

            view.friendsTab.scroll.onValueChanged.Invoke(Vector2.zero);

            Assert.IsFalse(called);

            // wait for the internal delay
            yield return new WaitForSeconds(1.5f);

            Assert.IsTrue(called);
        }

        [UnityTest]
        public IEnumerator RequireMoreRequestEntries()
        {
            var called = false;
            view.OnRequireMoreFriendRequests += () => called = true;
            GivenRequestTabFocused();
            GivenFriendRequestReceived("bleh");
            view.ShowMoreRequestsToLoadHint(5);

            // wait until queued entry is created
            yield return null;

            view.friendRequestsTab.scroll.onValueChanged.Invoke(Vector2.zero);

            Assert.IsFalse(called);

            // wait for the internal delay
            yield return new WaitForSeconds(1.5f);

            Assert.IsTrue(called);
        }

        [UnityTest]
        public IEnumerator ReceiveRequestThenConvertIntoFriend()
        {
            const string userId = "userId";

            yield return AddReceivedRequestByFriendshipAction();
            yield return AddApprovedFriendshipAction();

            Assert.IsTrue(view.ContainsFriend(userId));
            Assert.IsFalse(view.ContainsFriendRequest(userId));
            Assert.IsNotNull(view.GetEntry(userId));
            Assert.IsNotNull(view.friendsTab.Get(userId));
            Assert.AreEqual(0, view.FriendRequestCount);
            Assert.AreEqual(1, view.FriendCount);
        }

        [Test]
        public void ShowLoadingSpinner()
        {
            view.ShowLoadingSpinner();

            Assert.IsTrue(view.loadingSpinner.activeSelf);
        }

        [Test]
        public void HideLoadingSpinner()
        {
            view.HideLoadingSpinner();

            Assert.IsFalse(view.loadingSpinner.activeSelf);
        }

        private void GivenSentFriendRequest(string userId)
        {
            view.Set(userId, new FriendRequestEntryModel
            {
                blocked = false,
                coords = Vector2.one,
                avatarSnapshotObserver = Substitute.For<ILazyTextureObserver>(),
                status = PresenceStatus.OFFLINE,
                userId = userId,
                userName = "name",
                isReceived = false
            });
        }

        private void GivenFriendRequestReceived(string userId)
        {
            view.Set(userId, new FriendRequestEntryModel
            {
                blocked = false,
                coords = Vector2.one,
                avatarSnapshotObserver = Substitute.For<ILazyTextureObserver>(),
                status = PresenceStatus.OFFLINE,
                userId = userId,
                userName = "name",
                isReceived = true
            });
        }

        private void GivenRequestTabFocused() =>
            view.FocusTab(1);

        private void GivenFriendListTabFocused() =>
            view.FocusTab(0);

        private void GivenRemovedFriend(string userId) =>
            view.Remove(userId);

        private void GivenApprovedFriend(string userId)
        {
            view.Set(userId, new FriendEntryModel
            {
                blocked = false,
                coords = Vector2.one,
                avatarSnapshotObserver = Substitute.For<ILazyTextureObserver>(),
                status = PresenceStatus.OFFLINE,
                userId = userId,
                userName = "name"
            });
        }
    }
}
