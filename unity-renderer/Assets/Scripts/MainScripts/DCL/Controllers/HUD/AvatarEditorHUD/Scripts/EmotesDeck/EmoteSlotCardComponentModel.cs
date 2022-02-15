using System;
using UnityEngine;

namespace EmotesDeck
{
    [Serializable]
    public class EmoteSlotCardComponentModel : BaseComponentModel
    {
        public string emoteId;
        public Sprite pictureSprite;
        public string pictureUri;
        public bool isSelected = false;
        public int slotNumber = -1;
    }
}