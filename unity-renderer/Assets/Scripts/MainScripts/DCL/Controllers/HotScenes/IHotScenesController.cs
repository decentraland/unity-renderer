﻿using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.Lambdas;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HotScenes
{
    /// <summary>
    /// Despite this service is called `HotScenesController` it is responsible to fetch users outside of comms region,
    /// so it is essentially `ColdScenesController`
    /// </summary>
    public interface IHotScenesController : IService
    {
        UniTask<IReadOnlyList<HotSceneInfo>> GetHotScenesListAsync(CancellationToken cancellationToken);

        [Serializable]
        class HotSceneInfo
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
        public class PlacesAPIResponse : PaginatedResponse
        {
            public bool ok;
            public int total;
            public List<PlaceInfo> data;
        }

        [Serializable]
        public class PlacesAPIGetParcelResponse
        {
            public bool ok;
            public PlaceInfo data;
        }
    }
}
