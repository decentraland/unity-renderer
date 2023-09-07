using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.Lambdas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        UniTask<IReadOnlyList<HotWorldInfo.WorldInfo>> GetHotWorldsListAsync(CancellationToken cancellationToken);

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
        class HotWorldInfo
        {
            [Serializable]
            public class WorldInfo
            {
                public string worldName;
                public int users;
            }

            [Serializable]
            public class WorldData
            {
                public int totalUsers;
                public WorldInfo[] perWorld;
            }

            public WorldData data;
            public string lastUpdated;

        }

        // TODO: This class should be moved to the PlacesAPIService folder
        [Serializable]
        public class PlaceInfo : ISerializationCallbackReceiver
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
            [SerializeField] private string[] positions;
            public string world_name;

            public Vector2Int[] Positions;

            public string base_position;
            public string contact_name;
            public string contact_email;
            public string content_rating;
            public bool disabled;
            public string disabled_at;
            public string created_at;
            public string updated_at;
            public string deployed_at;
            public int favorites;
            public int likes;
            public int dislikes;
            public string[] categories;
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

            public string like_rate;

            [JsonIgnore]
            public float? like_rate_as_float
            {
                get
                {
                    if (string.IsNullOrEmpty(like_rate))
                        return null;

                    if (float.TryParse(like_rate, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                        return result;

                    return null;
                }
            }

            public void OnBeforeSerialize()
            {
                if (Positions == null)
                {
                    positions = null;
                    return;
                }

                positions = new string[Positions.Length];
                for (int i = 0; i < Positions.Length; i++)
                    positions[i] = $"{Positions[i].x},{Positions[i].y}";
            }

            public void OnAfterDeserialize()
            {
                if (positions == null)
                    return;
                Positions = new Vector2Int[positions.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    string[] split = positions[i].Split(',');
                    Positions[i] = new Vector2Int(int.Parse(split[0]), int.Parse(split[1]));
                }
            }
        }

        // TODO: This class should be moved to the PlacesAPIService folder
        [Serializable]
        public class PlacesAPIResponse : PaginatedResponse
        {
            public bool ok;
            public int total;
            public List<PlaceInfo> data;
        }

        // TODO: This class should be moved to the PlacesAPIService folder
        [Serializable]
        public class PlacesAPIGetParcelResponse
        {
            public bool ok;
            public PlaceInfo data;
        }
    }
}
