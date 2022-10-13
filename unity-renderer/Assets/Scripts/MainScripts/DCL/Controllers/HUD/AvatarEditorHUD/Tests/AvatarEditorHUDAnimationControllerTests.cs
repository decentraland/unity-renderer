using System.Collections;
using System.Collections.Generic;
using AvatarEditorHUD_Tests;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AvatarEditorHUDAnimationControllerTests : IntegrationTestSuite_Legacy
{
    private AvatarEditorHUDController_Mock controller;
    private UserProfile userProfile;
    private BaseDictionary<string, WearableItem> catalog;
    private CatalogController catalogController;
    private ColorList skinColorList;
    private ColorList hairColorList;
    private ColorList eyeColorList;

    
    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        
        if (controller == null)
        {
            skinColorList = Resources.Load<ColorList>("SkinTone");
            hairColorList = Resources.Load<ColorList>("HairColor");
            eyeColorList = Resources.Load<ColorList>("EyeColor");

            userProfile = ScriptableObject.CreateInstance<UserProfile>();
        }

        IAnalytics analytics = Substitute.For<IAnalytics>();
        catalogController = TestUtils.CreateComponentWithGameObject<CatalogController>("CatalogController");
        catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        controller = new AvatarEditorHUDController_Mock(DataStore.i.featureFlags, analytics);
        // TODO: We should convert the WearablesFetchingHelper static class into a non-static one and make it implement an interface. It would allow us to inject it
        //       into AvatarEditorHUDController and we would be able to replace the GetThirdPartyCollections() call by a mocked one in this test, allowing us to avoid
        //       the use of 'collectionsAlreadyLoaded = true'.
        controller.collectionsAlreadyLoaded = true;
        controller.Initialize(userProfile, catalog);
        controller.SetVisibility(true);
        DataStore.i.common.isPlayerRendererLoaded.Set(true);

        userProfile.UpdateData(new UserProfileModel()
        {
            name = "name",
            email = "mail",
            avatar = new AvatarModel()
            {
                bodyShape = WearableLiterals.BodyShapes.FEMALE,
                wearables = new List<string>() { },
            }
        });
            
        controller.avatarIsDirty = false;
    }

    [Test]
    public void AnimationRunOnSelected()
    {
        controller.view.wearableGridPairs[2].selector.ToggleClicked(controller.view.wearableGridPairs[2].selector.itemToggles["urn:decentraland:off-chain:base-avatars:green_hoodie"]);
        Assert.IsTrue(controller.avatarEditorHUDAnimationController.activeCategory.StartsWith("Outfit_Upper_v0"));
    }
    
    [TearDown]
    public void TearDown()
    {
        Object.Destroy(catalogController.gameObject);
        controller.Dispose();
    }

    
}
