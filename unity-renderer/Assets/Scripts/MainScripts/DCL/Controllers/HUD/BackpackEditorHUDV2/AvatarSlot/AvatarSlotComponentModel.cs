using System;

namespace DCL.Backpack
{
    [Serializable]
    public record AvatarSlotComponentModel
    {
        public string rarity;
        public string imageUri;
        public string category;
        public bool isHidden;
        public string hiddenBy;
    }
}
