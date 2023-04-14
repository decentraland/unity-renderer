using DCL;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarEditorHUD_Tests
{
    public class AvatarEditorHUDViewShould : IntegrationTestSuite_Legacy
    {
        private UserProfile userProfile;
        private AvatarEditorHUDController_Mock controller;
        private IAnalytics analytics;
        private IWearablesCatalogService wearablesCatalogService;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            Setup_AvatarEditorHUDController();

            controller.collectionsAlreadyLoaded = true;
            controller.avatarIsDirty = false;
            controller.UnequipAllWearables();
            controller.SetVisibility(true);
        }

        protected override IEnumerator TearDown()
        {
            wearablesCatalogService.Dispose();
            controller.Dispose();
            yield return base.TearDown();
        }

        private void Setup_AvatarEditorHUDController()
        {
            userProfile = ScriptableObject.CreateInstance<UserProfile>();
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

            analytics = Substitute.For<IAnalytics>();
            wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();

            IUserProfileBridge userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.GetOwn().Returns(userProfile);

            controller = new AvatarEditorHUDController_Mock(DataStore.i.featureFlags, analytics, wearablesCatalogService,
                userProfileBridge);
            controller.collectionsAlreadyLoaded = true;
            controller.Initialize();
        }

        [Test]
        [TestCase("urn:decentraland:off-chain:base-avatars:f_african_leggins", WearableLiterals.BodyShapes.FEMALE)]
        [TestCase("urn:decentraland:off-chain:base-avatars:eyebrows_02", WearableLiterals.BodyShapes.MALE)]
        public void Activate_CompatibleWithBodyShape_ItemToggle(string wearableId, string bodyShape)
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = bodyShape,
                    wearables = new List<string>() { },
                }
            });
            var category = wearablesCatalogService.WearablesCatalog.Get(wearableId).data.category;

            Assert.IsTrue(controller.myView.selectorsByCategory.ContainsKey(category));
            var selector = controller.myView.selectorsByCategory[category];

            Assert.IsTrue(selector.currentItemToggles.ContainsKey(wearableId));
            var itemToggle = selector.currentItemToggles[wearableId];
            Assert.NotNull(itemToggle.wearableItem);

            Assert.AreEqual(wearableId, itemToggle.wearableItem.id);
            Assert.IsTrue(itemToggle.gameObject.activeSelf);
        }

        [Test]
        [TestCase("urn:decentraland:off-chain:base-avatars:f_african_leggins", WearableLiterals.BodyShapes.MALE)]
        [TestCase("urn:decentraland:off-chain:base-avatars:eyebrows_02", WearableLiterals.BodyShapes.FEMALE)]
        public void IncompatibleWithBodyShape_ItemToggle(string wearableId, string bodyShape)
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = bodyShape,
                    wearables = new List<string>() { },
                }
            });

            var category = wearablesCatalogService.WearablesCatalog.Get(wearableId).data.category;

            Assert.IsTrue(controller.myView.selectorsByCategory.ContainsKey(category));
            var selector = controller.myView.selectorsByCategory[category];

            Assert.IsTrue(selector.currentItemToggles.ContainsKey(wearableId));
            Assert.IsTrue(selector.totalWearables.ContainsKey(wearableId));
        }

        [Test]
        [TestCase("urn:decentraland:off-chain:base-avatars:f_mouth_00")]
        [TestCase("urn:decentraland:off-chain:base-avatars:bee_t_shirt")]
        [TestCase("urn:decentraland:off-chain:base-avatars:m_mountainshoes.glb")]
        [TestCase("urn:decentraland:off-chain:base-avatars:moptop")]
        public void NotAdd_BaseWearables_ToCollectibles(string wearableId)
        {
            Assert.IsFalse(controller.myView.collectiblesItemSelector.currentItemToggles.ContainsKey(wearableId));
        }

        [Test]
        [TestCase("urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body")]
        public void Add_Exclusives_ToCollectibles(string wearableId)
        {
            userProfile.SetInventory(new[] {wearableId});
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "email",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                }
            });

            // changed from itemToggles from totalWearables since this wearable cant be used by the test avatar ( female )
            Assert.IsTrue(controller.myView.collectiblesItemSelector.totalWearables.ContainsKey(wearableId));
        }

        [Test]
        [TestCase(WearableLiterals.ItemRarity.RARE)]
        [TestCase(WearableLiterals.ItemRarity.EPIC)]
        [TestCase(WearableLiterals.ItemRarity.LEGENDARY)]
        [TestCase(WearableLiterals.ItemRarity.MYTHIC)]
        [TestCase(WearableLiterals.ItemRarity.UNIQUE)]
        public void CreateNFTsButtonsByRarityCorrectly(string rarity)
        {
            WearableItem dummyItem = CreateDummyNFT(rarity);

            var selector = controller.myView.selectorsByCategory[dummyItem.data.category];
            var itemToggleObject = selector.currentItemToggles[dummyItem.id].gameObject;

            var originalName = "";//selector.itemToggleFactory.nftDictionary[rarity].prefab.name;

            Assert.IsTrue(
                itemToggleObject.name
                    .Contains(originalName)); //Comparing names because PrefabUtility.GetOutermostPrefabInstanceRoot(itemToggleObject) is returning null
        }

        [Test]
        public void FillNFTInfoPanelCorrectly()
        {
            WearableItem dummyItem = CreateDummyNFT(WearableLiterals.ItemRarity.EPIC);

            var itemToggle = controller.myView.selectorsByCategory[dummyItem.data.category].currentItemToggles[dummyItem.id];
            var nftInfo = (itemToggle as NFTItemToggle)?.nftItemInfo;

            Assert.NotNull(nftInfo);
            Assert.AreEqual(dummyItem.GetName(), nftInfo.name.text);
            Assert.AreEqual(dummyItem.description, nftInfo.description.text);
            Assert.AreEqual($"{dummyItem.issuedId} / {dummyItem.GetIssuedCountFromRarity(dummyItem.rarity)}",
                nftInfo.minted.text);
        }

        [Test]
        public void NotShowAmmountIfOnlyOneItemIsPossesed()
        {
            var wearableId = "urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body";
            userProfile.SetInventory(new[] {wearableId});
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.MALE,
                    wearables = new List<string>() { },
                }
            });

            Assert.IsFalse(controller.myView.collectiblesItemSelector.currentItemToggles[wearableId].amountContainer.gameObject
                .activeSelf);
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        public void ShowAndUpdateAmount(int amount)
        {
            var wearableId = "urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body";
            userProfile.SetInventory(Enumerable.Repeat(wearableId, amount));
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.MALE,
                    wearables = new List<string>() { },
                }
            });

            var itemToggle = controller.myView.selectorsByCategory[WearableLiterals.Categories.UPPER_BODY]
                .currentItemToggles[wearableId];

            Assert.IsTrue(itemToggle.amountContainer.gameObject.activeSelf);
            Assert.AreEqual($"x{amount}", itemToggle.amountText.text);
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        public void ShowAndUpdateAmountInCollectibleTab(int amount)
        {
            var wearableId = "urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body";
            userProfile.SetInventory(Enumerable.Repeat(wearableId, amount));
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.MALE,
                    wearables = new List<string>() { },
                }
            });

            var itemToggle = controller.myView.collectiblesItemSelector.currentItemToggles[wearableId];

            Assert.IsTrue(itemToggle.amountContainer.gameObject.activeSelf);
            Assert.AreEqual($"x{amount}", itemToggle.amountText.text);
        }

        [Test]
        public void MarkNFTAsSmart()
        {
            var smartNft = CreateSmartNFT();

            var selector = controller.myView.selectorsByCategory[smartNft.data.category];
            var itemToggleObject = (NFTItemToggle) selector.currentItemToggles[smartNft.id];

            Assert.IsTrue( itemToggleObject.smartItemBadge.activeSelf);
        }

        [Test]
        public void HideSmartIconWhenIsNormalNFT()
        {
            var smartNft = CreateDummyNFT("rare");

            var selector = controller.myView.selectorsByCategory[smartNft.data.category];
            var itemToggleObject = (NFTItemToggle) selector.currentItemToggles[smartNft.id];

            Assert.IsFalse( itemToggleObject.smartItemBadge.activeSelf);
        }

        [Test]
        public void ShowWarningWhenNoLinkedWearableAvailable()
        {
            controller.ToggleThirdPartyCollection(true, "MOCK_COLLECTION_ID", "MOCK_COLLECTION_NAME");
            Assert.True(controller.view.noItemInCollectionWarning.isActiveAndEnabled);
        }

        private WearableItem CreateDummyNFT(string rarity)
        {
            var dummyItem = new WearableItem()
            {
                id = "dummyItem",
                rarity = rarity,
                description = "My Description",
                issuedId = 1,
                data = new WearableItem.Data()
                {
                    category = WearableLiterals.Categories.EYES,
                    tags = new[] {WearableLiterals.Tags.EXCLUSIVE},
                    representations = new[]
                    {
                        new WearableItem.Representation()
                        {
                            bodyShapes = new[] {WearableLiterals.BodyShapes.FEMALE, WearableLiterals.BodyShapes.MALE},
                        }
                    }
                },
                i18n = new[] {new i18n() {code = "en", text = "Dummy Item"}}
            };

            userProfile.SetInventory(new[] {dummyItem.id});
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

            wearablesCatalogService.WearablesCatalog.Remove(dummyItem.id);
            wearablesCatalogService.WearablesCatalog.Add(dummyItem.id, dummyItem);
            return dummyItem;
        }

        private WearableItem CreateSmartNFT()
        {
            var dummyItem = new WearableItem
            {
                id = "smartItem",
                rarity = "epic",
                description = "My Description",
                issuedId = 1,
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.EYES,
                    tags = new[] {WearableLiterals.Tags.EXCLUSIVE},
                    representations = new[]
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new[] {WearableLiterals.BodyShapes.FEMALE, WearableLiterals.BodyShapes.MALE},
                            contents = new[] {new WearableItem.MappingPair {key = "game.js", hash = "hash"}}
                        }
                    }
                },
                i18n = new[] {new i18n {code = "en", text = "Smart Item"}}
            };

            userProfile.SetInventory(new[] {dummyItem.id});
            userProfile.UpdateData(new UserProfileModel
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>()
                }
            });

            wearablesCatalogService.WearablesCatalog.Remove(dummyItem.id);
            wearablesCatalogService.WearablesCatalog.Add(dummyItem.id, dummyItem);
            return dummyItem;
        }
    }
}
