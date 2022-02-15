using System;
using UnityEngine;

namespace EmotesDeck
{
    [Serializable]
    internal class EmoteSlotCardComponentModel : BaseComponentModel
    {
        public string emoteId;
        public Sprite pictureSprite;
        public string pictureUri;
        public bool isSelected = false;
        public int slotNumber = -1;
    }
}