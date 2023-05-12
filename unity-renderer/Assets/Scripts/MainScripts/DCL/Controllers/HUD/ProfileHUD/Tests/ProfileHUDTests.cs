using DCL;
using NUnit.Framework;
using System.Collections;
using NSubstitute;
using SocialFeaturesAnalytics;
using UnityEngine.TestTools;

public class ProfileHUDTests : IntegrationTestSuite_Legacy
{
    private ProfileHUDController controller;
    private BaseComponentView baseView;
    private IUserProfileBridge userProfileBridge;
    private ISocialAnalytics socialAnalytics;
    private bool allUIHiddenOriginalValue;


    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        allUIHiddenOriginalValue = CommonScriptableObjects.allUIHidden.Get();
        CommonScriptableObjects.allUIHidden.Set(false);
        controller = new ProfileHUDController(userProfileBridge, socialAnalytics, Substitute.For<DataStore>());
        baseView = controller.view.GameObject.GetComponent<BaseComponentView>();
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
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
