using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

[assembly: InternalsVisibleTo("UserProfileTests")]

[CreateAssetMenu(fileName = "UserProfile", menuName = "UserProfile")]
public class UserProfile : ScriptableObject
{
    public event Action<UserProfile> OnUpdate = (x) => { };

    public string userName => model.userName;
    public string mail => model.mail;
    public Sprite avatarPic => avatarPicTexture != null ? avatarPicTexture : defaultTexture;

    [SerializeField] private Sprite defaultTexture;

    internal UserProfileModel model = new UserProfileModel();
    internal Sprite avatarPicTexture = null;
    internal Coroutine downloadingCoroutine = null;

    public void UpdateData(UserProfileModel newModel)
    {
        UpdateProperties(newModel);
        DownloadIfNeeded();
        OnUpdate(this);
    }

    internal void UpdateProperties(UserProfileModel newModel)
    {
        model.userName = newModel.userName;
        model.mail = newModel.mail;

        if (model.avatarPicURL != newModel.avatarPicURL)
        {
            avatarPicTexture = null;
        }

        model.avatarPicURL = newModel.avatarPicURL;
    }

    internal void DownloadIfNeeded()
    {
        if (avatarPicTexture != null)
        {
            return;
        }

        if (downloadingCoroutine != null)
        {
            CoroutineStarter.Stop(downloadingCoroutine);
        }

        downloadingCoroutine = CoroutineStarter.Start(DownloadCoroutine());
    }

    private IEnumerator DownloadCoroutine()
    {
        avatarPicTexture = null;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(model.avatarPicURL);

        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            avatarPicTexture = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        else
        {
            Debug.LogError(www.error);
        }

        OnUpdate(this);
        downloadingCoroutine = null;
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

[Serializable]
public class UserProfileModel
{
    public string userName;
    public string mail;
    public string avatarPicURL;
}
