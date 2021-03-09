using System.Collections.Generic;

namespace DCL.Helpers.NFT
{
    public struct NFTOwner
    {
        public string ethAddress;
        public List<NFTInfo> assets;

        static public NFTOwner defaultNFTOwner
        {
            get
            {
                NFTOwner ret = new NFTOwner();
                ret.ethAddress = "0x0000000000000000000000000000000000000000";
                ret.assets = new List<NFTInfo>();
                return ret;
            }
        }
    }

    public struct NFTInfo
    {
        public string name;
        public string tokenId;
        public string description;
        public string owner;
        public long numSales;
        public string imageUrl;
        public string thumbnailUrl;
        public string previewImageUrl;
        public string originalImageUrl;
        public string assetLink;
        public string marketLink;
        public string lastSaleDate;
        public string lastSaleAmount;
        public PaymentTokenInfo? lastSaleToken;
        public UnityEngine.Color? backgroundColor;
        public MarketInfo? marketInfo;
        public string currentPrice;
        public PaymentTokenInfo? currentPriceToken;
        public AssetContract assetContract;

        static public NFTInfo defaultNFTInfo
        {
            get
            {
                NFTInfo ret = new NFTInfo();
                ret.owner = "0x0000000000000000000000000000000000000000";
                ret.numSales = 0;
                ret.assetContract = new AssetContract();
                return ret;
            }
        }
    }

    public struct PaymentTokenInfo
    {
        public string symbol;
    }

    public struct MarketInfo
    {
        public string name;
    }

    public struct AssetContract
    {
        public string address;
        public string name;
    }
}