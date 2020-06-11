using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers.NFT.Markets.OpenSea_Internal;

namespace DCL.Helpers.NFT.Markets
{
    internal class OpenSea : INFTMarket
    {
        MarketInfo openSeaMarketInfo = new MarketInfo() { name = "OpenSea" };
        OpenSeaRequestController requestController = new OpenSeaRequestController();
        Dictionary<string, NFTInfo> cachedResponses = new Dictionary<string, NFTInfo>();

        IEnumerator INFTMarket.FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError)
        {
            string nftId = $"{assetContractAddress}/{tokenId}";
            if (cachedResponses.ContainsKey(nftId))
            {
                onSuccess?.Invoke(cachedResponses[nftId]);
                yield break;
            }

            OpenSeaRequest request = requestController.AddRequest(assetContractAddress, tokenId);
            yield return request.OnResolved(
                (assetResponse) =>
                {
                    NFTInfo nftInfo = ResponseToNFTInfo(assetResponse);
                    if (!cachedResponses.ContainsKey(nftId))
                    {
                        cachedResponses.Add(nftId, nftInfo);
                    }
                    onSuccess?.Invoke(nftInfo);
                },
                (error) =>
                {
                    onError?.Invoke($"{openSeaMarketInfo.name} error fetching {assetContractAddress}/{tokenId} {error}");
                });
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

            OrderInfo sellOrder = GetSellOrder(response.sell_orders, response.owner.address);
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

        private string PriceToFloatingPointString(OpenSea_Internal.AssetSaleInfo saleInfo)
        {
            if (saleInfo.payment_token == null) return null;
            return PriceToFloatingPointString(saleInfo.total_price, saleInfo.payment_token);
        }

        private string PriceToFloatingPointString(string price, OpenSea_Internal.PaymentTokenInfo tokenInfo)
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
            if (orders == null)
                return null;

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
    }
}