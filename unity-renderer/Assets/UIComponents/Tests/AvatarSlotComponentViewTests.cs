using NUnit.Framework;
using UnityEngine;

public class AvatarSlotComponentViewTests
{
    private const string TEST_CATEGORY = "testCategory";

    private AvatarSlotComponentView avatarSlot;
    private Texture2D testTexture;
    private Sprite testSprite;
    private NftTypeIconSO nftTypeIconMapping;

    [SetUp]
    public void SetUp()
    {
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        nftTypeIconMapping = ScriptableObject.CreateInstance<NftTypeIconSO>();
        nftTypeIconMapping.nftIcons = new SerializableKeyValuePair<string, Sprite>[1];
        nftTypeIconMapping.nftIcons[0] = new SerializableKeyValuePair<string, Sprite>()
        {
            key = TEST_CATEGORY,
            value = testSprite
        };
        avatarSlot = BaseComponentView.Create<AvatarSlotComponentView>("AvatarSlot");
        avatarSlot.typeIcons = nftTypeIconMapping;
    }

    [TearDown]
    public void TearDown()
    {
        avatarSlot.Dispose();
        Object.Destroy(avatarSlot.gameObject);
        Object.Destroy(testTexture);
        Object.Destroy(testSprite);
        Object.Destroy(nftTypeIconMapping);
    }

    [Test]
    public void SetCategoryIcon()
    {
        avatarSlot.SetCategory(TEST_CATEGORY);
        Assert.IsTrue(Equals(avatarSlot.typeImage.sprite, nftTypeIconMapping.GetTypeImage(TEST_CATEGORY)), "The icon obtained from the mapping differs from the set one.");
    }

    [Test]
    public void SetFocus()
    {
        Assert.IsFalse(avatarSlot.focusedImage.enabled, "After the On Focus the focused outline is not correctly enabled");
        avatarSlot.OnFocus();
        Assert.IsTrue(avatarSlot.focusedImage.enabled, "After the On Focus the focused outline is not correctly enabled");
    }

    [Test]
    public void SetOutOfFocus()
    {
        avatarSlot.OnLoseFocus();
        Assert.IsFalse(avatarSlot.focusedImage.enabled, "After the On Focus the focused outline is not correctly enabled");
    }
}
