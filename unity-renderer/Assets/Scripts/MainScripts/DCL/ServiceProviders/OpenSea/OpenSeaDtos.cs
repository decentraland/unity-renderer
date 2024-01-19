using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;
using System;

namespace MainScripts.DCL.ServiceProviders.OpenSea
{
    [Serializable]
    public class OpenSeaManyNftDto
    {
        public NftDto[] nfts;
    }

    [Serializable]
    public class OpenSeaNftDto
    {
        public NftDto nft;
    }

    [Serializable]
    public class NftDto
    {
        public string identifier;
        public string collection;
        public string image_url;
        public string name;
        public string description;
        public string opensea_url;
        public string contract;
        public NFTOwner[] owners;
    }
}
