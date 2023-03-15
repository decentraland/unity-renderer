using System;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

public class HotScenesController : MonoBehaviour
{
    public static HotScenesController i { get; private set; }

    public event Action OnHotSceneListFinishUpdating;
    public event Action OnHotSceneListChunkUpdated;

    public List<HotSceneInfo> hotScenesList { get; private set; } = new List<HotSceneInfo>();
    public bool isUpdating { get; private set; }
    public float timeSinceLastUpdate { get { return Time.realtimeSinceStartup - lastUpdateTime; } }

    private float lastUpdateTime = float.MinValue * .5f;

    [Serializable]
    public class HotSceneInfo
    {
        [Serializable]
        public class Realm
        {
            public string serverName;
            public string layer;
            public int usersCount;
            public int maxUsers;
            public Vector2Int[] userParcels;
        }

        public string id;
        public string name;
        public string creator;
        public string description;
        public string thumbnail;
        public Vector2Int baseCoords;
        public Vector2Int[] parcels;
        public int usersTotalCount;
        public Realm[] realms;
    }

    [Serializable]
    public class PlacesAPIResponse
    {
        public bool ok;
        public int total;
        public List<PlaceInfo> data;
    }

    [Serializable]
    public class PlaceInfo
    {
        [Serializable]
        public class Realm
        {
            public string serverName;
            public string layer;
            public string url;
            public int usersCount;
            public int maxUsers;
            public Vector2Int[] userParcels;
        }
        public string id;
        public string title;
        public string description;
        public string image;
        public string owner;
        public string[] tags;
        public Vector2Int[] positions;
        public string base_position;
        public string contact_name;
        public string contact_email;
        public string content_rating;
        public bool disabled;
        public string disabled_at;
        public string created_at;
        public string updated_at;
        public int favorites;
        public int likes;
        public int dislikes;
        public string[] categories;
        public int like_rate;
        public bool highlighted;
        public string highlighted_image;
        public bool featured;
        public string featured_image;
        public bool user_favorite;
        public bool user_like;
        public bool user_dislike;
        public int user_count;
        public int user_visits;
        public Realm[] realms_detail;
    }

    [Serializable]
    internal struct HotScenesUpdatePayload
    {
        public int chunkIndex;
        public int chunksCount;
        public HotSceneInfo[] scenesInfo;
    }

    void Awake() { i = this; }

    public void UpdateHotScenesList(string json)
    {
        var updatePayload = Utils.SafeFromJson<HotScenesUpdatePayload>(json);

        if (updatePayload.chunkIndex == 0)
        {
            isUpdating = true;
            hotScenesList.Clear();
        }

        hotScenesList.AddRange(updatePayload.scenesInfo);
        OnHotSceneListChunkUpdated?.Invoke();

        if (updatePayload.chunkIndex >= updatePayload.chunksCount - 1)
        {
            isUpdating = false;
            lastUpdateTime = Time.realtimeSinceStartup;
            OnHotSceneListFinishUpdating?.Invoke();
        }
    }
}
