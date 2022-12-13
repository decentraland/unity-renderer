using DCL.Components;
using DCL.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneObject
{
    [System.Serializable]
    public class ObjectMetrics
    {
        public int meshes;
        public int bodies;
        public int materials;
        public int textures;
        public int triangles;
        public int entities;
    }

    public string id;
    public string model;
    public Dictionary<string, string> contents;
    
}