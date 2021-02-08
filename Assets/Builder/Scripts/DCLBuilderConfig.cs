using System;
using UnityEngine;

namespace Builder
{
    public static class DCLBuilderConfig
    {
        public static event Action<BuilderConfig> OnConfigChanged;
        public static BuilderConfig config { get; private set; } = BuilderConfig.DefaultBuilderConfig;

        public static void SetConfig(BuilderConfig newBuilderConfig)
        {
            config = newBuilderConfig;
            OnConfigChanged?.Invoke(config);
        }

        public static void SetConfig(string configJson)
        {
            try
            {
                JsonUtility.FromJsonOverwrite(configJson, config);
                SetConfig(config.Clone());
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting builder's configuration {e.Message}");
            }
        }
    }

    [Serializable]
    public class BuilderConfig
    {
        [Serializable]
        public class Camera
        {
            public float zoomMin;
            public float zoomMax;
            public float zoomDefault;
        }

        [Serializable]
        public class Environment
        {
            public bool disableFloor;
        }

        public static BuilderConfig DefaultBuilderConfig
        {
            get
            {
                return new BuilderConfig()
                {
                    camera = new Camera() { zoomMin = 1f, zoomMax = 100f, zoomDefault = 32 },
                    environment = new Environment() { disableFloor = false }
                };
            }
        }

        public BuilderConfig Clone() => (BuilderConfig)MemberwiseClone();

        public Camera camera;
        public Environment environment;
    }
}