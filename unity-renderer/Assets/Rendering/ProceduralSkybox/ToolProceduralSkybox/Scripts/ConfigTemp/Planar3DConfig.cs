using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class Planar3DConfig
    {
        public bool enabled;
        public string nameInEditor;
        public LayerRenderType renderType;
        public float timeSpan_End;
        public float timeSpan_start;
        public float fadeInTime;
        public float fadeOutTime;
        public GameObject prefab;
        public float satelliteSize;
        public float radius;

        public Planar3DConfig(string name) { this.nameInEditor = name; }
    }
}