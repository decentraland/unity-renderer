using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using Variables.RealmsInfo;

/// <summary>
/// GET Entities from connected Catalyst
/// </summary>
public interface ICatalyst : IDisposable
{
    /// <summary>
    /// url for content server
    /// </summary>
    public string contentUrl { get; }
    
    /// <summary>
    /// get scenes deployed in parcels
    /// </summary>
    /// <param name="parcels">parcels to get scenes from</param>
    /// <returns>promise of an array of scenes entities</returns>
    Promise<CatalystSceneEntityPayload[]> GetDeployedScenes(string[] parcels);
    
    /// <summary>
    /// get entities of entityType
    /// </summary>
    /// <param name="entityType">type of the entity to fetch. see CatalystEntitiesType class</param>
    /// <param name="pointers">pointers to fetch</param>
    /// <returns>promise of a string containing catalyst response json</returns>
    Promise<string> GetEntities(string entityType, string[] pointers);
    
    /// <summary>
    /// wraps a WebRequest inside a promise and cache it result
    /// </summary>
    /// <param name="url">url to make the request to</param>
    /// <returns>promise of the server response</returns>
    Promise<string> Get(string url);
}

public class Catalyst : ICatalyst
{
    private const float CACHE_TIME = 5 * 60;
    private const int MAX_POINTERS_PER_REQUEST = 90;

    public string contentUrl => realmContentServerUrl;

    private string realmDomain = "https://peer.decentraland.org";
    private string realmContentServerUrl = "https://peer.decentraland.org/content";

    private readonly Dictionary<string, string> cache = new Dictionary<string, string>();

    public Catalyst()
    {
        if (DataStore.i.playerRealm.Get() != null)
        {
            realmDomain = DataStore.i.playerRealm.Get().domain;
            realmContentServerUrl = DataStore.i.playerRealm.Get().contentServerUrl;
        }
        DataStore.i.playerRealm.OnChange += PlayerRealmOnOnChange;
    }

    public void Dispose()
    {
        DataStore.i.playerRealm.OnChange -= PlayerRealmOnOnChange;
    }

    public Promise<CatalystSceneEntityPayload[]> GetDeployedScenes(string[] parcels)
    {
        var promise = new Promise<CatalystSceneEntityPayload[]>();

        GetEntities(CatalystEntitiesType.SCENE, parcels)
            .Then(json =>
            {
                CatalystSceneEntityPayload[] scenes = null;
                bool hasException = false;
                try
                {
                    CatalystSceneEntityPayload[] parsedValue = Utils.ParseJsonArray<CatalystSceneEntityPayload[]>(json);
                    
                    // remove duplicated 
                    List<CatalystSceneEntityPayload> noDuplicates = new List<CatalystSceneEntityPayload>();
                    for (int i = 0; i < parsedValue.Length; i++)
                    {
                        var sceneToCheck = parsedValue[i];
                        if (noDuplicates.Any(scene => scene.id == sceneToCheck.id))
                            continue;
                        
                        noDuplicates.Add(sceneToCheck);
                    }
                    scenes = noDuplicates.ToArray();
                }
                catch (Exception e)
                {
                    promise.Reject(e.Message);
                    hasException = true;
                }
                finally
                {
                    if (!hasException)
                        promise.Resolve(scenes);
                }
            })
            .Catch(error => promise.Reject(error));

        return promise;
    }
    
    public Promise<string> GetEntities(string entityType, string[] pointers)
    {
        Promise<string> promise = new Promise<string>();
        
        string[][] pointersGroupsToFetch;

        if (pointers.Length <= MAX_POINTERS_PER_REQUEST)
        {
            pointersGroupsToFetch = new [] { pointers };
        }
        else
        {
            // split pointers array in length of MAX_POINTERS_PER_REQUEST
            int i = 0;
            var query = from s in pointers
                let num = i++
                group s by num / MAX_POINTERS_PER_REQUEST
                into g
                select g.ToArray();
            pointersGroupsToFetch = query.ToArray();
        }

        if (pointersGroupsToFetch.Length == 0)
        {
            promise.Reject("error: no pointers to fetch");
            return promise;
        }

        Promise<string>[] splittedPromises = new Promise<string>[pointersGroupsToFetch.Length];
        
        for (int i = 0; i < pointersGroupsToFetch.Length; i++)
        {
            string urlParams = "";
            urlParams = pointersGroupsToFetch[i].Aggregate(urlParams, (current, pointer) => current + $"&pointer={pointer}");
            string url = $"{realmDomain}/content/entities/{entityType}?{urlParams}";
            
            splittedPromises[i] = Get(url);
            splittedPromises[i].Then(value =>
            {
                // check if all other promises have been resolved
                for (int j = 0; j < splittedPromises.Length; j++)
                {
                    if (splittedPromises[j] == null || splittedPromises[j].keepWaiting || !string.IsNullOrEmpty(splittedPromises[j].error))
                    {
                        return;
                    }
                }

                // make sure not to continue if promise was already resolved
                if (!promise.keepWaiting)
                    return;
                
                // build json with all promises result
                string json = splittedPromises[0].value.Substring(1, splittedPromises[0].value.Length - 2);
                for (int j = 1; j < splittedPromises.Length; j++)
                {
                    string jsonContent = splittedPromises[j].value.Substring(1, splittedPromises[j].value.Length - 2);
                    json += $",{jsonContent}";
                }
                promise.Resolve($"[{json}]");
            });
            splittedPromises[i].Catch(error => promise.Reject(error));
        }

        return promise;
    }

    public Promise<string> Get(string url)
    {
        Promise<string> promise = new Promise<string>();

        if (cache.TryGetValue(url, out string cachedResult))
        {
            promise.Resolve(cachedResult);
            return promise;
        }

        WebRequestController.i.Get(url, null ,request =>
        {
            AddToCache(url, request.downloadHandler.text);
            promise.Resolve(request.downloadHandler.text);
        }, request =>
        {
            promise.Reject($"{request.error} {request.downloadHandler.text} at url {url}");
        });

        return promise;
    }

    private void PlayerRealmOnOnChange(CurrentRealmModel current, CurrentRealmModel previous)
    {
        realmDomain = current.domain;
        realmContentServerUrl = current.contentServerUrl;
    }

    private void AddToCache(string url, string result)
    {
        cache[url] = result;

        // NOTE: remove from cache after CACHE_TIME time passed
        CoroutineStarter.Start(RemoveCache(url, CACHE_TIME));
    }

    private IEnumerator RemoveCache(string url, float delay)
    {
        yield return WaitForSecondsCache.Get(delay);
        cache?.Remove(url);
    }
}