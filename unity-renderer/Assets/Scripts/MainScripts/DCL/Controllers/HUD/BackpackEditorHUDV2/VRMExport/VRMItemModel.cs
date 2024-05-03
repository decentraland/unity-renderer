using System;

namespace DCL.Backpack
{
    [Serializable]
    public record VRMItemModel
    {
        public string wearableUrn;
        public string wearableImageUrl;
        public string wearableName;
        public string wearableCategoryName;
        public string wearableCreatorName;
        public string wearableCreatorImageUrl;
    }
}
