using AvatarShape_Tests;
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
        private BaseDictionary<string, WearableItem> catalog;

        protected override bool justSceneSetUp => true;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            yield return base.SetUp_CharacterController();

            Setup_AvatarEditorHUDController();

            controller.UnequipAllWearables();
            controller.SetVisibility(true);
        }

        protected override IEnumerator TearDown()
        {
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
            }, false);

            catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            controller = new AvatarEditorHUDController_Mock();
            controller.Initialize(userProfile, catalog);
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
            }, false);
            var category = catalog.Get(wearableId).data.category;

            Assert.IsTrue(controller.myView.selectorsByCategory.ContainsKey(category));
            var selector = controller.myView.selectorsByCategory[category];

            Assert.IsTrue(selector.itemToggles.ContainsKey(wearableId));
            var itemToggle = selector.itemToggles[wearableId];
            Assert.NotNull(itemToggle.wearableItem);

            Assert.AreEqual(wearableId, itemToggle.wearableItem.id);
            Assert.IsTrue(itemToggle.gameObject.activeSelf);
        }

        [Test]
        [TestCase("urn:decentraland:off-chain:base-avatars:f_african_leggins", WearableLiterals.BodyShapes.MALE)]
        [TestCase("urn:decentraland:off-chain:base-avatars:eyebrows_02", WearableLiterals.BodyShapes.FEMALE)]
        public void NotCreate_IncompatibleWithBodyShape_ItemToggle(string wearableId, string bodyShape)
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
            }, false);

            var category = catalog.Get(wearableId).data.category;

            Assert.IsTrue(controller.myView.selectorsByCategory.ContainsKey(category));
            var selector = controller.myView.selectorsByCategory[category];

            Assert.IsTrue(selector.itemToggles.ContainsKey(wearableId));
            var itemToggle = selector.itemToggles[wearableId];
            Assert.NotNull(itemToggle.wearableItem);

            Assert.AreEqual(wearableId, itemToggle.wearableItem.id);
            Assert.IsFalse(itemToggle.gameObject.activeSelf);
        }

        [Test]
        [TestCase("urn:decentraland:off-chain:base-avatars:f_mouth_00")]
        [TestCase("urn:decentraland:off-chain:base-avatars:bee_t_shirt")]
        [TestCase("urn:decentraland:off-chain:base-avatars:m_mountainshoes.glb")]
        [TestCase("urn:decentraland:off-chain:base-avatars:moptop")]
        public void NotAdd_BaseWearables_ToCollectibles(string wearableId) { Assert.IsFalse(controller.myView.collectiblesItemSelector.itemToggles.ContainsKey(wearableId)); }

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
            }, false);

            Assert.IsTrue(controller.myView.collectiblesItemSelector.itemToggles.ContainsKey(wearableId));
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
            var itemToggleObject = selector.itemToggles[dummyItem.id].gameObject;

            var originalName = selector.itemToggleFactory.nftDictionary[rarity].prefab.name;

            Assert.IsTrue(itemToggleObject.name.Contains(originalName)); //Comparing names because PrefabUtility.GetOutermostPrefabInstanceRoot(itemToggleObject) is returning null
        }

        [Test]
        public void FillNFTInfoPanelCorrectly()
        {
            WearableItem dummyItem = CreateDummyNFT(WearableLiterals.ItemRarity.EPIC);

            var itemToggle = controller.myView.selectorsByCategory[dummyItem.data.category].itemToggles[dummyItem.id];
            var nftInfo = (itemToggle as NFTItemToggle)?.nftItemInfo;

            Assert.NotNull(nftInfo);
            Assert.AreEqual(dummyItem.GetName(), nftInfo.name.text);
            Assert.AreEqual(dummyItem.description, nftInfo.description.text);
            Assert.AreEqual($"{dummyItem.issuedId} / {dummyItem.GetIssuedCountFromRarity(dummyItem.rarity)}", nftInfo.minted.text);
        }

        [Test]
        public void NotShowAmmountIfOnlyOneItemIsPossesed()
        {
            var wearableId = "urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body";
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
            }, false);

            Assert.IsFalse(controller.myView.collectiblesItemSelector.itemToggles[wearableId].amountContainer.gameObject.activeSelf);
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        public void ShowAndUpdateAmount(int amount)
        {
            var wearableId = "urn:decentraland:off-chain:halloween_2019:sad_clown_upper_body";
            userProfile.SetInventory(Enumerable.Repeat(wearableId, amount).ToArray());
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                }
            }, false);

            var itemToggle = controller.myView.selectorsByCategory[WearableLiterals.Categories.UPPER_BODY].itemToggles[wearableId];

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
            userProfile.SetInventory(Enumerable.Repeat(wearableId, amount).ToArray());
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                }
            }, false);

            var itemToggle = controller.myView.collectiblesItemSelector.itemToggles[wearableId];

            Assert.IsTrue(itemToggle.amountContainer.gameObject.activeSelf);
            Assert.AreEqual($"x{amount}", itemToggle.amountText.text);
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
            }, false);

            catalog.Remove(dummyItem.id);
            catalog.Add(dummyItem.id, dummyItem);
            return dummyItem;
        }
    }
}