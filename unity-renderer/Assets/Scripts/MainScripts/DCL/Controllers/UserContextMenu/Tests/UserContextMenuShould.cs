using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using SocialFeaturesAnalytics;
using NSubstitute;

public class UserContextMenuShould
{
    const string TEST_USER_ID = "test_user_id";

    private UserContextMenu contextMenu;
    private FriendsController friendsController;
    private UserProfileController profileController;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var prefab = Resources.Load<UserContextMenu>("UserContextMenuPanel");
        contextMenu = UnityEngine.Object.Instantiate(prefab);
        contextMenu.socialAnalytics = Substitute.For<ISocialAnalytics>();

        friendsController = (new GameObject()).AddComponent<FriendsController>();
        profileController = (new GameObject()).AddComponent<UserProfileController>();
        profileController.AddUserProfileToCatalog(new UserProfileModel()
        {
            name = TEST_USER_ID,
            userId = TEST_USER_ID
        });

        yield break;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(contextMenu.gameObject);
        UnityEngine.Object.Destroy(friendsController.gameObject);
        UnityEngine.Object.Destroy(profileController.gameObject);

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
        bool blockEventInvoked = false;
        Action<string, bool> onBlock = (id, block) => blockEventInvoked = true;
        contextMenu.OnBlock += onBlock;

        contextMenu.Show(TEST_USER_ID);
        contextMenu.blockButton.onClick.Invoke();

        contextMenu.OnBlock -= onBlock;
        Assert.IsTrue(blockEventInvoked);
        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");
    }

    [Test]
    public void FriendSetupsCorrectly()
    {
        contextMenu.Show(TEST_USER_ID, UserContextMenu.MenuConfigFlags.Friendship);

        Assert.IsTrue(contextMenu.friendshipContainer.activeSelf, "friendshipContainer should be active");

        Assert.IsTrue(contextMenu.friendAddContainer.activeSelf, "friendAddContainer should be active");
        Assert.IsFalse(contextMenu.friendRemoveContainer.activeSelf, "friendRemoveContainer should not be active");
        Assert.IsFalse(contextMenu.friendRequestedContainer.activeSelf, "friendRequestedContainer should not be active");
        Assert.IsFalse(contextMenu.deleteFriendButton.gameObject.activeSelf, "deleteFriendButton should not be active");

        FriendsController.i.UpdateFriendshipStatus(JsonUtility.ToJson(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.REQUESTED_TO
        }));

        Assert.IsFalse(contextMenu.friendAddContainer.activeSelf, "friendAddContainer should not be active");
        Assert.IsFalse(contextMenu.friendRemoveContainer.activeSelf, "friendRemoveContainer should not be active");
        Assert.IsTrue(contextMenu.friendRequestedContainer.activeSelf, "friendRequestedContainer should be active");
        Assert.IsFalse(contextMenu.deleteFriendButton.gameObject.activeSelf, "deleteFriendButton should not be active");

        FriendsController.i.UpdateFriendshipStatus(JsonUtility.ToJson(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.APPROVED
        }));

        Assert.IsFalse(contextMenu.friendAddContainer.activeSelf, "friendAddContainer should not be active");
        Assert.IsTrue(contextMenu.friendRemoveContainer.activeSelf, "friendRemoveContainer should be active");
        Assert.IsFalse(contextMenu.friendRequestedContainer.activeSelf, "friendRequestedContainer should not be active");
        Assert.IsTrue(contextMenu.deleteFriendButton.gameObject.activeSelf, "deleteFriendButton should be active");

        FriendsController.i.UpdateFriendshipStatus(JsonUtility.ToJson(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.DELETED
        }));

        Assert.IsTrue(contextMenu.friendAddContainer.activeSelf, "friendAddContainer should be active");
        Assert.IsFalse(contextMenu.friendRemoveContainer.activeSelf, "friendRemoveContainer should not be active");
        Assert.IsFalse(contextMenu.friendRequestedContainer.activeSelf, "friendRequestedContainer should not be active");
        Assert.IsFalse(contextMenu.deleteFriendButton.gameObject.activeSelf, "deleteFriendButton should not be active");
    }

    [Test]
    public void MessageButtonSetupsCorrectly()
    {
        contextMenu.Show(TEST_USER_ID, UserContextMenu.MenuConfigFlags.Message);

        Assert.IsFalse(contextMenu.messageButton.gameObject.activeSelf, "messageButton should not be active");

        FriendsController.i.UpdateFriendshipStatus(JsonUtility.ToJson(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.REQUESTED_TO
        }));

        Assert.IsFalse(contextMenu.messageButton.gameObject.activeSelf, "messageButton should not be active");

        FriendsController.i.UpdateFriendshipStatus(JsonUtility.ToJson(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.APPROVED
        }));

        Assert.IsTrue(contextMenu.messageButton.gameObject.activeSelf, "messageButton should be active");

        FriendsController.i.UpdateFriendshipStatus(JsonUtility.ToJson(new FriendshipUpdateStatusMessage
        {
            userId = TEST_USER_ID,
            action = FriendshipAction.DELETED
        }));

        Assert.IsFalse(contextMenu.messageButton.gameObject.activeSelf, "messageButton should not be active");
    }
}