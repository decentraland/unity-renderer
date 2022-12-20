using DCL;
using DCL.Helpers;
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
        private CatalogController catalogController;
        private AvatarEditorHUDController_Mock controller;
        private BaseDictionary<string, WearableItem> catalog;
        private IAnalytics analytics;

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
            Object.Destroy(catalogController.gameObject);
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
            catalogController = TestUtils.CreateComponentWithGameObject<CatalogController>("CatalogController");
            catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            controller = new AvatarEditorHUDController_Mock(DataStore.i.featureFlags, analytics);
            controller.collectionsAlreadyLoaded = true;
            controller.Initialize(userProfile, catalog);
        }

        private static (string, string)[] compatibleWearableShapePairs =
        {
            ("urn:decentraland:off-chain:base-avatars:f_african_leggins", WearableLiterals.BodyShapes.FEMALE),
            ("urn:decentraland:off-chain:base-avatars:eyebrows_02", WearableLiterals.BodyShapes.MALE),
        };

        [UnityTest]
        public IEnumerator Activate_CompatibleWithBodyShape_ItemToggle([ValueSource(nameof(compatibleWearableShapePairs))] (string, string) wearableShapePair)
        {
            string wearableId = wearableShapePair.Item1;
            string bodyShape = wearableShapePair.Item2;

            userProfile.UpdateData(new UserProfileModel
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel
                {
                    bodyShape = bodyShape,
                    wearables = new List<string>(),
                },
            });

            string category = catalog.Get(wearableId).data.category;

            Assert.IsTrue(controller.myView.selectorsByCategory.ContainsKey(category));
            var selector = controller.myView.selectorsByCategory[category];

            yield return null;

            Assert.IsTrue(selector.currentItemToggles.ContainsKey(wearableId));
            var itemToggle = selector.currentItemToggles[wearableId];
            Assert.NotNull(itemToggle.wearableItem);

            Assert.AreEqual(wearableId, itemToggle.wearableItem.id);
            Assert.IsTrue(itemToggle.gameObject.activeSelf);
        }

        private static (string, string)[] incompatibleWearableShapePairs =
        {
            ("urn:decentraland:off-chain:base-avatars:f_african_leggins", WearableLiterals.BodyShapes.MALE),
            ("urn:decentraland:off-chain:base-avatars:eyebrows_02", WearableLiterals.BodyShapes.FEMALE),
        };

        [UnityTest]
        public IEnumerator IncompatibleWithBodyShape_ItemToggle([ValueSource(nameof(incompatibleWearableShapePairs))] (string, string) wearableShapePair)
        {
            string wearableId = wearableShapePair.Item1;
            string bodyShape = wearableShapePair.Item2;

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

            var category = catalog.Get(wearableId).data.category;

            Assert.IsTrue(controller.myView.selectorsByCategory.ContainsKey(category));
            var selector = controller.myView.selectorsByCategory[category];
            yield return null;

            Assert.IsTrue(selector.currentItemToggles.ContainsKey(wearableId));
            Assert.IsTrue(selector.totalWearables.ContainsKey(wearableId));
        }

        private static string[] wearables =
        {
            "urn:decentraland:off-chain:base-avatars:f_mouth_00",
            "urn:decentraland:off-chain:base-avatars:bee_t_shirt",
            "urn:decentraland:off-chain:base-avatars:m_mountainshoes.glb",
            "urn:decentraland:off-chain:base-avatars:moptop"
        };

        [UnityTest]
        public IEnumerator NotAdd_BaseWearables_ToCollectibles([ValueSource(nameof(wearables))] string wearableId)
        {
            yield return null;
            Assert.IsFalse(controller.myView.collectiblesItemSelector.currentItemToggles.ContainsKey(wearableId));
        }

        [Test]
        [TestCase("urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body")]
        public void Add_Exclusives_ToCollectibles(string wearableId)
        {
            userProfile.SetInventory(new[] { wearableId });

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

            // changed from itemToggles from totalWearables since this wearable cant be used by the test avatar ( female )
            Assert.IsTrue(controller.myView.collectiblesItemSelector.totalWearables.ContainsKey(wearableId));
        }

        private static string[] rarities =
        {
            WearableLiterals.ItemRarity.RARE,
            WearableLiterals.ItemRarity.EPIC,
            WearableLiterals.ItemRarity.LEGENDARY,
            WearableLiterals.ItemRarity.MYTHIC,
            WearableLiterals.ItemRarity.UNIQUE,
        };

        [UnityTest]
        public IEnumerator CreateNFTsButtonsByRarityCorrectly([ValueSource(nameof(rarities))] string rarity)
        {
            WearableItem dummyItem = CreateDummyNFT(rarity);

            var selector = controller.myView.selectorsByCategory[dummyItem.data.category];
            var itemToggleObject = selector.currentItemToggles[dummyItem.id].gameObject;

            var originalName = ""; //selector.itemToggleFactory.nftDictionary[rarity].prefab.name;

            yield return null;

            Assert.IsTrue(
                itemToggleObject.name
                                .Contains(originalName)); //Comparing names because PrefabUtility.GetOutermostPrefabInstanceRoot(itemToggleObject) is returning null
        }

        [UnityTest]
        public IEnumerator FillNFTInfoPanelCorrectly()
        {
            WearableItem dummyItem = CreateDummyNFT(WearableLiterals.ItemRarity.EPIC);

            var itemToggle = controller.myView.selectorsByCategory[dummyItem.data.category].currentItemToggles[dummyItem.id];
            var nftInfo = (itemToggle as NFTItemToggle)?.nftItemInfo;

            yield return null;

            Assert.NotNull(nftInfo);
            Assert.AreEqual(dummyItem.GetName(), nftInfo.name.text);
            Assert.AreEqual(dummyItem.description, nftInfo.description.text);

            Assert.AreEqual($"{dummyItem.issuedId} / {dummyItem.GetIssuedCountFromRarity(dummyItem.rarity)}",
                nftInfo.minted.text);
        }

        [UnityTest]
        public IEnumerator NotShowAmmountIfOnlyOneItemIsPossesed()
        {
            var wearableId = "urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body";
            userProfile.SetInventory(new[] { wearableId });

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

            yield return null;

            Assert.IsFalse(controller.myView.collectiblesItemSelector.currentItemToggles[wearableId]
                                     .amountContainer.gameObject
                                     .activeSelf);
        }

        private static int[] amounts = { 2, 3, 5, 10 };

        [UnityTest]
        public IEnumerator ShowAndUpdateAmount([ValueSource(nameof(amounts))] int amount)
        {
            var wearableId = "urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body";
            userProfile.SetInventory(Enumerable.Repeat(wearableId, amount).ToArray());

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
            yield return null;

            Assert.IsTrue(itemToggle.amountContainer.gameObject.activeSelf);
            Assert.AreEqual($"x{amount}", itemToggle.amountText.text);
        }

        [UnityTest]
        public IEnumerator ShowAndUpdateAmountInCollectibleTab([ValueSource(nameof(amounts))] int amount)
        {
            var wearableId = "urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body";
            userProfile.SetInventory(Enumerable.Repeat(wearableId, amount).ToArray());

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

            yield return null;

            var itemToggle = controller.myView.collectiblesItemSelector.currentItemToggles[wearableId];

            Assert.IsTrue(itemToggle.amountContainer.gameObject.activeSelf);
            Assert.AreEqual($"x{amount}", itemToggle.amountText.text);
        }

        [UnityTest]
        public IEnumerator MarkNFTAsSmart()
        {
            var smartNft = CreateSmartNFT();

            var selector = controller.myView.selectorsByCategory[smartNft.data.category];
            var itemToggleObject = (NFTItemToggle)selector.currentItemToggles[smartNft.id];

            yield return null;
            Assert.IsTrue(itemToggleObject.smartItemBadge.activeSelf);
        }

        [UnityTest]
        public IEnumerator HideSmartIconWhenIsNormalNFT()
        {
            var smartNft = CreateDummyNFT("rare");

            var selector = controller.myView.selectorsByCategory[smartNft.data.category];
            var itemToggleObject = (NFTItemToggle)selector.currentItemToggles[smartNft.id];

            yield return null;
            Assert.IsFalse(itemToggleObject.smartItemBadge.activeSelf);
        }

        [UnityTest]
        public IEnumerator ShowWarningWhenNoLinkedWearableAvailable()
        {
            controller.ToggleThirdPartyCollection(true, "MOCK_COLLECTION_ID", "MOCK_COLLECTION_NAME");
            yield return null;
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
                    tags = new[] { WearableLiterals.Tags.EXCLUSIVE },
                    representations = new[]
                    {
                        new WearableItem.Representation()
                        {
                            bodyShapes = new[] { WearableLiterals.BodyShapes.FEMALE, WearableLiterals.BodyShapes.MALE },
                        }
                    }
                },
                i18n = new[] { new i18n() { code = "en", text = "Dummy Item" } }
            };

            userProfile.SetInventory(new[] { dummyItem.id });

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

            catalog.Remove(dummyItem.id);
            catalog.Add(dummyItem.id, dummyItem);
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
                    tags = new[] { WearableLiterals.Tags.EXCLUSIVE },
                    representations = new[]
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new[] { WearableLiterals.BodyShapes.FEMALE, WearableLiterals.BodyShapes.MALE },
                            contents = new[] { new WearableItem.MappingPair { key = "game.js", hash = "hash" } }
                        }
                    }
                },
                i18n = new[] { new i18n { code = "en", text = "Smart Item" } }
            };

            userProfile.SetInventory(new[] { dummyItem.id });

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

            catalog.Remove(dummyItem.id);
            catalog.Add(dummyItem.id, dummyItem);
            return dummyItem;
        }
    }
}
