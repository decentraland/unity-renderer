using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Backpack
{
    public class AvatarSlotComponentViewShould
    {
        private const string TEST_CATEGORY = "mask";
        private const string TEST_RARITY = "Legendary";

        private AvatarSlotComponentView avatarSlot;
        private Texture2D testTexture;
        private Sprite testSprite;
        private NftTypeIconSO nftTypeIconMapping;
        private NftRarityBackgroundSO nftRarityBackgroundMapping;
        private NftTypeColorSupportingSO nftTypeColorSupporting;
        private NftTypePreviewCameraFocusConfig nftTypePreviewCameraFocus;

        [SetUp]
        public void SetUp()
        {
            testTexture = new Texture2D(20, 20);
            testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
            nftTypeIconMapping = ScriptableObject.CreateInstance<NftTypeIconSO>();
            nftRarityBackgroundMapping = ScriptableObject.CreateInstance<NftRarityBackgroundSO>();
            nftTypeColorSupporting = ScriptableObject.CreateInstance<NftTypeColorSupportingSO>();
            nftTypePreviewCameraFocus = ScriptableObject.CreateInstance<NftTypePreviewCameraFocusConfig>();

            nftTypeIconMapping.nftIcons = new SerializableKeyValuePair<string, Sprite>[1];
            nftTypeIconMapping.nftIcons[0] = new SerializableKeyValuePair<string, Sprite>
            {
                key = TEST_CATEGORY,
                value = testSprite
            };

            nftTypeColorSupporting.colorSupportingByNftType = new SerializableKeyValuePair<string, bool>[1];
            nftTypeColorSupporting.colorSupportingByNftType[0] = new SerializableKeyValuePair<string, bool>
            {
                key = TEST_CATEGORY,
                value = true
            };

            nftTypePreviewCameraFocus.previewCameraFocusByNftType = new NftTypePreviewCameraFocusConfig.NftTypePreviewCameraFocus[1];
            nftTypePreviewCameraFocus.previewCameraFocusByNftType[0] = new NftTypePreviewCameraFocusConfig.NftTypePreviewCameraFocus
            {
                nftType = TEST_CATEGORY,
                cameraFocus = PreviewCameraFocus.FaceEditing,
            };

            nftRarityBackgroundMapping.rarityIcons = new SerializableKeyValuePair<string, Sprite>[1];
            nftRarityBackgroundMapping.rarityIcons[0] = new SerializableKeyValuePair<string, Sprite>
            {
                key = TEST_RARITY,
                value = testSprite
            };

            AvatarSlotComponentView prefab = AssetDatabase.LoadAssetAtPath<AvatarSlotComponentView>(
                                                               "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Prefabs/AvatarSlot.prefab");

            avatarSlot = Object.Instantiate(prefab);
            avatarSlot.typeIcons = nftTypeIconMapping;
            avatarSlot.rarityBackgrounds = nftRarityBackgroundMapping;
            avatarSlot.typeColorSupporting = nftTypeColorSupporting;
            avatarSlot.previewCameraFocus = nftTypePreviewCameraFocus;
        }

        [TearDown]
        public void TearDown()
        {
            avatarSlot.Dispose();
            Object.Destroy(avatarSlot.gameObject);
            Object.Destroy(nftTypeIconMapping);
            Object.Destroy(nftRarityBackgroundMapping);
            Object.Destroy(nftTypeColorSupporting);
            Object.Destroy(nftTypePreviewCameraFocus);
            Object.Destroy(testTexture);
            Object.Destroy(testSprite);
        }

        [Test]
        public void SetCategory()
        {
            avatarSlot.SetCategory(TEST_CATEGORY);
            Assert.IsTrue(Equals(avatarSlot.typeImage.sprite, nftTypeIconMapping.GetTypeImage(TEST_CATEGORY)), "The icon obtained from the mapping differs from the set one.");
            Assert.IsTrue(nftTypeColorSupporting.IsColorSupportedByType(TEST_CATEGORY));

            var previewCamFocus = nftTypePreviewCameraFocus.GetPreviewCameraFocus(TEST_CATEGORY);
            Assert.AreEqual(PreviewCameraFocus.FaceEditing, previewCamFocus);
        }

        [Test]
        public void SetRarityBackground()
        {
            avatarSlot.SetRarity(TEST_RARITY);
            Assert.IsTrue(Equals(avatarSlot.backgroundRarityImage.sprite, nftRarityBackgroundMapping.GetRarityImage(TEST_RARITY)), "The icon obtained from the mapping differs from the set one.");
        }

        [Test]
        public void SetFocus()
        {
            Assert.IsFalse(avatarSlot.focusedImage.enabled, "Outline should be disabled by default");
            avatarSlot.OnFocus();
            Assert.IsTrue(avatarSlot.focusedImage.enabled, "After the On Focus the focused outline is not correctly enabled");
            Assert.IsTrue(avatarSlot.tooltipContainer.gameObject.activeInHierarchy, "Tooltip container was not enabled on the avatar slot focus");
        }

        [Test]
        public void SetOutOfFocus()
        {
            avatarSlot.OnLoseFocus();
            Assert.IsFalse(avatarSlot.focusedImage.enabled, "After the On Lose Focus the focused outline is not disabled");
            Assert.IsFalse(avatarSlot.tooltipContainer.gameObject.activeInHierarchy, "Tooltip container was not disabled on the avatar slot un-focus");
        }

        [Test]
        public void SetTooltipText1()
        {
            avatarSlot.SetModel(new AvatarSlotComponentModel()
            {
                category = TEST_CATEGORY,
                isHidden = true,
                hiddenBy = "hat",
                wearableId = "asdasdasd"
            });

            Assert.AreEqual(avatarSlot.tooltipCategoryText.text, "Mask");
            Assert.True(avatarSlot.tooltipHiddenText.gameObject.activeSelf);
            Assert.AreEqual(avatarSlot.tooltipHiddenText.text, "Hidden by Hat");
        }

        [Test]
        public void SetTooltipText2()
        {
            avatarSlot.SetModel(new AvatarSlotComponentModel()
            {
                category = TEST_CATEGORY,
                isHidden = false,
                hiddenBy = "",
                wearableId = "asdasdasd"
            });

            avatarSlot.RefreshControl();

            Assert.AreEqual(avatarSlot.tooltipCategoryText.text, "Mask");
            Assert.False(avatarSlot.tooltipHiddenText.gameObject.activeInHierarchy);
        }

        [Test]
        public void SetHiddenBy()
        {
            avatarSlot.SetModel(new AvatarSlotComponentModel()
            {
                category = TEST_CATEGORY,
                wearableId = "asdasdasd"
            });
            avatarSlot.SetIsHidden(true, "mask");

            Assert.AreEqual(avatarSlot.tooltipCategoryText.text, "Mask");
            Assert.True(avatarSlot.tooltipHiddenText.gameObject.activeSelf);
            Assert.AreEqual(avatarSlot.tooltipHiddenText.text, "Hidden by Mask");
        }

        [Test]
        public void SetHiddenByMultipleCategories()
        {
            avatarSlot.SetModel(new AvatarSlotComponentModel()
            {
                category = TEST_CATEGORY,
                wearableId = "asdasdasd"
            });

            //Set first hiding category
            avatarSlot.SetIsHidden(true, "helmet");
            Assert.AreEqual(avatarSlot.tooltipCategoryText.text, "Mask");
            Assert.True(avatarSlot.tooltipHiddenText.gameObject.activeSelf);
            Assert.True(avatarSlot.hiddenSlot.activeSelf);
            Assert.AreEqual("Hidden by Helmet", avatarSlot.tooltipHiddenText.text);

            //Set second hiding category that should hide the first one
            avatarSlot.SetIsHidden(true, "top_head");
            Assert.AreEqual(avatarSlot.tooltipCategoryText.text, "Mask");
            Assert.True(avatarSlot.tooltipHiddenText.gameObject.activeSelf);
            Assert.AreEqual("Hidden by Helmet", avatarSlot.tooltipHiddenText.text);

            //Remove the first hiding category that should leave the second one as hiding
            avatarSlot.SetIsHidden(false, "helmet");
            Assert.AreEqual(avatarSlot.tooltipCategoryText.text, "Mask");
            Assert.True(avatarSlot.tooltipHiddenText.gameObject.activeSelf);
            Assert.AreEqual("Hidden by Top Head", avatarSlot.tooltipHiddenText.text);

            //Remove the first hiding category that should remove all hiding constrains
            avatarSlot.SetIsHidden(false, "top_head");
            Assert.AreEqual(avatarSlot.tooltipCategoryText.text, "Mask");
            Assert.False(avatarSlot.tooltipHiddenText.gameObject.activeInHierarchy);
            Assert.False(avatarSlot.hiddenSlot.activeSelf);
        }
    }
}
