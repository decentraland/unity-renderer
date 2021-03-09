using System.Collections.Generic;
using System.Linq;
using DCL.Interface;

public class FriendsTabView : FriendsTabViewBase
{
    private const int CREATION_AMOUNT_PER_FRAME = 5;

    public EntryList onlineFriendsList = new EntryList();
    public EntryList offlineFriendsList = new EntryList();
    public event System.Action<FriendEntry> OnWhisper;
    public event System.Action<string> OnDeleteConfirmation;

    private string lastProcessedFriend;
    internal readonly Dictionary<string, FriendEntryBase.Model> creationQueue = new Dictionary<string, FriendEntryBase.Model>();

    public override void Initialize(FriendsHUDView owner, int preinstantiatedEntries)
    {
        base.Initialize(owner, preinstantiatedEntries);

        onlineFriendsList.toggleTextPrefix = "ONLINE";
        offlineFriendsList.toggleTextPrefix = "OFFLINE";

        if (ChatController.i != null)
        {
            ChatController.i.OnAddMessage -= ChatController_OnAddMessage;
            ChatController.i.OnAddMessage += ChatController_OnAddMessage;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (ChatController.i != null)
            ChatController.i.OnAddMessage -= ChatController_OnAddMessage;
    }

    protected override bool CreateEntry(string userId)
    {
        if (!base.CreateEntry(userId)) return false;

        var entry = GetEntry(userId) as FriendEntry;

        entry.OnWhisperClick += (x) => OnWhisper?.Invoke(x);
        entry.OnJumpInClick += (x) => this.owner.OnCloseButtonPressed();

        return true;
    }

    public override bool RemoveEntry(string userId)
    {
        if (!base.RemoveEntry(userId))
            return false;

        offlineFriendsList.Remove(userId);
        onlineFriendsList.Remove(userId);
        offlineFriendsList.RemoveLastTimestamp(userId);
        onlineFriendsList.RemoveLastTimestamp(userId);
        return true;
    }

    public override bool UpdateEntry(string userId, FriendEntryBase.Model model)
    {
        if (!base.UpdateEntry(userId, model))
        {
            //Replace the queued model for creation for the updated one.
            if (creationQueue.ContainsKey(userId))
                creationQueue[userId] = model;
            return false;
        }

        var entry = entries[userId];

        if (model.status == PresenceStatus.ONLINE)
        {
            offlineFriendsList.Remove(userId);
            onlineFriendsList.Add(userId, entry);

            var removedTimestamp = offlineFriendsList.RemoveLastTimestamp(userId);
            onlineFriendsList.AddOrUpdateLastTimestamp(removedTimestamp);
        }
        else
        {
            onlineFriendsList.Remove(userId);
            offlineFriendsList.Add(userId, entry);

            var removedTimestamp = onlineFriendsList.RemoveLastTimestamp(userId);
            offlineFriendsList.AddOrUpdateLastTimestamp(removedTimestamp);
        }

        return true;
    }

    protected override void OnPressDeleteButton(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;

        FriendEntryBase friendEntryToDelete = GetEntry(userId);
        if (friendEntryToDelete != null)
        {
            confirmationDialog.SetText($"Are you sure you want to delete {friendEntryToDelete.model.userName} as a friend?");
            confirmationDialog.Show(() =>
            {
                RemoveEntry(userId);
                OnDeleteConfirmation?.Invoke(userId);
            });
        }
    }

    private void ChatController_OnAddMessage(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE)
            return;

        FriendEntryBase friend = GetEntry(message.sender != UserProfile.GetOwnUserProfile().userId
            ? message.sender
            : message.recipient);

        if (friend == null)
            return;

        bool reorderFriendEntries = false;

        if (friend.userId != lastProcessedFriend)
        {
            lastProcessedFriend = friend.userId;
            reorderFriendEntries = true;
        }

        LastFriendTimestampModel timestampToUpdate = new LastFriendTimestampModel
        {
            userId = friend.userId,
            lastMessageTimestamp = message.timestamp
        };

        // Each time a private message is received (or sent by the player), we sort the online and offline lists by timestamp
        if (friend.model.status == PresenceStatus.ONLINE)
        {
            onlineFriendsList.AddOrUpdateLastTimestamp(timestampToUpdate, reorderFriendEntries);
        }
        else
        {
            offlineFriendsList.AddOrUpdateLastTimestamp(timestampToUpdate, reorderFriendEntries);
        }

        lastProcessedFriend = friend.userId;
    }

    public void CreateOrUpdateEntryDeferred(string userId, FriendEntryBase.Model model)
    {
        if(creationQueue.ContainsKey(userId))
            creationQueue[userId] = model;
        else
            creationQueue.Add(userId, model);
    }

    /// <summary>
    /// To avoid having hiccups when a player with dozens of friends load into the game
    /// we deferred the entries instantiation to multiple frames
    /// </summary>
    protected override void UpdateLayout()
    {
        if (creationQueue.Count == 0)
            return;

        for (int i = 0; i < CREATION_AMOUNT_PER_FRAME && creationQueue.Count != 0; i++)
        {
            var pair = creationQueue.FirstOrDefault();
            creationQueue.Remove(pair.Key);
            CreateEntry(pair.Key);
            UpdateEntry(pair.Key, pair.Value);
        }

        //If we have creations to process we avoid reconstructing the layout until we are done
        if (creationQueue.Count != 0)
        {
            layoutIsDirty = false;
        }

        base.UpdateLayout();
    }
}