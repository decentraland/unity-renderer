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
    [Serializable]
    public class Model
    {
        public string userId;
        public string name;
        public string email;
        public string description;
        public ulong created_at;
        public ulong updated_at;
        public string version;
        public AvatarShape.Model avatar;
    }

    public event Action<UserProfile> OnUpdate = (x) => { };

    public string userName => model.name;
    public string email => model.email;
    public Sprite faceSnapshot => faceSnapshotSprite != null ? faceSnapshotSprite : defaultSprite;
    public Sprite bodySnapshot => bodySnapshotSprite != null ? bodySnapshotSprite : defaultSprite;

    [SerializeField] private Sprite defaultSprite;

    internal Model model = new Model() //Empty initialization to avoid nullchecks
    {
        avatar = new AvatarShape.Model()
        {
            snapshots = new AvatarShape.Model.Snapshots()
        }
    };
    internal Sprite faceSnapshotSprite = null;
    internal Sprite bodySnapshotSprite = null;
    internal Coroutine downloadingFaceCoroutine = null;
    internal Coroutine downloadingBodyCoroutine = null;

    public void UpdateData(Model newModel)
    {
        UpdateProperties(newModel);
        DownloadFaceIfNeeded();
        DownloadBodyIfNeeded();
        OnUpdate(this);
    }

    internal void UpdateProperties(Model newModel)
    {
        model.name = newModel.name;
        model.email = newModel.email;

        if (model.avatar.snapshots.face != newModel.avatar.snapshots.face)
        {
            faceSnapshotSprite = null;
        }
        model.avatar.snapshots.face = newModel.avatar.snapshots.face;

        if (model.avatar.snapshots.body != newModel.avatar.snapshots.body)
        {
            bodySnapshotSprite = null;
        }
        model.avatar.snapshots.body = newModel.avatar.snapshots.body;
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
        downloadingFaceCoroutine = CoroutineStarter.Start(DownloadSnapshotCoroutine(model.avatar.snapshots.face, (x) => faceSnapshotSprite = x));
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
        downloadingBodyCoroutine = CoroutineStarter.Start(DownloadSnapshotCoroutine(model.avatar.snapshots.body, (x) => bodySnapshotSprite = x));
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
