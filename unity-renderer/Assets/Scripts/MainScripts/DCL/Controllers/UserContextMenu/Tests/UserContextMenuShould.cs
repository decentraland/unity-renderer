using DCL;
using DCL.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

public class UserContextMenuShould
{
    const string TEST_USER_ID = "test_user_id";

    private UserContextMenu contextMenu;
    private UserProfileController profileController;
    private IFriendsController friendsController;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<UserContextMenu>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/UserContextMenu/UserContextMenuPanel.prefab");
        contextMenu = Object.Instantiate(prefab);
        contextMenu.socialAnalytics = Substitute.For<ISocialAnalytics>();

        var serviceLocator = ServiceLocatorTestFactory.CreateMocked();
        Environment.Setup(serviceLocator);
        friendsController = Environment.i.serviceLocator.Get<IFriendsController>();

        profileController = new GameObject().AddComponent<UserProfileController>();

        profileController.AddUserProfileToCatalog(new UserProfileModel
        {
            name = TEST_USER_ID,
            userId = TEST_USER_ID
        });

        DataStore.i.featureFlags.flags.Set(new FeatureFlag { flags = { ["friends_enabled"] = true } });

        yield break;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(contextMenu.gameObject);
        Object.Destroy(profileController.gameObject);

        yield break;
    }

    [Test]
    public void ShowContextMenuProperly()
    {
        bool showEventCalled = false;
        Action onShow = () => showEventCalled = true;
        contextMenu.OnShowMenu += onShow;

        contextMenu.Show(TEST_USER_ID);

        contextMenu.OnShowMenu -= onShow;
        Assert.IsTrue(contextMenu.gameObject.activeSelf, "The context menu should be visible.");
        Assert.IsTrue(showEventCalled);
    }

    [Test]
    public void HideContextMenuProperly()
    {
        contextMenu.Hide();

        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void HideContextMenuProperly(bool isActive)
    {
        contextMenu.SetFriendshipContentActive(isActive);

        Assert.AreEqual(isActive, contextMenu.friendshipContainer.activeSelf);
    }

    [Test]
    public void ClickOnPassportButton()
    {
        bool passportEventInvoked = false;
        Action<string> onPassport = (id) => passportEventInvoked = true;
        contextMenu.OnPassport += onPassport;

        contextMenu.Show(TEST_USER_ID);
        contextMenu.passportButton.onClick.Invoke();

        contextMenu.OnPassport -= onPassport;
        Assert.IsTrue(passportEventInvoked);
        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");
    }

    [Test]
    public void ClickOnReportButton()
    {
        bool reportEventInvoked = false;
        Action<string> onReport = (id) => reportEventInvoked = true;
        contextMenu.OnReport += onReport;

        contextMenu.Show(TEST_USER_ID);
        contextMenu.reportButton.onClick.Invoke();

        contextMenu.OnReport -= onReport;
        Assert.IsTrue(reportEventInvoked);
        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");
    }

    [Test]
    public void ClickOnBlockButton()
    {
        void TriggerConfirmAction(GenericConfirmationNotificationData current, GenericConfirmationNotificationData previous)
        {
            current.ConfirmAction?.Invoke();
        }

        var blockEventInvoked = false;
        Action<string, bool> onBlock = (id, block) => blockEventInvoked = true;
        contextMenu.OnBlock += onBlock;
        DataStore.i.notifications.GenericConfirmation.OnChange += TriggerConfirmAction;

        contextMenu.Show(TEST_USER_ID);
        contextMenu.blockButton.onClick.Invoke();

        Assert.IsTrue(blockEventInvoked);
        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");

        DataStore.i.notifications.GenericConfirmation.OnChange -= TriggerConfirmAction;
        contextMenu.OnBlock -= onBlock;
    }

    [Test]
    public void FriendSetupsCorrectly()
    {
        contextMenu.Show(TEST_USER_ID, UserContextMenu.MenuConfigFlags.Friendship);

        Assert.IsTrue(contextMenu.friendshipContainer.activeSelf, "friendshipContainer should be active");

        Assert.IsTrue(contextMenu.friendAddContainer.activeSelf, "friendAddContainer should be active");
        Assert.IsFalse(contextMenu.friendRemoveContainer.activeSelf, "friendRemoveContainer should not be active");

        Assert.IsFalse(contextMenu.friendRequestedContainer.activeSelf,
            "friendRequestedContainer should not be active");

        Assert.IsFalse(contextMenu.deleteFriendButton.gameObject.activeSelf, "deleteFriendButton should not be active");

        WhenFriendshipStatusUpdates(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.REQUESTED_TO
        });

        Assert.IsFalse(contextMenu.friendAddContainer.activeSelf, "friendAddContainer should not be active");
        Assert.IsFalse(contextMenu.friendRemoveContainer.activeSelf, "friendRemoveContainer should not be active");
        Assert.IsTrue(contextMenu.friendRequestedContainer.activeSelf, "friendRequestedContainer should be active");
        Assert.IsFalse(contextMenu.deleteFriendButton.gameObject.activeSelf, "deleteFriendButton should not be active");

        WhenFriendshipStatusUpdates(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.APPROVED
        });

        Assert.IsFalse(contextMenu.friendAddContainer.activeSelf, "friendAddContainer should not be active");
        Assert.IsTrue(contextMenu.friendRemoveContainer.activeSelf, "friendRemoveContainer should be active");

        Assert.IsFalse(contextMenu.friendRequestedContainer.activeSelf,
            "friendRequestedContainer should not be active");

        Assert.IsTrue(contextMenu.deleteFriendButton.gameObject.activeSelf, "deleteFriendButton should be active");

        WhenFriendshipStatusUpdates(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.DELETED
        });

        Assert.IsTrue(contextMenu.friendAddContainer.activeSelf, "friendAddContainer should be active");
        Assert.IsFalse(contextMenu.friendRemoveContainer.activeSelf, "friendRemoveContainer should not be active");

        Assert.IsFalse(contextMenu.friendRequestedContainer.activeSelf,
            "friendRequestedContainer should not be active");

        Assert.IsFalse(contextMenu.deleteFriendButton.gameObject.activeSelf, "deleteFriendButton should not be active");
    }

    [Test]
    public void MessageButtonSetupsCorrectly()
    {
        contextMenu.Show(TEST_USER_ID, UserContextMenu.MenuConfigFlags.Message);

        Assert.IsFalse(contextMenu.messageButton.gameObject.activeSelf, "messageButton should not be active");

        WhenFriendshipStatusUpdates(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.REQUESTED_TO
        });

        Assert.IsFalse(contextMenu.messageButton.gameObject.activeSelf, "messageButton should not be active");

        WhenFriendshipStatusUpdates(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.APPROVED
        });

        Assert.IsTrue(contextMenu.messageButton.gameObject.activeSelf, "messageButton should be active");

        WhenFriendshipStatusUpdates(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.DELETED
        });

        Assert.IsFalse(contextMenu.messageButton.gameObject.activeSelf, "messageButton should not be active");
    }

    private void WhenFriendshipStatusUpdates(FriendshipUpdateStatusMessage status)
    {
        friendsController.OnUpdateFriendship += Raise.Event<Action<string, FriendshipAction>>(status.userId, status.action);
    }
}
