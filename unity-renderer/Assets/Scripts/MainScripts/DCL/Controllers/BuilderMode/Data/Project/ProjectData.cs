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

        public int rows;
        public int colums;
    }
}