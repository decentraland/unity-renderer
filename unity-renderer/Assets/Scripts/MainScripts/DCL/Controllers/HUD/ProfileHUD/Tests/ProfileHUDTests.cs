using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class ProfileHUDTests : TestsBase
{
    protected override bool justSceneSetUp => true;
    ProfileHUDController controller;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = new ProfileHUDController();
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void Creation()
    {
        Assert.NotNull(controller.view);
    }

    [Test]
    public void VisibilityDefaulted()
    {
        Assert.AreEqual(true, controller.view.gameObject.activeInHierarchy);
        Assert.AreEqual(false, controller.view.menuShowHideAnimator.isVisible);
    }

    [Test]
    public void VisibilityOverridenTrue()
    {
        controller.SetVisibility(true);
        Assert.AreEqual(true, controller.view.gameObject.activeInHierarchy);
        Assert.AreEqual(false, controller.view.menuShowHideAnimator.isVisible);
    }

    [Test]
    public void VisibilityOverridenFalse()
    {
        controller.SetVisibility(false);
        Assert.AreEqual(false, controller.view.menuShowHideAnimator.isVisible);
    }

    [Test]
    public void ContentVisibilityOnToggle()
    {
        Assert.AreEqual(false, controller.view.menuShowHideAnimator.isVisible);
        controller.view.ToggleMenu();
        Assert.AreEqual(true, controller.view.menuShowHideAnimator.isVisible);
        controller.view.ToggleMenu();
        Assert.AreEqual(false, controller.view.menuShowHideAnimator.isVisible);
    }

    [Test]
    public void UpdateProfileCorrectly()
    {
        const string address = "0x12345678901234567890";
        const string addressEnd = "7890";
        const string addressFormatted = "0x1234...567890";

        UserProfileModel profileModel = new UserProfileModel()
        {
            name = "PraBian",
            userId = address,
            hasClaimedName = true
        };
        UserProfile profile = UserProfile.GetOwnUserProfile();
        profile.UpdateData(profileModel,false);

        for (int i = 0; i < controller.view.hideOnNameClaimed.Length; i++)
        {
            Assert.AreEqual(false, controller.view.hideOnNameClaimed[i].gameObject.activeSelf);
        }
        Assert.AreEqual(profileModel.name, controller.view.textName.text);

        profileModel.hasClaimedName = false;
        profile.UpdateData(profileModel,true);

        for (int i = 0; i < controller.view.hideOnNameClaimed.Length; i++)
        {
            Assert.AreEqual(true, controller.view.hideOnNameClaimed[i].gameObject.activeSelf);
        }
        Assert.AreEqual(profileModel.name, controller.view.textName.text);
        Assert.AreEqual($".{addressEnd}", controller.view.textPostfix.text);
        Assert.AreEqual(addressFormatted, controller.view.textAddress.text);

    }
}