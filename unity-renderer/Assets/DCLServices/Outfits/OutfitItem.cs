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
        public ElementColor eyes;
        public ElementColor hair;
        public ElementColor skin;
        public string[] wearables;
    }

    [Serializable]
    public class ElementColor
    {
        public Color color;
    }
}
