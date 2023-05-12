using DCL.Helpers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class FriendRequestEntryShould
{
    FriendRequestEntry entry;

    [SetUp]
    public void SetUp()
    {
        entry = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<FriendRequestEntry>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/FriendRequestEntry.prefab"));
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(entry.gameObject);
    }

    [Test]
    public void BePopulatedCorrectly()
    {
        var model1snapshot = Texture2D.whiteTexture;
        var model2snapshot = Texture2D.blackTexture;
        var model1 = new FriendRequestEntryModel
        {
            userId = "userId1",
            userName = "test1",
            avatarSnapshotObserver = LazyTextureObserver.CreateWithTexture(model1snapshot),
            isReceived = true
        };
        var model2 = new FriendRequestEntryModel
        {
            userId = "userId2",
            userName = "test2",
            avatarSnapshotObserver = LazyTextureObserver.CreateWithTexture(model2snapshot),
            isReceived = false
        };

        entry.Populate(model1);

        Assert.AreEqual(model1.userName, entry.playerNameText.text);

        Assert.IsFalse(entry.cancelButton.gameObject.activeSelf);
        Assert.IsTrue(entry.acceptButton.gameObject.activeSelf);
        Assert.IsTrue(entry.rejectButton.gameObject.activeSelf);

        entry.Populate(model2);

        Assert.AreEqual(model2.userName, entry.playerNameText.text);

        Assert.IsTrue(entry.cancelButton.gameObject.activeSelf);
        Assert.IsFalse(entry.acceptButton.gameObject.activeSelf);
        Assert.IsFalse(entry.rejectButton.gameObject.activeSelf);
    }

    [Test]
    public void SendProperEventWhenOnAcceptedIsPressed()
    {
        var model = new FriendRequestEntryModel
        {
            userId = "userId-1"
        };
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
        var model = new FriendRequestEntryModel
        {
            userId = "userId-1"
        };
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
        var model = new FriendRequestEntryModel
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

    [Test]
    public void SendProperEventWhenOnRejectedIsPressed()
    {
        var model = new FriendRequestEntryModel
        {
            userId = "userId-1"
        };
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
