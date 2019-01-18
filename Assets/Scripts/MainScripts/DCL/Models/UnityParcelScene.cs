using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Models
{

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
            public string id;

            public Vector2Int basePosition;
            public Vector2Int[] parcels;

            public List<ContentMapping> contents;
            public string baseUrl;

            [Serializable]
            public class ContentMapping
            {
                public string file;
                public string hash;
            }
        }
    }
}
