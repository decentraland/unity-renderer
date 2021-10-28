using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class BuilderAPIController : IBuilderAPIController
{
    private BuilderInWorldBridge builderInWorldBridge;

    private SceneObject[] builderAssets;

    private Dictionary<string, Promise<RequestHeader>> headersRequests = new Dictionary<string, Promise<RequestHeader>>();

    public void Initialize(IContext context)
    {
        builderInWorldBridge = context.sceneReferences.bridgeGameObject.GetComponent<BuilderInWorldBridge>();
        builderInWorldBridge.OnHeadersReceived += HeadersReceived;
    }

    public void Dispose() { builderInWorldBridge.OnHeadersReceived -= HeadersReceived; }

    internal Promise<RequestHeader> AskHeadersToKernel(string method, string endpoint)
    {
        Promise<RequestHeader> promise = new Promise<RequestHeader>();
        headersRequests.Add(endpoint, promise);
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
        Promise<RequestHeader> headersPromise =AskHeadersToKernel(method, endpoint);
        headersPromise.Then(request =>
        {
            switch (method)
            {
                case "get":
                    Promise<string> resultPromise = new Promise<string>();
                    BIWUtils.MakeGetCall(BIWUrlUtils.GetBuilderAPIBaseUrl()+request.endpoint, resultPromise, request.headers);
                    return resultPromise;
            }
        });
     
        return null;
    }

    public void GetCompleteCatalog(string ethAddres)
    {
        if (catalogAdded)
            return;

        if (areCatalogHeadersReady)
        {
            string ethAddress = "";
            var userProfile = UserProfile.GetOwnUserProfile();
            if (userProfile != null)
                ethAddress = userProfile.ethAddress;
        
            catalogAsyncOp = BIWUtils.MakeGetCall(BIWUrlUtils.GetUrlCatalog(ethAddress), CatalogReceived, catalogCallHeaders);
            catalogAsyncOp = BIWUtils.MakeGetCall(BIWUrlUtils.GetUrlCatalog(""), CatalogReceived, catalogCallHeaders);
        }
        else
        {
            AskHeadersToKernel();
        }

        isCatalogRequested = true;
    }

    public SceneObject[] GetAssets(List<string> assetsIds)
    {
        //TODO: Implement functionality
        return null;
    }

    public List<Manifest> GetAllManifests()
    {
        //TODO: Implement functionality
        return new List<Manifest>();
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
}