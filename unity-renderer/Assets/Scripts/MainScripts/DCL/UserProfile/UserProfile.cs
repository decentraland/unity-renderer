using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

[assembly: InternalsVisibleTo("UserProfileTests")]

[CreateAssetMenu(fileName = "UserProfile", menuName = "UserProfile")]
public class UserProfile : ScriptableObject //TODO Move to base variable
{
    public event Action<UserProfile> OnUpdate;

    public string userName => model.name;
    public string email => model.email;
    public AvatarModel avatar => model.avatar;
    public string[] inventory => model.inventory;

    private Texture2D faceSnapshotValue = null;
    public Texture2D faceSnapshot => faceSnapshotValue;
    private Texture2D bodySnapshotValue;
    public Texture2D bodySnapshot => bodySnapshotValue;

    internal UserProfileModel model = new UserProfileModel() //Empty initialization to avoid nullchecks
    {
        avatar = new AvatarModel()
    };

    internal Coroutine downloadingFaceCoroutine = null;
    internal Coroutine downloadingBodyCoroutine = null;

    public void UpdateData(UserProfileModel newModel, bool downloadAssets = true)
    {
        UpdateProperties(newModel);

        if (downloadAssets)
        {
            DownloadFaceIfNeeded();
            DownloadBodyIfNeeded();
        }

        OnUpdate?.Invoke(this);
    }

    public bool ContainsItem(string itemId)
    {
        if (inventory == null)
            return false;

        return inventory.Contains(itemId);
    }

    internal void UpdateProperties(UserProfileModel newModel)
    {
        var currentFace = model.snapshots?.face;
        var currentBody = model.snapshots?.body;

        model.name = newModel?.name;
        model.email = newModel?.email;
        model.avatar.CopyFrom(newModel?.avatar);
        model.snapshots = newModel?.snapshots;
        model.inventory = newModel?.inventory;

        if (model.snapshots == null || model.snapshots.face != currentFace)
        {
            faceSnapshotValue = null;
        }

        if (model.snapshots == null || model.snapshots.body != currentBody)
        {
            faceSnapshotValue = null;
        }
    }

    internal void DownloadFaceIfNeeded()
    {
        if (faceSnapshot != null)
        {
            return;
        }

        if (downloadingFaceCoroutine != null)
        {
            CoroutineStarter.Stop(downloadingFaceCoroutine);
        }

        faceSnapshotValue = null;

        if (model == null || string.IsNullOrEmpty(model.snapshots?.face))
            return;

        downloadingFaceCoroutine = CoroutineStarter.Start(DownloadSnapshotCoroutine(model.snapshots.face, (x) => faceSnapshotValue = x));
    }

    internal void DownloadBodyIfNeeded()
    {
        if (bodySnapshot != null)
        {
            return;
        }

        if (downloadingBodyCoroutine != null)
        {
            CoroutineStarter.Stop(downloadingBodyCoroutine);
        }

        bodySnapshotValue = null;

        if (model == null || string.IsNullOrEmpty(model.snapshots?.body))
            return;

        downloadingBodyCoroutine = CoroutineStarter.Start(DownloadSnapshotCoroutine(model.snapshots.body, (x) => bodySnapshotValue = x));
    }

    private IEnumerator DownloadSnapshotCoroutine(string url, Action<Texture2D> successCallback)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            successCallback.Invoke(texture);
        }
        else
        {
            Debug.LogError(www.error);
        }

        OnUpdate?.Invoke(this);
    }

    public void OverrideAvatar(AvatarModel newModel, Texture2D faceSnapshot, Texture2D bodySnapshot)
    {
        model.avatar.CopyFrom(newModel);
        faceSnapshotValue = faceSnapshot;
        bodySnapshotValue = bodySnapshot;
        OnUpdate?.Invoke(this);
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
