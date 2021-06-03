using System.Collections.Generic;
using UnityEngine;

namespace DCL.Interface
{
    [System.Serializable]
    public class CrashPayload
    {
        public static class DumpLiterals
        {
            public const string TOTAL_SCENE_LIMITS = "total-scenes-limit";
            public const string LOADED_SCENES = "loaded-scenes-dump";
            public const string POOL_MANAGER = "pool-manager-dump";
            public const string QUALITY_SETTINGS = "quality-settings-dump";
            public const string COMPONENTS = "components-dump";
            public const string GLTFS = "gltf-dump";
            public const string ASSET_BUNDLES = "asset-bundle-dump";
            public const string TEXTURES = "texture-dump";
            public const string TRAIL = "trail";
            public const string TELEPORTS = "teleports";
        }

        public Dictionary<string, object> fields = new Dictionary<string, object>();
    }
}