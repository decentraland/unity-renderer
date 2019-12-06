using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Models
{
    [Serializable]
    public struct CreateUISceneMessage
    {
        public string id;
        public string baseUrl;
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
            public string id;
            public string baseUrl;
            public string baseUrlBundles;

            public List<ContentServerUtils.MappingPair> contents;

            public Vector2Int basePosition;
            public Vector2Int[] parcels;
        }
    }
}
