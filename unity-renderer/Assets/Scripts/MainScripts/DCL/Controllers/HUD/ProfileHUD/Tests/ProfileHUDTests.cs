using DCL;
using NUnit.Framework;
using System.Collections;
using NSubstitute;
using SocialFeaturesAnalytics;
using UnityEngine;
using UnityEngine.TestTools;

public class ProfileHUDTests : IntegrationTestSuite_Legacy
{
    private UserProfile userProfile;
    private ProfileHUDController controller;
    private BaseComponentView baseView;
    private IUserProfileBridge userProfileBridge;
    private ISocialAnalytics socialAnalytics;
    private bool allUIHiddenOriginalValue;


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
        controller = new ProfileHUDController(userProfileBridge, socialAnalytics, Substitute.For<DataStore>());
        baseView = controller.view.GameObject.GetComponent<BaseComponentView>();
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        Object.Destroy(userProfile);
        controller.Dispose();
        baseView.Dispose();
        CommonScriptableObjects.allUIHidden.Set(allUIHiddenOriginalValue);
        yield return base.TearDown();
    }

    [Test]
    public void Creation()
    {
        Assert.NotNull(controller.view);
        Assert.NotNull(baseView);
        Assert.IsTrue(controller.view.HasManaCounterView());
    }
}
