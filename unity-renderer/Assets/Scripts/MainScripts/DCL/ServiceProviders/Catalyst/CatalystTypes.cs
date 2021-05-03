using System;
using UnityEngine;

public static class CatalystEntitiesType
{
    public static readonly string SCENE = "scene";
    public static readonly string PROFILE = "profile";
    public static readonly string WEARABLE = "wearable";
}

[Serializable]
public class CatalystEntityContent
{
    public string file;
    public string hash;
}

[Serializable]
public class CatalystEntityBase
{
    public string id;
    public string type;
    public long timestamp;
    public string[] pointers;
    public CatalystEntityContent[] content;
}

[Serializable]
public class CatalystSceneEntityPayload : CatalystEntityBase
{
    public CatalystSceneEntityMetadata metadata;
}

[Serializable]
public class CatalystSceneEntityMetadata
{
    [Serializable]
    public class Display
    {
        public string title;
        public string description;
        public string navmapThumbnail;
    }
    
    [Serializable]
    public class Contact
    {
        public string name;
    }
    
    [Serializable]
    public class Scene
    {
        public string @base;
        public string[] parcels;
    }
    
    [Serializable]
    public class Policy
    {
        public string contentRating;
        public bool fly;
        public bool voiceEnabled;
        public string[] blacklist;
    }
    
    [Serializable]
    public class SpawnPoint
    {
        [Serializable]
        public class Vector3
        {
            public float x;
            public float y;
            public float z;
        }

        public string name;
        public bool @default;
        public Vector3 position;
        public Vector3 cameraTarget;
    }
    
    [Serializable]
    public class Source
    {
        [Serializable]
        public class Layout
        {
            public string rows;
            public string cols;
        }
        
        public int version;
        public string origin;
        public string projectId;
        public Vector2Int point;
        public string rotation;
        public Layout layout;
    }    

    public Display display;
    public Contact contact;
    public Scene scene;
    public Policy policy;
    public Source source;
    public SpawnPoint[] spawnPoints;
    public string owner;
    public string[] tags;
    public string[] requiredPermissions;
}