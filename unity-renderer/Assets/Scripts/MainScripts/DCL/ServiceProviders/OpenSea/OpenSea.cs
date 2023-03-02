using System;
using System.Collections;
using System.Linq;
using DCL.Helpers.NFT.Markets.OpenSea_Internal;
using UnityEngine;

namespace DCL.Helpers.NFT.Markets
{
    public class OpenSea : INFTMarket
    {
        readonly MarketInfo openSeaMarketInfo = new MarketInfo() { name = "OpenSea" };

        private readonly RequestController requestController = new RequestController();

        IEnumerator INFTMarket.FetchNFTsFromOwner(string address, Action<NFTOwner> onSuccess, Action<string> onError)
        {
            var request = requestController.FetchOwnedNFT(address);

            yield return new UnityEngine.WaitUntil(() => !request.pending);

            if (request.resolved)
            {
                onSuccess?.Invoke( ResponseToNFTOwner(address, request.resolvedValue));
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }

        IEnumerator INFTMarket.FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError)
        {
            RequestBase<AssetResponse> request = requestController.FetchNFT(assetContractAddress, tokenId);

            yield return new UnityEngine.WaitUntil(() => !request.pending);

            if (request.resolved)
            {
                onSuccess?.Invoke( ResponseToNFTInfo(request.resolvedValue));
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }

        IEnumerator INFTMarket.FetchNFTInfoSingleAsset(string assetContractAddress, string tokenId, Action<NFTInfoSingleAsset> onSuccess, Action<string> onError)
        {
            RequestBase<SingleAssetResponse> request = requestController.FetchSingleNFT(assetContractAddress, tokenId);

            yield return new UnityEngine.WaitUntil(() => !request.pending);

            if (request.resolved)
            {
                onSuccess?.Invoke( ResponseToNFTInfo(request.resolvedValue));
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }

        private NFTOwner ResponseToNFTOwner(string ethAddress, AssetsResponse response)
        {
            NFTOwner ownerInfo = NFTOwner.defaultNFTOwner;
            ownerInfo.ethAddress = ethAddress;

            foreach (AssetResponse assetResponse in response.assets)
            {
                ownerInfo.assets.Add(ResponseToNFTInfo(assetResponse));
            }

            return ownerInfo;
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
            ret.imageUrl = response.image_url;
            ret.assetLink = response.external_link;
            ret.marketLink = response.permalink;
            ret.tokenId = response.token_id;

            ret.assetContract.address = response.asset_contract.address;
            ret.assetContract.name = response.asset_contract.name;

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

            OrderInfo sellOrder = GetSellOrder(response.sell_orders, response.top_ownerships);
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

        private NFTInfoSingleAsset ResponseToNFTInfo(SingleAssetResponse response)
        {
            NFTInfoSingleAsset ret = NFTInfoSingleAsset.defaultNFTInfoSingleAsset;
            ret.tokenId = response.token_id;
            ret.marketInfo = openSeaMarketInfo;
            ret.name = response.name;
            ret.description = response.description;
            ret.previewImageUrl = response.image_preview_url;
            ret.originalImageUrl = response.image_original_url;
            ret.assetLink = response.external_link;
            ret.marketLink = response.permalink;

            ret.assetContract.address = response.asset_contract.address;
            ret.assetContract.name = response.asset_contract.name;

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

            OrderInfo sellOrder = GetSellOrder(response.orders, response.top_ownerships);
            if (sellOrder != null)
            {
                ret.currentPrice = PriceToFloatingPointString(sellOrder.current_price, sellOrder.payment_token_contract);
                ret.currentPriceToken = new NFT.PaymentTokenInfo()
                {
                    symbol = sellOrder.payment_token_contract.symbol
                };
            }

            ret.owners = new NFTInfoSingleAsset.Owners[response.top_ownerships.Length];
            for (int i = 0; i < response.top_ownerships.Length; i++)
            {
                ret.owners[i] = new NFTInfoSingleAsset.Owners()
                {
                    owner = response.top_ownerships[i].owner.address,
                };

                float.TryParse(response.top_ownerships[i].quantity, out float quantity);
                ret.owners[i].quantity = quantity;
            }

            return ret;
        }

        private string PriceToFloatingPointString(OpenSea_Internal.AssetSaleInfo saleInfo)
        {
            if (saleInfo.payment_token == null)
                return null;
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

        private OrderInfo GetSellOrder(OrderInfo[] orders, OwnershipInfo[] owners)
        {
            if (orders == null)
                return null;

            for (int i = 0; i < orders.Length; i++)
            {
                if (owners.Any(ownerInfo => orders[i].maker.address == ownerInfo.owner.address))
                {
                    return orders[i];
                }
            }

            return null;
        }

        public void Dispose()
        {
            requestController?.Dispose();
        }
    }
}