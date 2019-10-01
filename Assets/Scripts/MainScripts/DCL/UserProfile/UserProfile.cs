using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DCL;
using UnityEngine;
using UnityEngine.Networking;

[assembly: InternalsVisibleTo("UserProfileTests")]

[CreateAssetMenu(fileName = "UserProfile", menuName = "UserProfile")]
public class UserProfile : ScriptableObject
{
    public event Action<UserProfile> OnUpdate = (x) => { };

    public string userName => model.name;
    public string email => model.email;
    public Sprite faceSnapshot => faceSnapshotSprite != null ? faceSnapshotSprite : defaultSprite;
    public Sprite bodySnapshot => bodySnapshotSprite != null ? bodySnapshotSprite : defaultSprite;

    [SerializeField] private Sprite defaultSprite;

    internal UserProfileModel model = new UserProfileModel() //Empty initialization to avoid nullchecks
    {
        avatar = new AvatarModel()
    };

    internal Sprite faceSnapshotSprite = null;
    internal Sprite bodySnapshotSprite = null;
    internal Coroutine downloadingFaceCoroutine = null;
    internal Coroutine downloadingBodyCoroutine = null;

    public void UpdateData(UserProfileModel newModel)
    {
        UpdateProperties(newModel);
        DownloadFaceIfNeeded();
        DownloadBodyIfNeeded();
        OnUpdate(this);
    }

    internal void UpdateProperties(UserProfileModel newModel)
    {
        var currentFace = model.snapshots?.face;
        var currentBody = model.snapshots?.body;

        model.name = newModel?.name;
        model.email = newModel?.email;
        model.avatar = newModel?.avatar;
        model.snapshots = newModel?.snapshots;

        if (model.snapshots == null || model.snapshots.face != currentFace)
        {
            faceSnapshotSprite = null;
        }

        if (model.snapshots == null || model.snapshots.body != currentBody)
        {
            faceSnapshotSprite = null;
        }
    }

    internal void DownloadFaceIfNeeded()
    {
        if (faceSnapshotSprite != null)
        {
            return;
        }

        if (downloadingFaceCoroutine != null)
        {
            CoroutineStarter.Stop(downloadingFaceCoroutine);
        }

        faceSnapshotSprite = null;

        if (model == null || string.IsNullOrEmpty(model.snapshots?.face)) 
            return;

        downloadingFaceCoroutine = CoroutineStarter.Start(DownloadSnapshotCoroutine(model.snapshots.face, (x) => faceSnapshotSprite = x));
    }

    internal void DownloadBodyIfNeeded()
    {
        if (bodySnapshotSprite != null)
        {
            return;
        }

        if (downloadingBodyCoroutine != null)
        {
            CoroutineStarter.Stop(downloadingBodyCoroutine);
        }

        bodySnapshotSprite = null;

        if (model == null || string.IsNullOrEmpty(model.snapshots?.body)) 
            return;

        downloadingBodyCoroutine = CoroutineStarter.Start(DownloadSnapshotCoroutine(model.snapshots.body, (x) => bodySnapshotSprite = x));
    }

    private IEnumerator DownloadSnapshotCoroutine(string url, Action<Sprite> successCallback)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            successCallback.Invoke(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
        }
        else
        {
            Debug.LogError(www.error);
        }

        OnUpdate(this);
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
}