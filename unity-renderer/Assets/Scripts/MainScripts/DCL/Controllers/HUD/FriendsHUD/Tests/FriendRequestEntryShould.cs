using NUnit.Framework;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

public class FriendRequestEntryShould : IntegrationTestSuite_Legacy
{
    static string FRIEND_REQUEST_ENTRY_RESOURCE_NAME = "FriendRequestEntry";

    FriendRequestEntry entry;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        GameObject go = Object.Instantiate((GameObject)Resources.Load(FRIEND_REQUEST_ENTRY_RESOURCE_NAME));
        entry = go.GetComponent<FriendRequestEntry>();
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
        var model1snapshot = Texture2D.whiteTexture;
        var model2snapshot = Texture2D.blackTexture;
        var model1 = new FriendEntry.Model() { userName = "test1", avatarSnapshotObserver = LazyTextureObserver.CreateWithTexture(model1snapshot) };
        var model2 = new FriendEntry.Model() { userName = "test2", avatarSnapshotObserver = LazyTextureObserver.CreateWithTexture(model2snapshot) };

        entry.userId = "userId1";
        entry.Populate(model1);
        entry.SetReceived(true);

        Assert.AreEqual(model1.userName, entry.playerNameText.text);
        Assert.AreEqual(model1snapshot, entry.playerImage.texture);

        Assert.IsFalse(entry.cancelButton.gameObject.activeSelf);
        Assert.IsTrue(entry.acceptButton.gameObject.activeSelf);
        Assert.IsTrue(entry.rejectButton.gameObject.activeSelf);

        entry.userId = "userId2";
        entry.Populate(model2);
        entry.SetReceived(false);

        Assert.AreEqual(model2.userName, entry.playerNameText.text);
        Assert.AreEqual(model2snapshot, entry.playerImage.texture);

        Assert.IsTrue(entry.cancelButton.gameObject.activeSelf);
        Assert.IsFalse(entry.acceptButton.gameObject.activeSelf);
        Assert.IsFalse(entry.rejectButton.gameObject.activeSelf);
    }

    [Test]
    public void SendProperEventWhenOnAcceptedIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.userId = "userId-1";
        entry.Populate(model);

        bool buttonPressed = false;
        entry.OnAccepted += (x) =>
        {
            if (x == entry)
                buttonPressed = true;
        };
        entry.acceptButton.onClick.Invoke();
        Assert.IsTrue(buttonPressed);
    }

    [Test]
    public void SendProperEventWhenOnCancelledIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.userId = "userId-1";
        entry.Populate(model);
        bool buttonPressed = false;
        entry.OnCancelled += (x) =>
        {
            if (x == entry)
                buttonPressed = true;
        };
        entry.cancelButton.onClick.Invoke();
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

    [Test]
    public void SendProperEventWhenOnRejectedIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.userId = "userId-1";
        entry.Populate(model);
        bool buttonPressed = false;
        entry.OnRejected += (x) =>
        {
            if (x == entry)
                buttonPressed = true;
        };
        entry.rejectButton.onClick.Invoke();
        Assert.IsTrue(buttonPressed);
    }
}