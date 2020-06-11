using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinimapMetadata", menuName = "MinimapMetadata")]
public class MinimapMetadata : ScriptableObject
{
    public enum TileType
    {
        MyParcel = 0,
        MyParcelsOnSale = 1,
        MyEstates = 2,
        MyEstatesOnSale = 3,
        WithAccess = 4,
        District = 5,
        Contribution = 6,
        Roads = 7,
        Plaza = 8,
        Taken = 9,
        OnSale = 10,
        Unowned = 11,
        Background = 12,
        Loading = 13
    }

    [Serializable]
    public class MinimapSceneInfo
    {
        public string name;
        public TileType type;
        public List<Vector2Int> parcels;

        public bool isPOI;
        public string owner;
        public string description;
        public string previewImageUrl;
    }

    [Serializable]
    public class MinimapUserInfo
    {
        public string userId;
        public string userName;
        public Vector3 worldPosition;
    }

    public event Action<MinimapSceneInfo> OnSceneInfoUpdated;
    public event Action<MinimapUserInfo> OnUserInfoUpdated;
    public event Action<string> OnUserInfoRemoved;

    HashSet<MinimapSceneInfo> scenesInfo = new HashSet<MinimapSceneInfo>();
    Dictionary<Vector2Int, MinimapSceneInfo> sceneInfoMap = new Dictionary<Vector2Int, MinimapSceneInfo>();
    Dictionary<string, MinimapUserInfo> usersInfo = new Dictionary<string, MinimapUserInfo>();

    public MinimapSceneInfo GetSceneInfo(int x, int y)
    {
        if (sceneInfoMap.TryGetValue(new Vector2Int(x, y), out MinimapSceneInfo result))
            return result;

        return null;
    }

    public void AddSceneInfo(MinimapSceneInfo sceneInfo)
    {
        if (scenesInfo.Contains(sceneInfo))
            return;

        int parcelsCount = sceneInfo.parcels.Count;

        for (int i = 0; i < parcelsCount; i++)
        {
            if (sceneInfoMap.ContainsKey(sceneInfo.parcels[i]))
            {
                //NOTE(Brian): I intended at first to just return; here. But turns out kernel is sending
                //             overlapping coordinates, sending first gigantic 'Estate' and 'Roads' scenes to
                //             send the proper scenes later. This will be fixed when we implement the content v3 data
                //             plumbing.
                sceneInfoMap.Remove(sceneInfo.parcels[i]);
            }

            sceneInfoMap.Add(sceneInfo.parcels[i], sceneInfo);
        }

        scenesInfo.Add(sceneInfo);

        OnSceneInfoUpdated?.Invoke(sceneInfo);
    }

    /// <summary>
    /// Adds (or updates) the information of an user in the minimap.
    /// </summary>
    /// <param name="userInfo">User info model</param>
    public void AddOrUpdateUserInfo(MinimapUserInfo userInfo)
    {
        if (usersInfo.TryGetValue(userInfo.userId, out MinimapUserInfo existingUserInfo))
            existingUserInfo = userInfo;
        else
            usersInfo.Add(userInfo.userId, userInfo);

        OnUserInfoUpdated?.Invoke(userInfo);
    }

    /// <summary>
    /// Removes the information of an user from the minimap.
    /// </summary>
    /// <param name="userId">User Id</param>
    public void RemoveUserInfo(string userId)
    {
        if (usersInfo.Remove(userId))
            OnUserInfoRemoved?.Invoke(userId);
    }

    public void Clear()
    {
        scenesInfo.Clear();
        sceneInfoMap.Clear();
    }

    private static MinimapMetadata minimapMetadata;
    public static MinimapMetadata GetMetadata()
    {
        if (minimapMetadata == null)
        {
            minimapMetadata = Resources.Load<MinimapMetadata>("ScriptableObjects/MinimapMetadata");
        }

        return minimapMetadata;
    }
}
