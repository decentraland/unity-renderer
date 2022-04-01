using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public class PublishInfo
    {
        public enum ProjectRotation
        {
            NORTH = 0,
            EAST = 1,
            SOUTH = 2,
            WEST = 3
        }

        public ProjectRotation rotation;
        public Vector2Int coordsToPublish;
    }
}