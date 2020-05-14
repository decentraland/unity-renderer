using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Interface;
using UnityEngine;

[CreateAssetMenu(fileName = "UserProfile", menuName = "UserProfile")]
public class UserProfile : ScriptableObject //TODO Move to base variable
{
    static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public event Action<UserProfile> OnUpdate;
    public event Action<Sprite> OnFaceSnapshotReadyEvent;
    public event Action<string> OnAvatarExpressionSet;

    public string userId => model.userId;
    public string userName => model.name;
    public string description => model.description;
    public string email => model.email;
    public List<string> blocked => model.blocked;
    public bool hasConnectedWeb3 => model.hasConnectedWeb3;
    public bool hasClaimedName => model.hasClaimedName;
    public AvatarModel avatar => model.avatar;
    public int tutorialStep => model.tutorialStep;
    internal Dictionary<string, int> inventory = new Dictionary<string, int>();

    public Sprite faceSnapshot { get; private set; }
    public Sprite bodySnapshot { get; private set; }

    internal UserProfileModel model = new UserProfileModel() //Empty initialization to avoid nullchecks
    {
        avatar = new AvatarModel()
    };

    public void UpdateData(UserProfileModel newModel, bool downloadAssets = true)
    {
        ForgetThumbnail(model?.snapshots?.face, OnFaceSnapshotReady);
        ForgetThumbnail(model?.snapshots?.body, OnBodySnapshotReady);

        inventory.Clear();
        faceSnapshot = null;
        bodySnapshot = null;

        if (newModel == null)
        {
            model = null;
            return;
        }

        model.userId = newModel.userId;
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
        if (model.inventory != null)
        {
            inventory = model.inventory.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        }

        if (downloadAssets && model.snapshots != null)
        {
            GetThumbnail(model.snapshots.face, OnFaceSnapshotReady);
            GetThumbnail(model.snapshots.body, OnBodySnapshotReady);
        }

        OnUpdate?.Invoke(this);
    }

    public int GetItemAmount(string itemId)
    {
        if (inventory == null || !inventory.ContainsKey(itemId))
            return 0;

        return inventory[itemId];
    }

    private void OnFaceSnapshotReady(Sprite sprite)
    {
        faceSnapshot = sprite;
        OnUpdate?.Invoke(this);
        OnFaceSnapshotReadyEvent?.Invoke(sprite);
    }

    private void OnBodySnapshotReady(Sprite sprite)
    {
        bodySnapshot = sprite;
        OnUpdate?.Invoke(this);
    }

    public void OverrideAvatar(AvatarModel newModel, Sprite faceSnapshot, Sprite bodySnapshot)
    {
        if (model?.snapshots != null)
        {
            ForgetThumbnail(model.snapshots.face, OnFaceSnapshotReady);
            ForgetThumbnail(model.snapshots.body, OnBodySnapshotReady);
        }

        model.avatar.CopyFrom(newModel);
        this.faceSnapshot = faceSnapshot;
        this.bodySnapshot = bodySnapshot;
        OnUpdate?.Invoke(this);
    }

    public void SetAvatarExpression(string id)
    {
        var timestamp = (long)(DateTime.UtcNow - epochStart).TotalMilliseconds;
        avatar.expressionTriggerId = id;
        avatar.expressionTriggerTimestamp = timestamp;
        WebInterface.SendExpression(id, timestamp);
        OnUpdate?.Invoke(this);
        OnAvatarExpressionSet?.Invoke(id);
    }

    public string[] GetInventoryItemsIds()
    {
        return inventory.Keys.ToArray();
    }

    public void SetTutorialStepId(int newTutorialStep)
    {
        model.tutorialStep = newTutorialStep;

        WebInterface.SaveUserTutorialStep(newTutorialStep);
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

    private void GetThumbnail(string url, Action<Sprite> callback)
    {
        if (string.IsNullOrEmpty(url))
            return;
        ThumbnailsManager.GetThumbnail(url, callback);
    }

    private void ForgetThumbnail(string url, Action<Sprite> callback)
    {
        if (string.IsNullOrEmpty(url))
            return;
        ThumbnailsManager.ForgetThumbnail(url, callback);
    }
}
