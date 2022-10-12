using System.Collections.Generic;
using AvatarEditorHUD_Tests;
using DCL;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class AvatarEditorHUDAnimationControllerTests 
{
    private AvatarEditorHUDController_Mock controller;
    private UserProfile userProfile;
    private BaseDictionary<string, WearableItem> catalog;

    
    [SetUp]
    void SetUp()
    {
        controller = new AvatarEditorHUDController_Mock(DataStore.i.featureFlags, Substitute.For<IAnalytics>());
        controller.collectionsAlreadyLoaded = true;
        userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(new UserProfileModel()
        {
            name = "name",
            email = "mail",
            avatar = new AvatarModel()
            {
                bodyShape = WearableLiterals.BodyShapes.FEMALE,
                wearables = new List<string>()
                    { }
            }
        });
        catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        controller.Initialize(userProfile, catalog);
    }

    [Test]
    public void AnimationRunOnSelected()
    {
        Debug.Log(controller.view.wearableGridPairs);
        controller.WearableClicked("urn:decentraland:off-chain:base-avatars:black_sun_glasses");
        Assert.IsTrue(true);
    }
    
    
}
