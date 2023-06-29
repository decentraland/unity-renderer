using DCL;
using DCL.Helpers;
using DCL.UserProfiles;
using Decentraland.Renderer.KernelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Environment = DCL.Environment;

[CreateAssetMenu(fileName = "UserProfile", menuName = "UserProfile")]
public class UserProfile : ScriptableObject //TODO Move to base variable
{
    public enum EmoteSource
    {
        EmotesWheel,
        Shortcut,
        Command,
        Backpack,
    }

    private const string FALLBACK_NAME = "fallback";

    public event Action<UserProfile> OnUpdate;
    public event Action<string, long, EmoteSource> OnAvatarEmoteSet;

    public string userId => model.userId;
    public string ethAddress => model.ethAddress;
    public string userName => model.name;
    public string description => model.description;
    public string email => model.email;
    public string bodySnapshotURL => model.ComposeCorrectUrl(model.snapshots.body);
    public string face256SnapshotURL => model.ComposeCorrectUrl(model.snapshots.face256);
    public string baseUrl => model.baseUrl;
    public UserProfileModel.ParcelsWithAccess[] parcelsWithAccess => model.parcelsWithAccess;
    public List<string> blocked => model.blocked ?? new List<string>();
    public List<string> muted => model.muted ?? new List<string>();
    public bool hasConnectedWeb3 => model.hasConnectedWeb3;
    public bool hasClaimedName => model.hasClaimedName;
    public bool isGuest => !model.hasConnectedWeb3;
    public AvatarModel avatar => model.avatar;
    public int tutorialStep => model.tutorialStep;
    public List<UserProfileModel.Link> Links => model.links;
    public AdditionalInfo AdditionalInfo => model.AdditionalInfo;

    internal Dictionary<string, int> inventory = new ();

    public ILazyTextureObserver snapshotObserver = new LazyTextureObserver();
    public ILazyTextureObserver bodySnapshotObserver = new LazyTextureObserver();

    internal static UserProfile ownUserProfile;

    // Empty initialization to avoid null-checks
    internal UserProfileModel model = new () { avatar = new AvatarModel() };

    private UserProfileModel ModelFallback() =>
        UserProfileModel.FallbackModel(FALLBACK_NAME, this.GetInstanceID());

    private AvatarModel AvatarFallback() =>
        AvatarModel.FallbackModel(FALLBACK_NAME, this.GetInstanceID());

    private int emoteLamportTimestamp = 1;

    public void UpdateData(UserProfileModel newModel)
    {
        if (newModel == null)
        {
            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("user_profile_null_model_exception"))
                Debug.LogError("Model is null when updating UserProfile! Using fallback or previous model instead.");

            // Check if there is a previous model to fallback to. Because default model has everything empty or null.
            newModel = string.IsNullOrEmpty(model.userId) ? ModelFallback() : model;
        }

        if (newModel.avatar == null)
        {
            model.avatar = new AvatarModel();

            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("user_profile_null_model_exception"))
                Debug.LogError("Avatar is null when updating UserProfile! Using fallback or previous avatar instead.");

            // Check if there is a previous avatar to fallback to.
            newModel.avatar = string.IsNullOrEmpty(model.userId) ? AvatarFallback() : model.avatar;
        }

        bool faceSnapshotDirty = model.snapshots.face256 != newModel.snapshots.face256;
        bool bodySnapshotDirty = model.snapshots.body != newModel.snapshots.body;

        model.userId = newModel.userId;
        model.ethAddress = newModel.ethAddress;
        model.parcelsWithAccess = newModel.parcelsWithAccess;
        model.tutorialStep = newModel.tutorialStep;
        model.hasClaimedName = newModel.hasClaimedName;
        model.name = newModel.name;
        model.email = newModel.email;
        model.description = newModel.description;
        model.baseUrl = newModel.baseUrl;
        model.avatar.CopyFrom(newModel.avatar);
        model.snapshots = newModel.snapshots;
        model.hasConnectedWeb3 = newModel.hasConnectedWeb3;
        model.blocked = newModel.blocked;
        model.muted = newModel.muted;
        model.version = newModel.version;
        model.links = newModel.links;
        model.AdditionalInfo.CopyFrom(newModel.AdditionalInfo);

        if (faceSnapshotDirty)
            snapshotObserver.RefreshWithUri(face256SnapshotURL);

        if (bodySnapshotDirty)
            bodySnapshotObserver.RefreshWithUri(bodySnapshotURL);

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

    public void SetAvatarExpression(string id, EmoteSource source)
    {
        int timestamp = emoteLamportTimestamp++;
        avatar.expressionTriggerId = id;
        avatar.expressionTriggerTimestamp = timestamp;

        ClientEmotesKernelService emotes = Environment.i.serviceLocator.Get<IRPC>().Emotes();
        // TODO: fix message `Timestamp` should NOT be `float`, we should use `int lamportTimestamp` or `long timeStamp`
        emotes?.TriggerExpression(new TriggerExpressionRequest()
        {
            Id = id,
            Timestamp = timestamp
        });

        OnUpdate?.Invoke(this);
        OnAvatarEmoteSet?.Invoke(id, timestamp, source);
    }

    public void SetInventory(IEnumerable<string> inventoryIds)
    {
        inventory.Clear();
        inventory = inventoryIds.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
    }

    public void AddToInventory(string wearableId)
    {
        if (inventory.ContainsKey(wearableId))
            inventory[wearableId]++;
        else
            inventory.Add(wearableId, 1);
    }

    public void RemoveFromInventory(string wearableId) { inventory.Remove(wearableId); }

    public bool ContainsInInventory(string wearableId) => inventory.ContainsKey(wearableId);

    public string[] GetInventoryItemsIds() =>
        inventory.Keys.ToArray();

    // TODO: Remove this call. The own user profile should be accessed via IUserProfileBridge.GetOwn()
    public static UserProfile GetOwnUserProfile()
    {
        if (ownUserProfile == null)
            ownUserProfile = Resources.Load<UserProfile>("ScriptableObjects/OwnUserProfile");

        return ownUserProfile;
    }

    public UserProfileModel CloneModel() => model.Clone();

    public bool IsBlocked(string userId) =>
        blocked != null && blocked.Contains(userId);

    public void Block(string userId)
    {
        if (IsBlocked(userId))
            return;
        blocked.Add(userId);
    }

    public void Unblock(string userId) { blocked.Remove(userId); }

    public bool HasEquipped(string wearableId) => avatar.wearables.Contains(wearableId);

#if UNITY_EDITOR
    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        if (AssetDatabase.Contains(this))
            Resources.UnloadAsset(this);
    }
#endif
}
