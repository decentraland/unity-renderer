using System;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;

public class AvatarEditorHUDAnimationControllerTests
{

    private IAvatarEditorHUDView editorHUDView;
    private AvatarEditorHUDAnimationController avatarEditorHUDAnimationController;
    private ICharacterPreviewController characterPreviewController;
    private IWearablesCatalogService wearablesCatalogService;

    [SetUp]
    public void SetUp()
    {
        wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        editorHUDView = Substitute.For<IAvatarEditorHUDView>();
        characterPreviewController = Substitute.For<ICharacterPreviewController>();
        editorHUDView.CharacterPreview.Returns(characterPreviewController);
        avatarEditorHUDAnimationController = new AvatarEditorHUDAnimationController(editorHUDView, wearablesCatalogService);
    }

    [Test]
    [TestCase("Outfit_Accessories_v0", "urn:decentraland:off-chain:base-avatars:blue_bandana")]
    [TestCase("Outfit_Accessories_v0", "urn:decentraland:off-chain:base-avatars:black_sun_glasses")]
    [TestCase("Outfit_Shoes_v0", "urn:decentraland:off-chain:base-avatars:sneakers")]
    [TestCase("Outfit_Lower_v0", "urn:decentraland:off-chain:base-avatars:f_african_leggins")]
    [TestCase("Outfit_Upper_v0", "urn:decentraland:off-chain:base-avatars:green_hoodie")]

    public void ActiveCategoryHasCorrectValueOnSelected(string expectedAnimationStart, string selectedWearable)
    {
        editorHUDView.WearableSelectorClicked += Raise.Event<Action<string>>(selectedWearable);
        Assert.True(avatarEditorHUDAnimationController.activeCategory.StartsWith(expectedAnimationStart));
    }

    [Test]
    [TestCase("urn:decentraland:off-chain:base-avatars:BaseFemale")]
    [TestCase( "urn:decentraland:off-chain:base-avatars:eyes_00")]
    public void ActiveCategoryIsEmptyOnSelected(string selectedWearable)
    {
        editorHUDView.WearableSelectorClicked += Raise.Event<Action<string>>(selectedWearable);
        Assert.True(string.IsNullOrEmpty(avatarEditorHUDAnimationController.activeCategory));
    }

    [Test]
    public void ActiveCategoryIsEmptyOnRandomize()
    {
        editorHUDView.OnRandomize += Raise.Event<Action>();
        Assert.True(string.IsNullOrEmpty(avatarEditorHUDAnimationController.activeCategory));
    }

    [Test]
    public void AnimationRunOnSelected()
    {
        editorHUDView.WearableSelectorClicked += Raise.Event<Action<string>>("urn:decentraland:off-chain:base-avatars:blue_bandana");
        editorHUDView.OnAvatarAppearFeedback += Raise.Event<Action<AvatarModel>>(new AvatarModel());
        characterPreviewController.Received().PlayEmote(Arg.Any<string>(), Arg.Any<long>());
    }

    [TearDown]
    public void TearDown()
    {
        wearablesCatalogService.Dispose();
        editorHUDView.Dispose();
    }
}
