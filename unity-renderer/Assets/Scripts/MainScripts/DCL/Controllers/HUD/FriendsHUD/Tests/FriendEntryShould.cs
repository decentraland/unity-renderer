using NUnit.Framework;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

public class FriendEntryShould : IntegrationTestSuite_Legacy
{
    static string FRIEND_ENTRY_RESOURCE_NAME = "FriendEntry";

    FriendEntry entry;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        GameObject go = Object.Instantiate((GameObject)Resources.Load(FRIEND_ENTRY_RESOURCE_NAME));
        entry = go.GetComponent<FriendEntry>();
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(entry.gameObject);
        yield break;
    }

    [Test]
    public void BePopulatedCorrectly()
    {
        var mockSnapshotObserver = new LazyTextureObserver();
        mockSnapshotObserver.RefreshWithTexture(Texture2D.whiteTexture);

        FriendEntry.Model model = new FriendEntry.Model()
        {
            coords = Vector2.one,
            avatarSnapshotObserver = mockSnapshotObserver,
            realm = "realm-test",
            realmServerName = "realm-test",
            realmLayerName = "realm-layer-test",
            status = PresenceStatus.ONLINE,
            userName = "test-name"
        };

        entry.userId = "userId-1";
        entry.Populate(model);

        Assert.AreEqual(model.userName, entry.playerNameText.text);
        Assert.AreEqual(entry.playerImage.texture, Texture2D.whiteTexture);
    }

    [Test]
    public void SendProperEventWhenWhisperButtonIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.userId = "userId-1";
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
        var model = new FriendEntry.Model() { };
        entry.userId = "userId-1";
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