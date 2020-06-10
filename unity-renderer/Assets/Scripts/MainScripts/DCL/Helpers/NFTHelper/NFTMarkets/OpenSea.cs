using System;
using System.Collections;
using System.Linq;
using UnityEngine.Networking;

namespace DCL.Helpers.NFT.Markets
{
    public class OpenSea : INFTMarket
    {
        const string API_URL_SINGLE_ASSET = "https://api.opensea.io/api/v1/asset/{0}/{1}/";

        MarketInfo openSeaMarketInfo = new MarketInfo() { name = "OpenSea" };

        IEnumerator INFTMarket.FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError)
        {
            string url = string.Format(API_URL_SINGLE_ASSET, assetContractAddress, tokenId);
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (!request.isNetworkError && !request.isHttpError)
                {
                    AssetResponse response = Utils.FromJsonWithNulls<AssetResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(ResponseToNFTInfo(response));
                }
                else
                {
                    onError?.Invoke($"{openSeaMarketInfo.name} error fetching {assetContractAddress}/{tokenId} {request.error}");
                }
            }
        }

        private NFTInfo ResponseToNFTInfo(AssetResponse response)
        {
            NFTInfo ret = NFTInfo.defaultNFTInfo;
            ret.marketInfo = openSeaMarketInfo;
            ret.name = response.name;
            ret.description = response.description;
            ret.thumbnailUrl = response.image_thumbnail_url;
            ret.previewImageUrl = response.image_preview_url;
            ret.originalImageUrl = response.image_original_url;
            ret.assetLink = response.external_link;
            ret.marketLink = response.permalink;

            if (!string.IsNullOrEmpty(response.owner?.address))
            {
                ret.owner = response.owner.address;
            }

            if (response.num_sales != null)
            {
                ret.numSales = response.num_sales.Value;
            }

            if (response.last_sale != null)
            {
                ret.lastSaleDate = response.last_sale.event_timestamp;

                if (response.last_sale.payment_token != null)
                {
                    ret.lastSaleAmount = PriceToFloatingPointString(response.last_sale);
                    ret.lastSaleToken = new NFT.PaymentTokenInfo()
                    {
                        symbol = response.last_sale.payment_token.symbol
                    };
                }
            }

            UnityEngine.Color backgroundColor;
            if (UnityEngine.ColorUtility.TryParseHtmlString("#" + response.background_color, out backgroundColor))
            {
                ret.backgroundColor = backgroundColor;
            }

            OrderInfo sellOrder = GetSellOrder(response.orders, response.owner.address);
            if (sellOrder != null)
            {
                ret.currentPrice = PriceToFloatingPointString(sellOrder.current_price, sellOrder.payment_token_contract);
                ret.currentPriceToken = new NFT.PaymentTokenInfo()
                {
                    symbol = sellOrder.payment_token_contract.symbol
                };
            }

            return ret;
        }

        private string PriceToFloatingPointString(AssetSaleInfo saleInfo)
        {
            if (saleInfo.payment_token == null) return null;
            return PriceToFloatingPointString(saleInfo.total_price, saleInfo.payment_token);
        }

        private string PriceToFloatingPointString(string price, PaymentTokenInfo tokenInfo)
        {
            string priceString = price;
            if (price.Contains('.'))
            {
                priceString = price.Split('.')[0];
            }
            int pointPosition = priceString.Length - tokenInfo.decimals;
            if (pointPosition <= 0)
            {
                return "0." + string.Concat(Enumerable.Repeat("0", Math.Abs(pointPosition))) + priceString;
            }
            else
            {
                return priceString.Insert(pointPosition, ".");
            }
        }

        private OrderInfo GetSellOrder(OrderInfo[] orders, string nftOwner)
        {
            OrderInfo ret = null;
            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i].maker.address == nftOwner)
                {
                    ret = orders[i];
                    break;
                }
            }
            return ret;
        }

        [Serializable]
        class AssetResponse
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
            public OrderInfo[] orders = null;
        }

        [Serializable]
        class AssetContract
        {
            public string address;
            public string asset_contract_type;
            public string created_date;
            public string name;
            public string nft_version;
            public long? owner = null;
            public string schema_name;
            public string symbol;
            public long? total_supply = null;
            public string description;
            public string external_link;
            public string image_url;
        }

        [Serializable]
        class AccountInfo
        {
            public string profile_img_url;
            public string address;
        }

        [Serializable]
        class AssetSaleInfo
        {
            public string event_type;
            public string event_timestamp;
            public string total_price;
            public PaymentTokenInfo payment_token = null;
            public TransactionInfo transaction = null;
        }

        [Serializable]
        class PaymentTokenInfo
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
        class TransactionInfo
        {
            public long id;
            public AccountInfo from_account;
            public AccountInfo to_account;
            public string transaction_hash;
        }

        [Serializable]
        class OrderInfo
        {
            public AccountInfo maker;
            public string current_price;
            public PaymentTokenInfo payment_token_contract;
        }
    }
}