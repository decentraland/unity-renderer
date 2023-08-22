using DCL.Controllers.LoadingScreenV2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Models
{
    public static class ScenePermissionNames
    {
        public const string ALLOW_MEDIA_HOSTNAMES = "ALLOW_MEDIA_HOSTNAMES";
        public const string OPEN_EXTERNAL_LINK = "OPEN_EXTERNAL_LINK";
    }

    [Serializable]
    public struct CreateGlobalSceneMessage
    {
        public string id;
        public int sceneNumber;
        public string name;
        public string description;
        public string baseUrl;
        public List<ContentServerUtils.MappingPair> contents;
        public string icon;
        public bool isPortableExperience;
        public bool sdk7;
        public string[] requiredPermissions;
        public string[] allowedMediaHostnames;
        public List<Hint> loadingScreenHints;
    }

    [Serializable]
    public class LoadParcelScenesMessage
    {
        public List<UnityParcelScene> parcelsToLoad = new List<UnityParcelScene>();

        /**
         * This class is received as an element of the list of parcels that need to be visible
         */
        [Serializable]
        public class UnityParcelScene
        {
            public static bool VERBOSE = false;

            // We can't remove this string id since it's used for mapping portable experience ids to their wearables and fetching scene asset bundles manifest
            public string id;

            public int sceneNumber;
            public string baseUrl;
            public string baseUrlBundles;
            public string description;
            public string iconUrl;

            public List<ContentServerUtils.MappingPair> contents;

            public Vector2Int basePosition;
            public Vector2Int[] parcels;

            // Indicates if it's a sdk7 scene
            public bool sdk7 = false;
            public string[] requiredPermissions;
            public string[] allowedMediaHostnames;

            public List<Hint> loadingScreenHints;
            public ScenePortableExperienceFeatureToggles scenePortableExperienceFeatureToggles;
        }
    }

    public enum ScenePortableExperienceFeatureToggles
    {
        Enable,
        Disable,
        HideUi
    }
}
