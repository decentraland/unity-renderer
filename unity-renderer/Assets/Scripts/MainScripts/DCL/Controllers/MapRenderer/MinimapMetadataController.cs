using DCL.Helpers;
using UnityEngine;

public class MinimapMetadataController : MonoBehaviour
{
    private MinimapMetadata minimapMetadata => MinimapMetadata.GetMetadata();
    public static MinimapMetadataController i { get; private set; }

    public void Awake()
    {
        i = this;
        minimapMetadata.Clear();
    }

    public void UpdateMinimapSceneInformation(string scenesInfoJson)
    {
        var scenesInfo = Utils.ParseJsonArray<MinimapMetadata.MinimapSceneInfo[]>(scenesInfoJson);

        foreach (var sceneInfo in scenesInfo)
        {
            minimapMetadata.AddSceneInfo(sceneInfo);
        }
    }

    /// <summary>
    /// Updates the information of an user in the minimap.
    /// </summary>
    /// <param name="userInfo">User info model</param>
    /// <param name="isRemoved">True for remove the user info</param>
    public void UpdateMinimapUserInformation(MinimapMetadata.MinimapUserInfo userInfo, bool isRemoved = false)
    {
        if (string.IsNullOrEmpty(userInfo.userId))
            return;

        if (!isRemoved)
            minimapMetadata.AddOrUpdateUserInfo(userInfo);
        else
            minimapMetadata.RemoveUserInfo(userInfo.userId);
    }
}
