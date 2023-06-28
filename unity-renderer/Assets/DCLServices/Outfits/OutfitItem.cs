using System;
using UnityEngine;

namespace DCL.Backpack
{
    [Serializable]
    public class OutfitItem
    {
        public int slot;
        public Outfit outfit;

        [Serializable]
        public class Outfit
        {
            public string bodyShape;
            public eyes eyes;
            public hair hair;
            public skin skin;
            public string[] wearables;
            public string[] forceRender;
        }

        [Serializable]
        public class eyes
        {
            public Color color;
        }

        [Serializable]
        public class hair
        {
            public Color color;
        }

        [Serializable]
        public class skin
        {
            public Color color;
        }
    }
}
