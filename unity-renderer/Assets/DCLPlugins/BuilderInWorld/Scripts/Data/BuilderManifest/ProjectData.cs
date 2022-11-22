using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL.Builder
{
    [Serializable]
    public class ProjectData
    {
        public string id;
        public string title;
        public string description;

        public bool is_public = false;
        [FormerlySerializedAs("scene_id")] public int scene_number;
        
        public string eth_address;
        
        [JsonProperty("thumbnail", NullValueHandling=NullValueHandling.Ignore)]
        public string thumbnail;


        public int rows;
        public int cols;

        public DateTime created_at;
        public DateTime updated_at;

        [JsonProperty("creation_coords", NullValueHandling=NullValueHandling.Ignore)]
        public string creation_coords;

    }
}