using System;
using UnityEngine;

namespace DCL.EmotesCustomization
{
    [Serializable]
    public class EmoteSlotCardComponentModel : BaseComponentModel
    {
        public string emoteId;
        public string emoteName;
        public Sprite pictureSprite;
        public string pictureUri;
        public bool isSelected = false;
        public int slotNumber = -1;
        public bool hasSeparator = true;
        public string rarity;
    }
}