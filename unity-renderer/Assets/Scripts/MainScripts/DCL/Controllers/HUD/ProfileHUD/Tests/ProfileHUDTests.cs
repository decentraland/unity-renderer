using DCL;
using DCL.Browser;
using DCL.MyAccount;
using NUnit.Framework;
using System.Collections;
using NSubstitute;
using SocialFeaturesAnalytics;
using System;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class ProfileHUDTests : IntegrationTestSuite_Legacy
{
    private UserProfile userProfile;
    private ProfileHUDController controller;
    private IUserProfileBridge userProfileBridge;
    private ISocialAnalytics socialAnalytics;
    private bool allUIHiddenOriginalValue;
    private DataStore dataStore;
    private IProfileHUDView view;
    private IBrowserBridge browserBridge;


    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.GetOwn().Returns(userProfile);
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        allUIHiddenOriginalValue = CommonScriptableObjects.allUIHidden.Get();
        CommonScriptableObjects.allUIHidden.Set(false);
        dataStore = new DataStore();
        dataStore.exploreV2.isInitialized.Set(true, false);
        view = Substitute.For<IProfileHUDView>();
        browserBridge = Substitute.For<IBrowserBridge>();

        controller = new ProfileHUDController(view,
            userProfileBridge, socialAnalytics, dataStore,
            new MyAccountCardController(Substitute.For<IMyAccountCardComponentView>(),
                dataStore, userProfileBridge, null, browserBridge), browserBridge);
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        Object.Destroy(userProfile);
        controller.Dispose();
        CommonScriptableObjects.allUIHidden.Set(allUIHiddenOriginalValue);
        yield return base.TearDown();
    }

    [Test]
    public void Creation()
    {
        // Assert
        view.Received(1).SetWalletSectionEnabled(false);
        view.Received(1).SetNonWalletSectionEnabled(false);
        view.Received(1).SetDescriptionIsEditing(false);
        view.Received(1).SetStartMenuButtonActive(true);
    }

    [Test]
    public void LogOut()
    {
        // Act
        view.LogedOutPressed += Raise.Event<EventHandler>();

        // Assert
        userProfileBridge.Received(1).LogOut();
    }

    [Test]
    public void SignUp()
    {
        // Act
        view.SignedUpPressed += Raise.Event<EventHandler>();

        // Assert
        userProfileBridge.Received(1).SignUp();
    }

    [Test]
    public void OpenView()
    {
        // Arrange
        var isOpen = false;
        controller.OnOpen += () => isOpen = true;

        // Act
        view.Opened += Raise.Event<EventHandler>();

        // Assert
        userProfileBridge.Received(1).RequestOwnProfileUpdate();
        Assert.IsTrue(isOpen);
    }

    [Test]
    public void CloseView()
    {
        // Arrange
        var isClosed = false;
        controller.OnClose += () => isClosed = true;

        // Act
        view.Closed += Raise.Event<EventHandler>();

        Assert.IsTrue(isClosed);
    }
}
