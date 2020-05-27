using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class FriendsHUDViewShould : TestsBase
{
    FriendsHUDController controller;
    FriendsHUDView view;

    protected override bool justSceneSetUp => true;
    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        base.SetUp();

        controller = new FriendsHUDController();
        controller.Initialize(null, null);
        this.view = controller.view;

        Assert.IsTrue(view != null, "Friends hud view is null?");
        Assert.IsTrue(controller != null, "Friends hud controller is null?");
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        yield return base.TearDown();
        UnityEngine.Object.Destroy(view);
    }

    [Test]
    public void ChangeContentWhenClickingTabs()
    {
        controller.view.friendsButton.onClick.Invoke();

        Assert.IsTrue(controller.view.friendsList.gameObject.activeSelf);
        Assert.IsFalse(controller.view.friendRequestsList.gameObject.activeSelf);

        controller.view.friendRequestsButton.onClick.Invoke();

        Assert.IsFalse(controller.view.friendsList.gameObject.activeSelf);
        Assert.IsTrue(controller.view.friendRequestsList.gameObject.activeSelf);
    }

    [Test]
    public void PopulateFriendListCorrectly()
    {
        string id1 = "userId-1";
        string id2 = "userId-2";

        var entry1 = CreateFriendEntry(id1, "Pravus", PresenceStatus.ONLINE);
        var entry2 = CreateFriendEntry(id2, "Brian", PresenceStatus.OFFLINE);

        Assert.IsNotNull(entry1);
        Assert.AreEqual(entry1.model.userName, entry1.playerNameText.text);
        Assert.AreEqual(controller.view.friendsList.onlineFriendsList.container, entry1.transform.parent);

        Assert.IsNotNull(entry2);
        Assert.AreEqual(entry2.model.userName, entry2.playerNameText.text);
        Assert.AreEqual(controller.view.friendsList.offlineFriendsList.container, entry2.transform.parent);

        var model2 = entry2.model;
        model2.status = PresenceStatus.ONLINE;
        controller.view.friendsList.CreateOrUpdateEntry(id2, model2);

        Assert.AreEqual(controller.view.friendsList.onlineFriendsList.container, entry2.transform.parent);
    }


    [Test]
    public void RemoveFriendCorrectly()
    {
        string id1 = "userId-1";

        controller.view.friendRequestsList.CreateOrUpdateEntry(id1, new FriendEntry.Model(), isReceived: true);

        Assert.IsNotNull(controller.view.friendRequestsList.GetEntry(id1));

        controller.view.friendRequestsList.RemoveEntry(id1);

        Assert.IsNull(controller.view.friendRequestsList.GetEntry(id1));
    }

    [Test]
    public void PopulateFriendRequestCorrectly()
    {
        string id1 = "userId-1";
        string id2 = "userId-2";

        var entry1 = CreateFriendRequestEntry(id1, "Pravus", true);
        var entry2 = CreateFriendRequestEntry(id2, "Brian", false);

        Assert.IsNotNull(entry1);
        Assert.AreEqual("Pravus", entry1.playerNameText.text);
        Assert.AreEqual(controller.view.friendRequestsList.receivedRequestsList.container, entry1.transform.parent);

        Assert.IsNotNull(entry2);
        Assert.AreEqual("Brian", entry2.playerNameText.text);
        Assert.AreEqual(controller.view.friendRequestsList.sentRequestsList.container, entry2.transform.parent);

        controller.view.friendRequestsList.UpdateEntry(id2, entry2.model, true);
        Assert.AreEqual(controller.view.friendRequestsList.receivedRequestsList.container, entry2.transform.parent);
    }

    [Test]
    public void CountProperlyStatus()
    {
        CreateFriendEntry("user1", "Armando Barreda", PresenceStatus.ONLINE);
        CreateFriendEntry("user2", "Guillermo Andino", PresenceStatus.ONLINE);

        CreateFriendEntry("user3", "Wanda Nara", PresenceStatus.OFFLINE);
        CreateFriendEntry("user4", "Mirtha Legrand", PresenceStatus.OFFLINE);

        Assert.AreEqual(2, view.friendsList.onlineFriendsList.Count());
        Assert.AreEqual(2, view.friendsList.offlineFriendsList.Count());

        view.friendsList.RemoveEntry("user1");
        view.friendsList.RemoveEntry("user3");

        Assert.AreEqual(1, view.friendsList.onlineFriendsList.Count());
        Assert.AreEqual(1, view.friendsList.offlineFriendsList.Count());
    }


    [Test]
    public void OpenContextMenuProperly()
    {
        var entry = CreateFriendEntry("userId-1", "Pravus");
        bool onMenuToggleCalled = false;

        System.Action<FriendEntryBase> callback = (x) => { onMenuToggleCalled = true; };

        Assert.IsFalse(view.friendsList.contextMenuPanel.gameObject.activeSelf);

        entry.OnMenuToggle += callback;
        entry.menuButton.onClick.Invoke();
        entry.OnMenuToggle -= callback;

        Assert.IsTrue(onMenuToggleCalled);
        Assert.IsTrue(view.friendsList.contextMenuPanel.gameObject.activeSelf);
        Assert.AreEqual(entry, view.friendsList.contextMenuPanel.targetEntry);
    }

    [Test]
    public void DeleteFriendProperly()
    {
        string id1 = "userId-1";
        var entry = CreateFriendEntry(id1, "Ted Bundy");

        entry.menuButton.onClick.Invoke();
        Assert.IsTrue(view.friendsList.contextMenuPanel.deleteButton.gameObject.activeSelf);
        Assert.IsTrue(view.friendsList.contextMenuPanel.deleteButton.isActiveAndEnabled);

        view.friendsList.contextMenuPanel.deleteButton.onClick.Invoke();
        view.friendsList.confirmationDialog.confirmButton.onClick.Invoke();

        Assert.IsNull(view.friendsList.GetEntry(id1));
    }

    [Test]
    public void RejectIncomingFriendRequestsProperly()
    {
        //NOTE(Brian): Confirm cancellation
        var entry = CreateFriendRequestEntry("id1", "Padre Grassi", isReceived: true);

        entry.rejectButton.onClick.Invoke();

        Assert.IsTrue(view.friendRequestsList.confirmationDialog.gameObject.activeSelf);

        view.friendRequestsList.confirmationDialog.confirmButton.onClick.Invoke();

        Assert.IsFalse(view.friendRequestsList.confirmationDialog.gameObject.activeSelf);
        Assert.IsNull(view.friendRequestsList.GetEntry(entry.userId));

        //NOTE(Brian): Deny cancellation
        var entry2 = CreateFriendRequestEntry("id1", "Warren Buffet", isReceived: true);

        entry2.rejectButton.onClick.Invoke();

        Assert.IsTrue(view.friendRequestsList.confirmationDialog.gameObject.activeSelf);

        view.friendRequestsList.confirmationDialog.cancelButton.onClick.Invoke();

        Assert.IsFalse(view.friendRequestsList.confirmationDialog.gameObject.activeSelf);
        Assert.IsNotNull(view.friendRequestsList.GetEntry(entry2.userId));
    }

    [Test]
    public void SendAndCancelFriendRequestsProperly()
    {
        //NOTE(Brian): Confirm cancellation
        var entry = CreateFriendRequestEntry("id1", "Padre Grassi", isReceived: false);

        entry.cancelButton.onClick.Invoke();

        Assert.IsTrue(view.friendRequestsList.confirmationDialog.gameObject.activeSelf);

        view.friendRequestsList.confirmationDialog.confirmButton.onClick.Invoke();

        Assert.IsFalse(view.friendRequestsList.confirmationDialog.gameObject.activeSelf);
        Assert.IsNull(view.friendRequestsList.GetEntry(entry.userId));

        //NOTE(Brian): Deny cancellation
        var entry2 = CreateFriendRequestEntry("id1", "Warren Buffet", isReceived: false);

        entry2.cancelButton.onClick.Invoke();

        Assert.IsTrue(view.friendRequestsList.confirmationDialog.gameObject.activeSelf);

        view.friendRequestsList.confirmationDialog.cancelButton.onClick.Invoke();

        Assert.IsFalse(view.friendRequestsList.confirmationDialog.gameObject.activeSelf);
        Assert.IsNotNull(view.friendRequestsList.GetEntry(entry2.userId));
    }

    FriendEntry CreateFriendEntry(string id, string name, PresenceStatus status = PresenceStatus.ONLINE)
    {
        var model1 = new FriendEntry.Model()
        {
            status = status,
            userName = name,
        };

        controller.view.friendsList.CreateOrUpdateEntry(id, model1);

        return controller.view.friendsList.GetEntry(id) as FriendEntry;
    }

    FriendRequestEntry CreateFriendRequestEntry(string id, string name, bool isReceived)
    {
        var model1 = new FriendEntry.Model()
        {
            userName = name,
        };

        controller.view.friendRequestsList.CreateOrUpdateEntry(id, model1, isReceived);

        return controller.view.friendRequestsList.GetEntry(id) as FriendRequestEntry;
    }

}
