using DCL.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using SocialBar.UserThumbnail;
using SocialFeaturesAnalytics;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UserThumbnailComponentViewMock : UserThumbnailComponentView
{
    public UserThumbnailComponentModel ConfiguredWith { get; private set; }

    public override void Configure(UserThumbnailComponentModel model) => ConfiguredWith = model;
}

public class PrivateChatWindowComponentViewShould
{
    private PrivateChatWindowComponentView view;
    private UserThumbnailComponentViewMock userThumbnail;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;

    [SetUp]
    public void SetUp()
    {
        view = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<PrivateChatWindowComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/PrivateChatHUD.prefab"));

        userThumbnail = new GameObject("userThumbnail").AddComponent<UserThumbnailComponentViewMock>();
        view.userThumbnail = userThumbnail;

        friendsController = Substitute.For<IFriendsController>();
        friendsController.GetAllocatedFriends().Returns(x => new Dictionary<string, UserStatus>());
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        view.Initialize(friendsController, socialAnalytics);
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
        Object.Destroy(userThumbnail.gameObject);
    }

    [Test]
    public void Show()
    {
        view.Show();

        Assert.IsTrue(view.gameObject.activeSelf);
    }

    [Test]
    public void Hide()
    {
        view.Hide();

        Assert.IsFalse(view.gameObject.activeSelf);
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public void Configure(bool online, bool blocked)
    {
        var profile = ScriptableObject.CreateInstance<UserProfile>();
        profile.UpdateData(new UserProfileModel{userId = "userId", name = "name", snapshots = new UserProfileModel.Snapshots{face256 = "someurl"}});

        view.Setup(profile, online, blocked);

        Assert.AreEqual("someurl", userThumbnail.ConfiguredWith.faceUrl);
        Assert.AreEqual(blocked, userThumbnail.ConfiguredWith.isBlocked);
        Assert.AreEqual(online, userThumbnail.ConfiguredWith.isOnline);
        Assert.AreEqual("name", view.userNameLabel.text);
        Assert.AreEqual(online, view.jumpInButtonContainer.activeSelf);
    }

    [Test]
    public void TriggerClose()
    {
        var called = false;
        view.OnClose += () => called = true;

        view.closeButton.onClick.Invoke();

        Assert.IsTrue(called);
    }

    [Test]
    public void TriggerBack()
    {
        var called = false;
        view.OnPressBack += () => called = true;

        view.backButton.onClick.Invoke();

        Assert.IsTrue(called);
    }

    [Test]
    public void TriggerFocusWhenWindowIsClicked()
    {
        var clicked = false;
        view.OnClickOverWindow += () => clicked = true;

        view.OnPointerDown(null);

        Assert.IsTrue(clicked);
    }

    [Test]
    public void TriggerFocusWhenWindowIsHovered()
    {
        var focused = false;
        view.OnFocused += f => focused = f;

        view.OnPointerEnter(null);

        Assert.IsTrue(focused);
    }

    [Test]
    public void TriggerUnfocusWhenPointerExits()
    {
        var focused = true;
        view.OnFocused += f => focused = f;

        view.OnPointerExit(null);

        Assert.IsFalse(focused);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetLoadingMessagesActiveCorrectly(bool isActive)
    {
        view.messagesLoading.SetActive(!isActive);

        view.SetLoadingMessagesActive(isActive);

        Assert.AreEqual(isActive, view.messagesLoading.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetOldMessagesLoadingActiveCorrectly(bool isActive)
    {
        view.oldMessagesLoadingContainer.SetActive(!isActive);

        view.SetOldMessagesLoadingActive(isActive);

        Assert.AreEqual(isActive, view.oldMessagesLoadingContainer.activeSelf);
    }
}
