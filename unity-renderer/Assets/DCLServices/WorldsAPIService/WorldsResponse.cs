using DCLServices.Lambdas;
using MainScripts.DCL.Controllers.HotScenes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DCLServices.WorldsAPIService
{
    public static class WorldsResponse
    {
        [Serializable]
        public class WorldInfo : ISerializationCallbackReceiver
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

        [Serializable]
        public class WorldsAPIResponse : PaginatedResponse
        {
            public bool ok;
            public int total;
            public List<WorldInfo> data;
        }
    }
}
