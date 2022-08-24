using System;
using UnityEngine;

namespace Variables.SpawnPoints
{
    [Serializable]
    public class SceneSpawnPoint
    {
        [Serializable]
        public class Position
        {
            public float[] x;
            public float[] y;
            public float[] z;
        }

        public string name;
        public Position position;
        public bool? @default;
        public Vector3? cameraTarget;
    }

    [Serializable]
    public class SceneSpawnPointsData
    {
        public SceneSpawnPoint[] spawnPoints;
        public bool? enabled;
    }
}