using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

[CreateAssetMenu(fileName = "UserProfile", menuName = "UserProfile")]
public class UserProfile : ScriptableObject //TODO Move to base variable
{
    static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public event Action<UserProfile> OnUpdate;
    public event Action<string, long> OnAvatarExpressionSet;

    public string userId => model.userId;
    public string ethAddress => model.ethAddress;
    public string userName => model.name;
    public string description => model.description;
    public string email => model.email;
    public string bodySnapshotURL => model.snapshots.body;
    public string face128SnapshotURL => model.snapshots.face128;
    public UserProfileModel.ParcelsWithAccess[] parcelsWithAccess => model.parcelsWithAccess;
    public List<string> blocked => model.blocked != null ? model.blocked : new List<string>();
    public List<string> muted => model.muted ?? new List<string>();
    public bool hasConnectedWeb3 => model.hasConnectedWeb3;
    public bool hasClaimedName => model.hasClaimedName;
    public AvatarModel avatar => model.avatar;
    public int tutorialStep => model.tutorialStep;

    internal Dictionary<string, int> inventory = new Dictionary<string, int>();

    public ILazyTextureObserver snapshotObserver = new LazyTextureObserver();

    internal UserProfileModel model = new UserProfileModel() //Empty initialization to avoid nullchecks
    {
        avatar = new AvatarModel()
    };

    public void UpdateData(UserProfileModel newModel)
    {
        if (newModel == null)
        {
            model = null;
            return;
        }

        bool faceSnapshotDirty = model.snapshots.face256 != newModel.snapshots.face256;

        model.userId = newModel.userId;
        model.ethAddress = newModel.ethAddress;
        model.parcelsWithAccess = newModel.parcelsWithAccess;
        model.tutorialStep = newModel.tutorialStep;
        model.hasClaimedName = newModel.hasClaimedName;
        model.name = newModel.name;
        model.email = newModel.email;
        model.description = newModel.description;
        model.avatar.CopyFrom(newModel.avatar);
        model.snapshots = newModel.snapshots;
        model.hasConnectedWeb3 = newModel.hasConnectedWeb3;
        model.inventory = newModel.inventory;
        model.blocked = newModel.blocked;
        model.muted = newModel.muted;

        if (model.inventory != null)
        {
            SetInventory(model.inventory);
        }

        if (model.snapshots != null && faceSnapshotDirty)
        {
            this.snapshotObserver.RefreshWithUri(model.snapshots.face256);
        }

        OnUpdate?.Invoke(this);
    }

    public int GetItemAmount(string itemId)
    {
        if (inventory == null || !inventory.ContainsKey(itemId))
            return 0;

        return inventory[itemId];
    }

    public void OverrideAvatar(AvatarModel newModel, Texture2D newFaceSnapshot)
    {
        model.avatar.CopyFrom(newModel);
        this.snapshotObserver.RefreshWithTexture(newFaceSnapshot);

        OnUpdate?.Invoke(this);
    }

    public void SetAvatarExpression(string id)
    {
        var timestamp = (long) (DateTime.UtcNow - epochStart).TotalMilliseconds;
        avatar.expressionTriggerId = id;
        avatar.expressionTriggerTimestamp = timestamp;
        WebInterface.SendExpression(id, timestamp);
        OnUpdate?.Invoke(this);
        OnAvatarExpressionSet?.Invoke(id, timestamp);
    }

    public void SetInventory(string[] inventoryIds)
    {
        inventory.Clear();
        inventory = inventoryIds.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
    }

    public string[] GetInventoryItemsIds() { return inventory.Keys.ToArray(); }

    internal static UserProfile ownUserProfile;

    public static UserProfile GetOwnUserProfile()
    {
        if (ownUserProfile == null)
        {
            ownUserProfile = Resources.Load<UserProfile>("ScriptableObjects/OwnUserProfile");
        }

        return ownUserProfile;
    }

    public UserProfileModel CloneModel() => model.Clone();

    public bool IsBlocked(string userId)
    {
        return blocked != null && blocked.Contains(userId);
    }

    public void Block(string userId)
    {
        if (IsBlocked(userId)) return;
        blocked.Add(userId);
    }
    
    public void Unblock(string userId)
    {
        blocked.Remove(userId);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        if (UnityEditor.AssetDatabase.Contains(this))
            Resources.UnloadAsset(this);
    }
#endif
}