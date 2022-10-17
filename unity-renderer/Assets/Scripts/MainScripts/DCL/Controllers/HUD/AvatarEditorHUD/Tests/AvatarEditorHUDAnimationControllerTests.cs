using System;
using System.Collections;
using System.Collections.Generic;
using AvatarEditorHUD_Tests;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.Core.Arguments;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class AvatarEditorHUDAnimationControllerTests
{

    private IAvatarEditorHUDView editorHUDView;
    private AvatarEditorHUDAnimationController avatarEditorHUDAnimationController;
    private ICharacterPreviewController characterPreviewController;
    private CatalogController catalogController;
    private BaseDictionary<string, WearableItem> catalog;

    
    [SetUp]
    public void SetUp()
    {
        editorHUDView = Substitute.For<IAvatarEditorHUDView>();
        characterPreviewController = Substitute.For<ICharacterPreviewController>();
        catalogController = TestUtils.CreateComponentWithGameObject<CatalogController>("CatalogController");
        catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        avatarEditorHUDAnimationController = new AvatarEditorHUDAnimationController(editorHUDView, characterPreviewController);
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
        editorHUDView.Dispose();
    }

    
}
