using DCL.Helpers;
using DCL.Social.Friends;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class FriendEntryShould
{
    FriendEntry entry;

    [SetUp]
    public void SetUp()
    {
        entry = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<FriendEntry>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/FriendEntry.prefab"));
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(entry.gameObject);
    }

    [Test]
    public void BePopulatedCorrectly()
    {
        var mockSnapshotObserver = new LazyTextureObserver();
        mockSnapshotObserver.RefreshWithTexture(Texture2D.whiteTexture);

        var model = new FriendEntryModel
        {
            userId = "userId-1",
            coords = Vector2.one,
            avatarSnapshotObserver = mockSnapshotObserver,
            realm = "realm-test",
            realmServerName = "realm-test",
            realmLayerName = "realm-layer-test",
            status = PresenceStatus.ONLINE,
            userName = "test-name"
        };

        entry.Populate(model);

        Assert.AreEqual(model.userName, entry.playerNameText.text);
    }

    [Test]
    public void SendProperEventWhenWhisperButtonIsPressed()
    {
        var model = new FriendEntryModel
        {
            userId = "userId-1"
        };
        entry.Populate(model);
        bool buttonPressed = false;
        entry.OnWhisperClick += (x) =>
        {
            if (x == entry)
                buttonPressed = true;
        };
        entry.whisperButton.onClick.Invoke();
        Assert.IsTrue(buttonPressed);
    }

    [Test]
    public void SendProperEventWhenOnMenuToggleIsPressed()
    {
        var model = new FriendEntryModel
        {
            userId = "userId-1"
        };
        entry.Populate(model);
        bool buttonPressed = false;
        entry.OnMenuToggle += (x) =>
        {
            if (x == entry)
                buttonPressed = true;
        };
        entry.menuButton.onClick.Invoke();
        Assert.IsTrue(buttonPressed);
    }
}
