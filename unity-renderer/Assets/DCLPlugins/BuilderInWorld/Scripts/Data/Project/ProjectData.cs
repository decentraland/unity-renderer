using System;
using Newtonsoft.Json;
using UnityEngine;

namespace DCL.Builder
{
    [Serializable]
    public class ProjectData
    {
        public string id;
        public string title;
        public string description;

        public bool is_public = false;
        public string scene_id;
        
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