using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;

public interface ITheGraph : IDisposable
{
    Promise<string> Query(string url, string query);
    Promise<string> Query(string url, string query, QueryVariablesBase variables);
    Promise<List<Land>> QueryLands(string tld, string address);
    Promise<List<Land>> QueryLands(string tld, string address, float cacheMaxAgeSeconds);
}

public class TheGraph : ITheGraph
{
    private const float DEFAULT_CACHE_TIME = 5 * 60;
    private const string LAND_SUBGRAPH_URL_ORG = "https://api.thegraph.com/subgraphs/name/decentraland/land-manager";
    private const string LAND_SUBGRAPH_URL_ZONE = "https://api.thegraph.com/subgraphs/name/decentraland/land-manager-ropsten";

    private readonly IDataCache<List<Land>> landQueryCache = new DataCache<List<Land>>();

    public Promise<string> Query(string url, string query) { return Query(url, query, null); }

    public Promise<string> Query(string url, string query, QueryVariablesBase variables)
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

    public Promise<List<Land>> QueryLands(string tld, string address) { return QueryLands(tld, address, DEFAULT_CACHE_TIME); }

    public Promise<List<Land>> QueryLands(string tld, string address, float cacheMaxAgeSeconds)
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

        string url = tld == "org" ? LAND_SUBGRAPH_URL_ORG : LAND_SUBGRAPH_URL_ZONE;

        Query(url, TheGraphQueries.getLandQuery, new AddressVariable() { address = lowerCaseAddress })
            .Then(resultJson =>
            {
                ProcessReceivedLandsData(promise, resultJson, lowerCaseAddress, true);
            })
            .Catch(error => promise.Reject(error));

        return promise;
    }

    public void Dispose() { landQueryCache.Dispose(); }

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
}