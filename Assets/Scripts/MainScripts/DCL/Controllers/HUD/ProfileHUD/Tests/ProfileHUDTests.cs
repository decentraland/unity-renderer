using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine.TestTools;

public class ProfileHUDTests : IntegrationTestSuite_Legacy
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
        Assert.NotNull(controller.manaCounterView);
    }

    [Test]
    public void VisibilityDefaulted()
    {
        Assert.IsTrue(controller.view.gameObject.activeInHierarchy);
        Assert.IsFalse(controller.view.menuShowHideAnimator.isVisible);
        Assert.IsFalse(controller.manaCounterView.gameObject.activeInHierarchy);
    }

    [Test]
    public void VisibilityOverridenTrue()
    {
        controller.SetVisibility(true);
        Assert.IsTrue(controller.view.gameObject.activeInHierarchy);
        Assert.IsFalse(controller.view.menuShowHideAnimator.isVisible);
    }

    [Test]
    public void VisibilityOverridenFalse()
    {
        controller.SetVisibility(false);
        Assert.IsFalse(controller.view.menuShowHideAnimator.isVisible);
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
        const string testUserName = "PraBian";

        UserProfileModel profileModel = new UserProfileModel()
        {
            name = testUserName,
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

        profileModel.name += "#1234";
        profileModel.hasClaimedName = false;
        profile.UpdateData(profileModel,true);

        for (int i = 0; i < controller.view.hideOnNameClaimed.Length; i++)
        {
            Assert.AreEqual(true, controller.view.hideOnNameClaimed[i].gameObject.activeSelf);
        }
        Assert.AreEqual(testUserName, controller.view.textName.text);
        Assert.AreEqual($"#{addressEnd}", controller.view.textPostfix.text);
        Assert.AreEqual(addressFormatted, controller.view.textAddress.text);

    }

    [Test]
    public void SetManaBalanceCorrectly()
    {
        string balance = "5";

        controller.SetManaBalance(balance);
        Assert.AreEqual(Convert.ToDouble(balance), Convert.ToDouble(controller.manaCounterView.balanceText.text));
    }

    [Test]
    public void AddBackpackWindowCorrectly()
    {
        Assert.IsNull(controller.avatarEditorHud);
        Assert.IsFalse(controller.view.buttonBackpack.gameObject.activeSelf);

        AvatarEditorHUDController testAvatarEditor = new AvatarEditorHUDController();
        controller.AddBackpackWindow(testAvatarEditor);

        Assert.IsNotNull(controller.avatarEditorHud);
        Assert.IsTrue(controller.view.buttonBackpack.gameObject.activeSelf);
    }

    [Test]
    public void ShowAndHideBackpackButtonCorrectly()
    {
        AvatarEditorHUDController testAvatarEditor = new AvatarEditorHUDController();
        controller.AddBackpackWindow(testAvatarEditor);

        controller.SetBackpackButtonVisibility(true);
        Assert.IsTrue(controller.view.buttonBackpack.gameObject.activeSelf);

        controller.SetBackpackButtonVisibility(false);
        Assert.IsFalse(controller.view.buttonBackpack.gameObject.activeSelf);
    }

    [Test]
    public void ActivateAndDeactivateProfileNameEditionCorrectly()
    {
        controller.view.textName.text = "test name";

        controller.view.ActivateProfileNameEditionMode(true);
        Assert.IsFalse(controller.view.editNameTooltipGO.activeSelf);
        Assert.IsFalse(controller.view.textName.gameObject.activeSelf);
        Assert.IsTrue(controller.view.inputName.gameObject.activeSelf);
        Assert.IsTrue(controller.view.inputName.text == controller.view.textName.text);

        controller.view.ActivateProfileNameEditionMode(false);
        Assert.IsTrue(controller.view.editNameTooltipGO.activeSelf);
        Assert.IsTrue(controller.view.textName.gameObject.activeSelf);
        Assert.IsFalse(controller.view.inputName.gameObject.activeSelf);
    }

    [Test]
    public void UpdateCharactersLimitLabelCorrectly()
    {
        controller.view.inputName.characterLimit = 100;
        controller.view.inputName.text = "";
        Assert.IsTrue(controller.view.textCharLimit.text == $"{controller.view.inputName.text.Length}/{controller.view.inputName.characterLimit}");

        controller.view.inputName.characterLimit = 50;
        controller.view.inputName.text = "test name";
        Assert.IsTrue(controller.view.textCharLimit.text == $"{controller.view.inputName.text.Length}/{controller.view.inputName.characterLimit}");
    }

    [Test]
    public void SetProfileNameCorrectly()
    {
        string newName = "new test name";

        controller.view.textName.text = "test name";
        controller.view.SetProfileName(newName);
        Assert.IsTrue(controller.view.textName.text == newName);
    }
}