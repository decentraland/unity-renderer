using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using UnityEngine;
using Variables.RealmsInfo;

public class Catalyst : ICatalyst
{
    private const float DEFAULT_CACHE_TIME = 5 * 60;
    private const int MAX_POINTERS_PER_REQUEST = 90;

    public string contentUrl => realmContentServerUrl;
    public string lambdasUrl => $"{realmDomain}/lambdas";

    private string realmDomain = "https://peer.decentraland.org";
    private string realmContentServerUrl = "https://peer.decentraland.org/content";

    private readonly IDataCache<CatalystSceneEntityPayload[]> deployedScenesCache = new DataCache<CatalystSceneEntityPayload[]>();

    public Catalyst()
    {
        if (DataStore.i.realm.playerRealm.Get() != null)
        {
            realmDomain = DataStore.i.realm.playerRealm.Get().domain;
            realmContentServerUrl = DataStore.i.realm.playerRealm.Get().contentServerUrl;
        }

        DataStore.i.realm.playerRealm.OnChange += PlayerRealmOnOnChange;
    }

    public void Dispose()
    {
        DataStore.i.realm.playerRealm.OnChange -= PlayerRealmOnOnChange;
        deployedScenesCache.Dispose();
    }

    public async UniTask<string> GetContent(string hash)
    {
        string callResult = "";
        string url = $"{realmContentServerUrl}/contents/" + hash;

        var callPromise = Get(url);
        callPromise.Then( result =>
        {
            callResult = result;
        });

        callPromise.Catch( error =>
        {
            callResult = error;
        });
        await callPromise;
        return callResult;
    }

    public Promise<CatalystSceneEntityPayload[]> GetDeployedScenes(string[] parcels) { return GetDeployedScenes(parcels, DEFAULT_CACHE_TIME); }

    public Promise<CatalystSceneEntityPayload[]> GetDeployedScenes(string[] parcels, float cacheMaxAgeSeconds)
    {
        var promise = new Promise<CatalystSceneEntityPayload[]>();

        string cacheKey = string.Join(";", parcels);

        if (cacheMaxAgeSeconds >= 0)
        {
            if (deployedScenesCache.TryGet(cacheKey, out CatalystSceneEntityPayload[] cacheValue, out float lastUpdate))
            {
                if (Time.unscaledTime - lastUpdate <= cacheMaxAgeSeconds)
                {
                    promise.Resolve(cacheValue);
                    return promise;
                }
            }
        }

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
                    {
                        deployedScenesCache.Add(cacheKey, scenes, DEFAULT_CACHE_TIME);
                        promise.Resolve(scenes);
                    }
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
            string url = $"{realmContentServerUrl}/entities/{entityType}?{urlParams}";

            splittedPromises[i] = Get(url);
            splittedPromises[i]
                .Then(value =>
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
                        if (!string.IsNullOrEmpty(jsonContent))
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

        DCL.Environment.i.platform.webRequest.Get(url, null, request =>
        {
            promise.Resolve(request.webRequest.downloadHandler.text);
        }, request =>
        {
            promise.Reject($"{request.webRequest.error} {request.webRequest.downloadHandler.text} at url {url}");
        });

        return promise;
    }

    private void PlayerRealmOnOnChange(CurrentRealmModel current, CurrentRealmModel previous)
    {
        realmDomain = current.domain;
        realmContentServerUrl = current.contentServerUrl;
    }
}