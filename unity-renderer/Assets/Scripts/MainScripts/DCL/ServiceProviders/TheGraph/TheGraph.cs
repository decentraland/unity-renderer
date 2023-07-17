using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DCL;
using DCL.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class TheGraph : ITheGraph
{
    private const float DEFAULT_CACHE_TIME = 5 * 60;
    private const string LAND_SUBGRAPH_URL_ORG = "https://api.thegraph.com/subgraphs/name/decentraland/land-manager";
    private const string LAND_SUBGRAPH_URL_ZONE = "https://api.thegraph.com/subgraphs/name/decentraland/land-manager-sepolia";
    private const string MANA_SUBGRAPH_URL_ETHEREUM = "https://api.thegraph.com/subgraphs/name/decentraland/mana-ethereum-mainnet";
    private const string MANA_SUBGRAPH_URL_POLYGON = "https://api.thegraph.com/subgraphs/name/decentraland/mana-matic-mainnet";
    private const string NFT_COLLECTIONS_SUBGRAPH_URL_ETHEREUM = "https://api.thegraph.com/subgraphs/name/decentraland/collections-ethereum-mainnet";
    private const string NFT_COLLECTIONS_SUBGRAPH_URL_MATIC = "https://api.thegraph.com/subgraphs/name/decentraland/collections-matic-mainnet";

    private readonly IDataCache<List<Land>> landQueryCache = new DataCache<List<Land>>();

    private Promise<string> Query(string url, string query, QueryVariablesBase variables)
    {
        Promise<string> promise = new Promise<string>();

        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(url))
        {
            promise.Reject($"error: {(string.IsNullOrEmpty(url) ? "url" : "query")} is empty");
            return promise;
        }

        string queryJson = $"\"query\":\"{Regex.Replace(query, @"\p{C}+", string.Empty)}\"";
        string variablesJson = variables != null ? $",\"variables\":{JsonUtility.ToJson(variables)}" : string.Empty;
        string bodyString = $"{{{queryJson}{variablesJson}}}";

        var request = new UnityWebRequest();
        request.url = url;
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(string.IsNullOrEmpty(bodyString) ? null : Encoding.UTF8.GetBytes(bodyString));
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 60;

        var operation = request.SendWebRequest();

        operation.completed += asyncOperation =>
        {
            if (request.WebRequestSucceded())
            {
                promise.Resolve(request.downloadHandler.text);
            }
            else
            {
                promise.Reject($"error: {request.error} response: {request.downloadHandler.text}");
            }

            request.Dispose();
        };

        return promise;
    }

    public Promise<List<Land>> QueryLands(string network, string address, float cacheMaxAgeSeconds)
    {
        string lowerCaseAddress = address.ToLower();

        Promise<List<Land>> promise = new Promise<List<Land>>();

        if (cacheMaxAgeSeconds >= 0)
        {
            if (landQueryCache.TryGet(lowerCaseAddress, out List<Land> cacheValue, out float lastUpdate))
            {
                if (Time.unscaledTime - lastUpdate <= cacheMaxAgeSeconds)
                {
                    promise.Resolve(cacheValue);
                    return promise;
                }
            }
        }

        string url = network == "mainnet" ? LAND_SUBGRAPH_URL_ORG : LAND_SUBGRAPH_URL_ZONE;

        Query(url, TheGraphQueries.getLandQuery, new AddressVariable() { address = lowerCaseAddress })
            .Then(resultJson =>
            {
                ProcessReceivedLandsData(promise, resultJson, lowerCaseAddress, true);
            })
            .Catch(error => promise.Reject(error));

        return promise;
    }

    public Promise<double> QueryMana(string address, TheGraphNetwork network)
    {
        Promise<double> promise = new Promise<double>();

        string lowerCaseAddress = address.ToLower();
        Query(
                network == TheGraphNetwork.Ethereum ? MANA_SUBGRAPH_URL_ETHEREUM : MANA_SUBGRAPH_URL_POLYGON,
                network == TheGraphNetwork.Ethereum ? TheGraphQueries.getEthereumManaQuery : TheGraphQueries.getPolygonManaQuery,
                new AddressVariable() { address = lowerCaseAddress })
           .Then(resultJson =>
            {
                try
                {
                    JObject result = JObject.Parse(resultJson);
                    JToken manaObject = result["data"]?["accounts"].First?["mana"];
                    if (manaObject == null || !double.TryParse(manaObject.Value<string>(), out double parsedMana))
                        throw new Exception($"QueryMana response couldn't be parsed: {resultJson}");

                    promise.Resolve(parsedMana / 1e18);
                }
                catch (Exception e)
                {
                    promise.Reject(e.ToString());
                }
            })
           .Catch(error => promise.Reject(error));
        return promise;
    }

    public Promise<List<Nft>> QueryNftCollections(string address, NftCollectionsLayer layer)
    {
        Promise<List<Nft>> promise = new Promise<List<Nft>>();

        string url = GetSubGraphUrl(layer);

        Query(url, TheGraphQueries.getNftCollectionsQuery, new AddressVariable() { address = address.ToLower() })
            .Then(resultJson =>
            {
                ProcessReceivedNftData(promise, resultJson);
            })
            .Catch(error => promise.Reject(error));

        return promise;
    }

    public Promise<List<Nft>> QueryNftCollectionsByUrn(string address, string urn, NftCollectionsLayer layer)
    {
        Promise<List<Nft>> promise = new Promise<List<Nft>>();

        string url = GetSubGraphUrl(layer);

        Query(url, TheGraphQueries.getNftCollectionByUserAndUrnQuery, new AddressAndUrnVariable() { address = address.ToLower(), urn = urn.ToLower() })
            .Then(resultJson =>
            {
                ProcessReceivedNftData(promise, resultJson);
            })
            .Catch(error => promise.Reject(error));

        return promise;
    }

    public void Dispose() { landQueryCache.Dispose(); }

    private string GetSubGraphUrl(NftCollectionsLayer layer)
    {
        string url = "";
        switch (layer)
        {
            case NftCollectionsLayer.ETHEREUM:
                url = NFT_COLLECTIONS_SUBGRAPH_URL_ETHEREUM;
                break;
            case NftCollectionsLayer.MATIC:
                url = NFT_COLLECTIONS_SUBGRAPH_URL_MATIC;
                break;
        }

        return url;
    }

    private void ProcessReceivedLandsData(Promise<List<Land>> landPromise, string jsonValue, string lowerCaseAddress, bool cache)
    {
        bool hasException = false;
        List<Land> lands = null;

        try
        {
            LandQueryResultWrapped result = JsonUtility.FromJson<LandQueryResultWrapped>(jsonValue);
            lands = LandHelper.ConvertQueryResult(result.data, lowerCaseAddress);

            if (cache)
            {
                landQueryCache.Add(lowerCaseAddress, lands, DEFAULT_CACHE_TIME);
            }
        }
        catch (Exception exception)
        {
            landPromise.Reject(exception.Message);
            hasException = true;
        }
        finally
        {
            if (!hasException)
            {
                landPromise.Resolve(lands);
            }
        }
    }

    private void ProcessReceivedNftData(Promise<List<Nft>> nftPromise, string jsonValue)
    {
        bool hasException = false;
        List<Nft> nfts = new List<Nft>();

        try
        {
            NftQueryResultWrapped result = JsonUtility.FromJson<NftQueryResultWrapped>(jsonValue);

            foreach (var nft in result.data.nfts)
            {
                nfts.Add(new Nft
                {
                    collectionId = nft.collection.id,
                    blockchainId =  nft.item.blockchainId,
                    tokenId = nft.tokenId,
                    urn = nft.urn,
                });
            }
        }
        catch (Exception exception)
        {
            nftPromise.Reject(exception.Message);
            hasException = true;
        }
        finally
        {
            if (!hasException)
            {
                nftPromise.Resolve(nfts);
            }
        }
    }
}
