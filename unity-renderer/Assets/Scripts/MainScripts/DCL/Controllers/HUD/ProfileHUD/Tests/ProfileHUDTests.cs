using NUnit.Framework;
using System.Collections;
using NSubstitute;
using UnityEngine.TestTools;

public class ProfileHUDTests : IntegrationTestSuite_Legacy
{
    private ProfileHUDController controller;
    private BaseComponentView baseView;
    private IUserProfileBridge userProfileBridge;
    private bool allUIHiddenOriginalValue;


    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        allUIHiddenOriginalValue = CommonScriptableObjects.allUIHidden.Get();
        CommonScriptableObjects.allUIHidden.Set(false);
        controller = new ProfileHUDController(userProfileBridge);
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

    [Test]
    public void VisibilityDefaulted()
    {
        Assert.IsTrue(controller.view.GameObject.activeInHierarchy);
        Assert.IsTrue(baseView.showHideAnimator.isVisible);
    }

    [Test]
    public void VisibilityOverridenTrue()
    {
        controller.SetVisibility(true);
        Assert.IsTrue(controller.view.GameObject.activeInHierarchy);
        Assert.IsTrue(baseView.showHideAnimator.isVisible);
    }

    [Test]
    public void VisibilityOverridenFalse()
    {
        controller.SetVisibility(false);
        Assert.IsFalse(baseView.showHideAnimator.isVisible);
    }

    [Test]
    public void ContentVisibilityOnToggle()
    {
        Assert.IsTrue(baseView.showHideAnimator.isVisible);
        controller.view.ToggleMenu();
        Assert.IsFalse(baseView.showHideAnimator.isVisible);
        controller.view.ToggleMenu();
        Assert.IsTrue(baseView.showHideAnimator.isVisible);
    }
}
