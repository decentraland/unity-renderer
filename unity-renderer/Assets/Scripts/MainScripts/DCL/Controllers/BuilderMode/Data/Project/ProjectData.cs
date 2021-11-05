using System;
using UnityEngine;

namespace DCL.Builder
{
    [Serializable]
    public class ProjectData
    {
        public string id;
        public string title;
        public string description;

        public string thumbnail;
        public string scene_id;

        public int rows;
        public int colums;

        public DateTime created_at;
        public DateTime updated_at;
        
    }
}