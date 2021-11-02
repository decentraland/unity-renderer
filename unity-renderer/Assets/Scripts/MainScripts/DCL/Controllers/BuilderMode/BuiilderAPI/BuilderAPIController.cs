using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DCL;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class BuilderAPIController : IBuilderAPIController
{
    internal const string CATALOG_ENDPOINT = "/assetPacks?owner=";
    internal const string ASSETS_ENDPOINT = "/assets?";
    internal const string PROJECTS_ENDPOINT = "/projects";
        
    internal const string GET = "get";
    
    public event Action<IWebRequestAsyncOperation> OnWebRequestCreated;
    

    private BuilderInWorldBridge builderInWorldBridge;
    
    private Dictionary<string, Promise<RequestHeader>> headersRequests = new Dictionary<string, Promise<RequestHeader>>();

    public void Initialize(IContext context)
    {
        builderInWorldBridge = context.sceneReferences.builderInWorldBridge.GetComponent<BuilderInWorldBridge>();
        if(builderInWorldBridge != null)
            builderInWorldBridge.OnHeadersReceived += HeadersReceived;
    }

    public void Dispose()
    {
        if(builderInWorldBridge != null)
            builderInWorldBridge.OnHeadersReceived -= HeadersReceived;
    }

    internal Promise<RequestHeader> AskHeadersToKernel(string method, string endpoint)
    {
        Promise<RequestHeader> promise = new Promise<RequestHeader>();
        headersRequests.Add(endpoint, promise);
        if(builderInWorldBridge != null)
            builderInWorldBridge.AskKernelForCatalogHeadersWithParams(method, endpoint);
        return promise;
    }

    internal void HeadersReceived(RequestHeader requestHeader)
    {
        string keyToRemove = "";
        foreach (var request in headersRequests)
        {
            if (request.Key == requestHeader.endpoint)
            {
                keyToRemove = request.Key;
                request.Value.Resolve(requestHeader);
            }
        }
        
        if(headersRequests.ContainsKey(keyToRemove))
            headersRequests.Remove(keyToRemove);
    }

    internal Promise<string> CallUrl(string method, string endpoint)
    {
        Promise<string> resultPromise = new Promise<string>();
        Promise<RequestHeader> headersPromise =AskHeadersToKernel(method, endpoint);
        headersPromise.Then(request =>
        {
            switch (method)
            {
                case GET:
                    IWebRequestAsyncOperation webRequest = BIWUtils.MakeGetCall(BIWUrlUtils.GetBuilderAPIBaseUrl()+request.endpoint, resultPromise, request.headers);
                    OnWebRequestCreated?.Invoke(webRequest);
                    break;
            }
        });
     
        return resultPromise;
    }

    public Promise<bool> GetCompleteCatalog(string ethAddres)
    {
        Promise<bool> fullCatalogPromise = new Promise<bool>();

        var promiseDefaultCatalog =  CallUrl(GET, CATALOG_ENDPOINT + "default");
        var promiseOwnedCatalog = CallUrl(GET, CATALOG_ENDPOINT + ethAddres);
        int amountOfCatalogReceived = 0;

        // Note: In order to get the full catalog we need to do 2 calls, the default one and the specific one
        // This is done in order to cache the response in the server 
        promiseDefaultCatalog.Then(catalogJson =>
        {
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(catalogJson);
        
            amountOfCatalogReceived++;
            if (amountOfCatalogReceived >= 2)
                fullCatalogPromise.Resolve(true);
        });

        promiseOwnedCatalog.Then(catalogJson =>
        {
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(catalogJson);

            amountOfCatalogReceived++;
            if (amountOfCatalogReceived >= 2)
                fullCatalogPromise.Resolve(true);
        });
        
        return fullCatalogPromise;
    }

    public Promise<bool> GetAssets(List<string> assetsIds)
    {
        List<string> assetsToAsk = new List<string>();
        foreach (string assetsId in assetsIds)
        {
            if(!AssetCatalogBridge.i.sceneObjectCatalog.ContainsKey(assetsId))
                assetsToAsk.Add(assetsId);
        }
        
        string query = "";
        foreach (string assetsId in assetsToAsk)
        {
            if (string.IsNullOrEmpty(query))
                query += "id=" + assetsId;
            else
                query += "&id=" + assetsId;
        }

        Promise<bool> fullCatalogPromise = new Promise<bool>();

        var promise =  CallUrl(GET, ASSETS_ENDPOINT +query);

        promise.Then(apiResult =>
        {
            var result = GetDataFromCall(apiResult);
            if (!string.IsNullOrEmpty(result))
            {
                AssetCatalogBridge.i.AddScenesObjectToSceneCatalog(result);
                fullCatalogPromise.Resolve(true);
            }
            else
            {
                fullCatalogPromise.Reject("API response is not OK");
            }
        });
        
        return fullCatalogPromise;
    }

    public Promise<List<ProjectData>> GetAllManifests()
    {
        Promise<List<ProjectData>> fullCatalogPromise = new Promise< List<ProjectData>>();

        var promise =  CallUrl(GET, PROJECTS_ENDPOINT);

        promise.Then(result =>
        {
            List<ProjectData> allManifest = JsonConvert.DeserializeObject<List<ProjectData>>(result);
            fullCatalogPromise.Resolve(allManifest);
        });
        return fullCatalogPromise;
    }

    public Manifest GetManifestFromProjectId(string projectId)
    {
        //TODO: Implement functionality
        return null;
    }

    public Manifest GetManifestFromLandCoordinates(string landCoords)
    {
        //TODO: Implement functionality
        return null;
    }

    public void UpdateProjectManifest(Manifest manifest)
    {
        //TODO: Implement functionality
    }

    public void CreateEmptyProjectWithCoords(string coords)
    {
        Manifest manifest = BIWUtils.CreateEmptyDefaultBuilderManifest(coords);
        //TODO: Implement functionality
    }


    private string GetDataFromCall(string result)
    {
        JObject jObject = (JObject)JsonConvert.DeserializeObject(result);
        if (jObject["ok"].ToObject<bool>())
        {
            return jObject["data"].ToString();
        }
        return "";
    }
}