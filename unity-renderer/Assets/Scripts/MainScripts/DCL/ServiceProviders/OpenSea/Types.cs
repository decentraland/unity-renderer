﻿using System;

namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    [Serializable]
    public class AssetsResponse
    {
        public AssetResponse[] assets;
    }

    [Serializable]
    public class AssetResponse
    {
        public string token_id;
        public long? num_sales = null;
        public string background_color;
        public string image_url;
        public string image_preview_url;
        public string image_thumbnail_url;
        public string image_original_url;
        public string name;
        public string description;
        public string external_link;
        public AssetContract asset_contract = null;
        public AccountInfo owner = null;
        public string permalink;
        public AssetSaleInfo last_sale = null;
        public OrderInfo[] sell_orders = null;
    }

    [Serializable]
    public class SingleAssetResponse : AssetResponse
    {
        public OrderInfo[] orders;
        public OwnershipInfo[] top_ownerships;
    }

    [Serializable]
    public class AssetContract
    {
        public string address;
        public string asset_contract_type;
        public string created_date;
        public string name;
        public string nft_version;
        public long? owner = null;
        public string schema_name;
        public string symbol;
        public string total_supply = null;
        public string description;
        public string external_link;
        public string image_url;
    }

    [Serializable]
    public class AccountInfo
    {
        public string profile_img_url;
        public string address;
    }

    [Serializable]
    public class AssetSaleInfo
    {
        public string event_type;
        public string event_timestamp;
        public string total_price;
        public PaymentTokenInfo payment_token = null;
        public TransactionInfo transaction = null;
    }

    [Serializable]
    public class PaymentTokenInfo
    {
        public long id;
        public string symbol;
        public string address;
        public string image_url;
        public string name;
        public int decimals;
        public string eth_price;
        public string usd_price;
    }

    [Serializable]
    public class TransactionInfo
    {
        public long id;
        public AccountInfo from_account;
        public AccountInfo to_account;
        public string transaction_hash;
    }

    [Serializable]
    public class OrderInfo
    {
        public AccountInfo maker;
        public string current_price;
        public PaymentTokenInfo payment_token_contract;
    }

    [Serializable]
    public class OwnershipInfo
    {
        public AccountInfo owner;
        public string quantity;
    }
}