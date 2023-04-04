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
            public int rows;
            public int cols;
        }

        public int version;
        public string origin;
        public string projectId;
        public Vector2IntRepresentantion point;
        public string rotation;
        public Layout layout;
        public bool isEmpty;
    }

    public class Vector2IntRepresentantion
    {
        public int x;
        public int y;

        public Vector2IntRepresentantion(Vector2Int vector2Int)
        {
            x = vector2Int.x;
            y = vector2Int.y;
        }
    }

    public Display display;
    public Contact contact;
    public Scene scene;
    public Source source;
    public SpawnPoint[] spawnPoints;
    public string owner;
    public string main;
    public string[] tags;
    public string[] requiredPermissions;
    public string[] allowedMediaHostnames;
}
