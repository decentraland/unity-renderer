using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Interface;
using UnityEngine;

[CreateAssetMenu(fileName = "UserProfile", menuName = "UserProfile")]
public class UserProfile : ScriptableObject //TODO Move to base variable
{
    static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public event Action<UserProfile> OnUpdate;
    public event Action<Texture2D> OnFaceSnapshotReadyEvent;
    public event Action<string, long> OnAvatarExpressionSet;

    public string userId => model.userId;
    public string ethAddress => model.ethAddress;
    public string userName => model.name;
    public string description => model.description;
    public string email => model.email;
    public UserProfileModel.ParcelsWithAccess[] parcelsWithAccess => model.parcelsWithAccess;
    public List<string> blocked => model.blocked != null ? model.blocked : new List<string>();
    public List<string> muted => model.muted ?? new List<string>();
    public bool hasConnectedWeb3 => model.hasConnectedWeb3;
    public bool hasClaimedName => model.hasClaimedName;
    public AvatarModel avatar => model.avatar;
    public int tutorialStep => model.tutorialStep;
    internal Dictionary<string, int> inventory = new Dictionary<string, int>();

    public Texture2D faceSnapshot { get; private set; }
    private AssetPromise_Texture thumbnailPromise;

    internal UserProfileModel model = new UserProfileModel() //Empty initialization to avoid nullchecks
    {
        avatar = new AvatarModel()
    };

    public void UpdateData(UserProfileModel newModel, bool downloadAssets = true)
    {
        inventory.Clear();
        faceSnapshot = null;

        if (newModel == null)
        {
            model = null;
            return;
        }

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
            inventory = model.inventory.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        }

        if (downloadAssets && model.snapshots != null)
        {
            //NOTE(Brian): Get before forget to prevent referenceCount == 0 and asset unload
            var newThumbnailPromise = ThumbnailsManager.GetThumbnail(model.snapshots.face256, OnFaceSnapshotReady);
            ThumbnailsManager.ForgetThumbnail(thumbnailPromise);
            thumbnailPromise = newThumbnailPromise;
        }
        else
        {
            ThumbnailsManager.ForgetThumbnail(thumbnailPromise);
            thumbnailPromise = null;
        }

        OnUpdate?.Invoke(this);
    }

    public int GetItemAmount(string itemId)
    {
        if (inventory == null || !inventory.ContainsKey(itemId))
            return 0;

        return inventory[itemId];
    }

    private void OnFaceSnapshotReady(Asset_Texture texture)
    {
        if (faceSnapshot != null)
            Destroy(faceSnapshot);

        if (texture != null)
            faceSnapshot = texture.texture;

        OnUpdate?.Invoke(this);
        OnFaceSnapshotReadyEvent?.Invoke(faceSnapshot);
    }

    public void OverrideAvatar(AvatarModel newModel, Texture2D newFaceSnapshot)
    {
        if (model?.snapshots != null)
        {
            if (thumbnailPromise != null)
            {
                ThumbnailsManager.ForgetThumbnail(thumbnailPromise);
                thumbnailPromise = null;
            }

            OnFaceSnapshotReady(null);
        }

        model.avatar.CopyFrom(newModel);
        this.faceSnapshot = newFaceSnapshot;
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

    public string[] GetInventoryItemsIds()
    {
        return inventory.Keys.ToArray();
    }

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