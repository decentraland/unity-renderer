using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainScripts.DCL.ServiceProviders.OpenSea.Interfaces
{
    public struct OwnNFTInfo
    {
        public string ethAddress;
        public List<NFTInfo> assets;

        public static OwnNFTInfo Default
        {
            get
            {
                var ret = new OwnNFTInfo();
                ret.ethAddress = "0x0000000000000000000000000000000000000000";
                ret.assets = new List<NFTInfo>();
                return ret;
            }
        }
    }

    [Serializable]
    public struct NFTOwner
    {
        public string address;
        public int quantity;
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
        public Color? backgroundColor;
        public MarketInfo? marketInfo;
        public string currentPrice;
        public PaymentTokenInfo? currentPriceToken;
        public AssetContract assetContract;
        public NFTOwner[] owners;

        public static NFTInfo Default
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

        public bool Equals(string assetContract, string tokenId) =>
            this.assetContract.address.ToLower() == assetContract.ToLower()
            && this.tokenId == tokenId;
    }

    [Serializable]
    public struct PaymentTokenInfo
    {
        public string symbol;
    }

    [Serializable]
    public struct MarketInfo
    {
        public string name;
    }

    [Serializable]
    public struct AssetContract
    {
        public string address;
        public string name;
    }
}
