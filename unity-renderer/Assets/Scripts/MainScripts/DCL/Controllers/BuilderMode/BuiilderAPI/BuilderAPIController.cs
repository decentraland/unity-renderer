using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;

public class BuilderAPIController : IBuilderAPIController
{
    private BuilderInWorldBridge builderInWorldBridge;

    private SceneObject[] builderAssets;

    private Dictionary<string, Promise<Dictionary<string, string>>> headersRequest = new Dictionary<string, Promise<Dictionary<string, string>>>();

    public void Initialize(IContext context)
    {
        builderInWorldBridge = context.sceneReferences.bridgeGameObject.GetComponent<BuilderInWorldBridge>();
        builderInWorldBridge.OnHeadersReceived += HeadersReceived;
    }

    public void Dispose() { builderInWorldBridge.OnHeadersReceived -= HeadersReceived; }

    private Promise<Dictionary<string, string>> AskHeadersToKernel(string method, string endpoint)
    {
        Promise<Dictionary<string, string>> promise = new Promise<Dictionary<string, string>>();
        headersRequest.Add(endpoint, promise);
        builderInWorldBridge.AskKernelForCatalogHeadersWithParams(method, endpoint);
        return promise;
    }

    internal void HeadersReceived(RequestHeader requestHeader)
    {
        foreach (var request in headersRequest)
        {
            if (request.Key == requestHeader.endpoint)
                request.Value.Resolve(requestHeader.headers);
        }
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