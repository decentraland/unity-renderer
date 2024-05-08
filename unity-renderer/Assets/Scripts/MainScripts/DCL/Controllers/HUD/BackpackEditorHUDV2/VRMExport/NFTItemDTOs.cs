using System;

namespace DCL.Backpack
{
    [Serializable]
    public struct NFTItemsDTO
    {
        public NFTDataDTO[] data;
    }

    [Serializable]
    public struct NFTDataDTO
    {
        public string name;
        public string thumbnail;
        public WearableDataDTO data;
        public string creator; // Creator wallet
        public string creatorName;
        public string creatorImageUrl;
        public string urn;
    }

    [Serializable]
    public struct WearableDataDTO
    {
        public WearableDTO wearable;
    }

    [Serializable]
    public struct WearableDTO
    {
        public string category;
    }
}
