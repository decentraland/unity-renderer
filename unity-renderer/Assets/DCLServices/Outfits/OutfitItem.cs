using System;
using UnityEngine;

[Serializable]
public class OutfitItem
{
    public int slot;
    public Outfit outfit;

    [Serializable]
    public class Outfit
    {
        public string bodyShape;
        public Color eyes;
        public Color hair;
        public Color skin;
        public string[] wearables;
    }
}
