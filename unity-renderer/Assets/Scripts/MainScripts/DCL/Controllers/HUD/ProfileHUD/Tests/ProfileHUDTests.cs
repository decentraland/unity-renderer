using NUnit.Framework;
using System.Collections;
using System.Globalization;
using NSubstitute;
using UnityEngine.TestTools;

public class ProfileHUDTests : IntegrationTestSuite_Legacy
{
    private ProfileHUDController controller;
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
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        CommonScriptableObjects.allUIHidden.Set(allUIHiddenOriginalValue);
        yield return base.TearDown();
    }

    [Test]
    public void Creation()
    {
        Assert.NotNull(controller.view);
        Assert.NotNull(controller.view.manaCounterView);
    }

    [Test]
    public void VisibilityDefaulted()
    {
        Assert.IsTrue(controller.view.gameObject.activeInHierarchy);
        Assert.IsFalse(controller.view.menuShowHideAnimator.isVisible);
        Assert.IsFalse(controller.view.manaCounterView.gameObject.activeInHierarchy);
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
        profile.UpdateData(profileModel);

        for (int i = 0; i < controller.view.hideOnNameClaimed.Length; i++)
        {
            Assert.AreEqual(false, controller.view.hideOnNameClaimed[i].gameObject.activeSelf);
        }

        Assert.AreEqual(profileModel.name, controller.view.textName.text);

        profileModel.name += "#1234";
        profileModel.hasClaimedName = false;
        profile.UpdateData(profileModel);

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
        string balance = "123456.123456";
        double.TryParse(balance, NumberStyles.Number, CultureInfo.InvariantCulture, out double manaBalance);
        string formattedManaBalance = (manaBalance / 1000D).ToString("0.#K");
        
        controller.SetManaBalance(balance);
        
        Assert.AreEqual(formattedManaBalance, controller.view.manaCounterView.balanceText.text);
    }

    [Test]
    public void ActivateAndDeactivateProfileNameEditionCorrectly()
    {
        controller.view.textName.text = "test name";

        controller.view.ActivateProfileNameEditionMode(true);
        Assert.IsFalse(controller.view.textName.gameObject.activeSelf);
        Assert.IsTrue(controller.view.inputName.gameObject.activeSelf);
        Assert.IsTrue(controller.view.inputName.text == controller.view.textName.text);

        controller.view.ActivateProfileNameEditionMode(false);
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

    [Test]
    public void UpdateProfileDescriptionTextComponent()
    {
        const string aboutMe = "i make pancakes";
        controller.view.SetDescription(aboutMe);
        Assert.IsTrue(controller.view.descriptionPreviewInput.text == aboutMe);
        Assert.IsTrue(controller.view.descriptionEditionInput.text == aboutMe);
    }

    [Test]
    public void UpdateProfileDescriptionIntoGatewayWhenSubmitInputField()
    {
        const string aboutMe = "i make pancakes";
        controller.view.ActivateDescriptionEditionMode(true);
        controller.view.descriptionEditionInput.text = aboutMe;
        controller.view.descriptionEditionInput.OnSubmit(null);
        userProfileBridge.Received(1).SaveDescription(aboutMe);
    }

    [Test]
    public void DoNotUpdateProfileDescriptionWhenIsTooLong()
    {
        const string aboutMe = "i make pancakes";
        controller.view.ActivateDescriptionEditionMode(true);
        controller.view.descriptionEditionInput.characterLimit = 5;
        controller.view.descriptionEditionInput.text = aboutMe;
        controller.view.descriptionEditionInput.OnSubmit(null);
        userProfileBridge.Received(0).SaveDescription(Arg.Any<string>());
    }

    [Test]
    public void DoNotUpdateProfileDescriptionWhenHasNotConnectedWallet()
    {
        var profileModel = new UserProfileModel
        {
            hasConnectedWeb3 = false,
            name = "user123",
            userId = "0x1234"
        };
        var profile = UserProfile.GetOwnUserProfile();
        profile.UpdateData(profileModel);

        const string aboutMe = "i make pancakes";
        controller.view.ActivateDescriptionEditionMode(true);
        controller.view.descriptionEditionInput.text = aboutMe;
        controller.view.descriptionEditionInput.OnSubmit(null);

        userProfileBridge.Received(0).SaveDescription(Arg.Any<string>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DescriptionInputChangesActiveDependingOfConnectedWallet(bool isWalletConnected)
    {
        var profileModel = new UserProfileModel
        {
            hasConnectedWeb3 = isWalletConnected,
            name = "user123",
            userId = "0x1234"
        };
        var profile = UserProfile.GetOwnUserProfile();
        profile.UpdateData(profileModel);

        Assert.AreEqual(controller.view.descriptionContainer.activeSelf, isWalletConnected);
    }

    
}