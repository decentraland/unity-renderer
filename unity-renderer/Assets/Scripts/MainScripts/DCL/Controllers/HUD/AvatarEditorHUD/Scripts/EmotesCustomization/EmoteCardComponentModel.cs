using System;
using UnityEngine;

namespace Emotes
{
    [Serializable]
    public class EmoteCardComponentModel : BaseComponentModel
    {
        public string id;
        public string name;
        public string description;
        public Sprite pictureSprite;
        public string pictureUri;
        public bool isAssignedInSelectedSlot = false;
        public bool isSelected = false;
        public int assignedSlot = -1;
        public string rarity;
        public bool isInL2;
    }

    [Serializable]
    public class EmoteRarity
    {
        public string rarity;
        public Color markColor;
    }
}