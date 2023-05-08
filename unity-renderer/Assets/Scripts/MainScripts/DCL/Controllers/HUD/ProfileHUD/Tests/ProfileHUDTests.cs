using DCL;
using NUnit.Framework;
using System.Collections;
using NSubstitute;
using SocialFeaturesAnalytics;
using UnityEditor;
using UnityEngine;
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

        var view = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<ProfileHUDViewV2>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/ProfileHUD/Prefabs/ProfileHUD.prefab"));

        controller = new ProfileHUDController(view, userProfileBridge, socialAnalytics, Substitute.For<DataStore>());
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
