using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Configuration;
using DCL.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class BuilderAPIController : IBuilderAPIController
{
    internal const string API_DATEFORMAT = "yyyy-MM-ddThh:mm:ss.fff%K";

    internal const string CATALOG_ENDPOINT = "/assetPacks";
    internal const string ASSETS_ENDPOINT = "/assets?";
    internal const string GET_PROJECTS_ENDPOINT = "/projects";
    internal const string DELETE_PROJECTS_ENDPOINT = "/projects/{ID}";
    internal const string PROJECT_MANIFEST_ID_ENDPOINT = "/projects/{ID}/manifest";
    internal const string PROJECT_MANIFEST_COORDS_ENDPOINT = "/manifests";
    internal const string PROJECT_THUMBNAIL_ENDPOINT = "/projects/{ID}/media";

    internal const string API_KO_RESPONSE_ERROR = "API response is KO";

    internal const string GET = "get";
    internal const string PUT = "put";
    internal const string POST = "post";
    internal const string DELETE = "delete";

    public event Action<IWebRequestAsyncOperation> OnWebRequestCreated;

    internal IBuilderAPIResponseResolver apiResponseResolver;

    private BuilderInWorldBridge builderInWorldBridge;

    private Dictionary<string, List<Promise<RequestHeader>>> headersRequests = new Dictionary<string, List<Promise<RequestHeader>>>();

    public void Initialize(IContext context)
    {
        apiResponseResolver = new BuilderAPIResponseResolver();
        builderInWorldBridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();
        if (builderInWorldBridge != null)
            builderInWorldBridge.OnHeadersReceived += HeadersReceived;
    }

    public void Dispose()
    {
        if (builderInWorldBridge != null)
            builderInWorldBridge.OnHeadersReceived -= HeadersReceived;
    }

    internal Promise<RequestHeader> AskHeadersToKernel(string method, string endpoint)
    {
        Promise<RequestHeader> promise = new Promise<RequestHeader>();
        if (headersRequests.ContainsKey(endpoint))
            headersRequests[endpoint].Add(promise);
        else
            headersRequests.Add(endpoint, new List<Promise<RequestHeader>>() { promise });

        if (builderInWorldBridge != null)
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
                foreach (Promise<RequestHeader> promise in request.Value)
                {
                    promise.Resolve(requestHeader);
                }
            }
        }

        if (headersRequests.ContainsKey(keyToRemove))
            headersRequests.Remove(keyToRemove);
    }

    internal Promise<string> CallUrl(string method, string endpoint, string callParams = "", byte[] body = null, string contentType = "")
    {
        Promise<string> resultPromise = new Promise<string>();
        Promise<RequestHeader> headersPromise = AskHeadersToKernel(method, endpoint);
        headersPromise.Then(request =>
        {
            switch (method)
            {
                case GET:
                    IWebRequestAsyncOperation webRequest = BIWUtils.MakeGetCall(BIWUrlUtils.GetBuilderAPIBaseUrl() + request.endpoint + callParams, resultPromise, request.headers);
                    OnWebRequestCreated?.Invoke(webRequest);
                    break;
                case PUT:
                    request.body = body;
                    CoroutineStarter.Start(CallWeb(request, resultPromise, contentType, PUT));
                    break;
                case POST:
                    request.body = body;
                    CoroutineStarter.Start(CallWeb(request, resultPromise, contentType, POST));
                    break;
                case DELETE:
                    CoroutineStarter.Start(CallWeb(request, resultPromise, contentType, DELETE));
                    break;
            }
        });

        return resultPromise;
    }

    //This will disappear when we implement the signed fetch call
    IEnumerator CallWeb (RequestHeader requestHeader, Promise<string> resultPromise, string contentType, string method)
    {
        UnityWebRequest www = null;
        switch (method)
        {
            case PUT:
                www = UnityWebRequest.Put (BIWUrlUtils.GetBuilderAPIBaseUrl() + requestHeader.endpoint, requestHeader.body);
                break;
            case POST:
                WWWForm form = new WWWForm();
                form.AddBinaryData("thumbnail", requestHeader.body);
                www = UnityWebRequest.Post(BIWUrlUtils.GetBuilderAPIBaseUrl() + requestHeader.endpoint, form );
                break;
            case DELETE:
                www = UnityWebRequest.Delete(BIWUrlUtils.GetBuilderAPIBaseUrl() + requestHeader.endpoint);
                break;
        }

        if (!string.IsNullOrEmpty(contentType))
            www.SetRequestHeader("Content-Type", contentType);

        foreach (var header in requestHeader.headers)
        {
            www.SetRequestHeader(header.Key, header.Value);
        }

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            resultPromise.Reject(www.error);
        }
        else
        {
            // Note: The builder API doesn't answer with an structure for the DELETE method so we resolve it as an empty string
            if (www.downloadHandler != null)
            {
                byte[] byteArray = www.downloadHandler.data;
                string result = System.Text.Encoding.UTF8.GetString(byteArray);
                resultPromise.Resolve(result);
            }
            else
            {
                resultPromise.Resolve("");
            }
        }

    }

    public Promise<bool> SetManifest(IManifest manifest)
    {
        Promise<bool> fullPromise = new Promise<bool>();

        JsonSerializerSettings dateFormatSettings = new JsonSerializerSettings
        {
            DateFormatString = API_DATEFORMAT,
        };

        string jsonManifest = JsonConvert.SerializeObject(manifest, dateFormatSettings);

        byte[] myData = System.Text.Encoding.UTF8.GetBytes(BIWUrlUtils.GetManifestJSON(jsonManifest));

        string endpoint = PROJECT_MANIFEST_ID_ENDPOINT.Replace("{ID}", manifest.project.id);
        var promise =  CallUrl(PUT, endpoint, "", myData, "application/json");

        promise.Then(result =>
        {
            var apiResponse = apiResponseResolver.GetResponseFromCall(result);
            if (apiResponse.ok)
                fullPromise.Resolve(true);
            else
                fullPromise.Reject(apiResponse.error);
        });

        promise.Catch(error =>
        {
            fullPromise.Reject(error);
        });

        return fullPromise;
    }

    public Promise<bool> SetThumbnail(string id, Texture2D thumbnail)
    {
        Promise<bool> fullPromise = new Promise<bool>();

        byte[] myData = thumbnail.EncodeToPNG();

        string endpoint = PROJECT_THUMBNAIL_ENDPOINT.Replace("{ID}", id);
        var promise =  CallUrl(POST, endpoint, "", myData);

        promise.Then(result =>
        {
            var apiResponse = apiResponseResolver.GetResponseFromCall(result);
            if (apiResponse.ok)
                fullPromise.Resolve(true);
            else
                fullPromise.Reject(apiResponse.error);
        });

        promise.Catch(error =>
        {
            fullPromise.Reject(error);
        });

        return fullPromise;
    }

    public Promise<Manifest> GetManifestById(string idProject)
    {
        Promise<Manifest> fullNewProjectPromise = new Promise<Manifest>();

        string url = PROJECT_MANIFEST_ID_ENDPOINT.Replace("{ID}", idProject);
        var promise =  CallUrl(GET, url);

        promise.Then(result =>
        {
            Manifest manifest = null;

            try
            {
                var x = JObject.Parse(result);

                //We check if the object contains the version attribute, if it contains it it is a manifest
                if (x["version"] != null)
                {
                    manifest = JsonConvert.DeserializeObject<Manifest>(result);
                    fullNewProjectPromise.Resolve(manifest);
                }
                else
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(result);
                    if (!response.ok)
                        fullNewProjectPromise.Reject(BIWSettings.PROJECT_NOT_FOUND);
                }
            }
            catch (Exception e)
            {
                fullNewProjectPromise.Reject(e.Message);
            }
        });
        promise.Catch(error =>
        {
            fullNewProjectPromise.Reject(error);
        });
        return fullNewProjectPromise;
    }

    public Promise<bool> DeleteProject(string id)
    {
        Promise<bool> fullPromise = new Promise<bool>();

        string endpoint = DELETE_PROJECTS_ENDPOINT.Replace("{ID}", id);
        var promise =  CallUrl(DELETE, endpoint);

        promise.Then(result =>
        {
            fullPromise.Resolve(true);
        });

        promise.Catch(error =>
        {
            fullPromise.Reject(error);
        });

        return fullPromise;
    }

    public Promise<Manifest> GetManifestByCoords(string landCoords)
    {
        Promise<Manifest> fullNewProjectPromise = new Promise<Manifest>();

        string callParams =  "?creation_coords_eq=" + landCoords;
        var promise =  CallUrl(GET, PROJECT_MANIFEST_COORDS_ENDPOINT, callParams );

        promise.Then(result =>
        {
            Manifest manifest = null;

            try
            {
                var jsonObject = JObject.Parse(result);

                //We check if the object contains the version attribute, if it contains it it is a manifest
                //The API has two version , one with the APIResponse and one who don't
                if (jsonObject["version"] != null)
                {
                    manifest = JsonConvert.DeserializeObject<Manifest>(result);
                    fullNewProjectPromise.Resolve(manifest);
                }
                // Sometimes the API respond that OK but the data is empty so we need to check that 
                else if (jsonObject["data"].HasValues && jsonObject["data"].First["version"] != null)
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(result);
                    manifest = JsonConvert.DeserializeObject<Manifest[]>(response.data.ToString())[0];
                    fullNewProjectPromise.Resolve(manifest);
                }
                else
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(result);
                    fullNewProjectPromise.Reject(BIWSettings.PROJECT_NOT_FOUND);
                }
            }
            catch (Exception e)
            {
                fullNewProjectPromise.Reject(e.Message);
            }
        });
        promise.Catch(error =>
        {
            fullNewProjectPromise.Reject(error);
        });
        return fullNewProjectPromise;
    }

    public Promise<ProjectData> CreateNewProject(ProjectData newProject)
    {
        Promise<ProjectData> fullNewProjectPromise = new Promise<ProjectData>();
        Manifest builderManifest = BIWUtils.CreateManifestFromProject(newProject);

        JsonSerializerSettings dateFormatSettings = new JsonSerializerSettings
        {
            DateFormatString = API_DATEFORMAT,
        };

        string jsonManifest = JsonConvert.SerializeObject(builderManifest, dateFormatSettings);
        byte[] myData = System.Text.Encoding.UTF8.GetBytes(BIWUrlUtils.GetManifestJSON(jsonManifest));

        string endpoint = PROJECT_MANIFEST_ID_ENDPOINT.Replace("{ID}", newProject.id);
        var promise =  CallUrl(PUT, endpoint, "", myData, "application/json");

        promise.Then(result =>
        {
            var apiResponse = apiResponseResolver.GetResponseFromCall(result);
            if (apiResponse.ok)
                fullNewProjectPromise.Resolve(JsonConvert.DeserializeObject<ProjectData>(apiResponse.data.ToString()));
            else
                fullNewProjectPromise.Reject(apiResponse.error);
        });

        promise.Catch(error =>
        {
            fullNewProjectPromise.Reject(error);
        });

        return fullNewProjectPromise;
    }

    public Promise<bool> GetCompleteCatalog(string ethAddres)
    {
        Promise<bool> fullCatalogPromise = new Promise<bool>();

        var promiseDefaultCatalog =  CallUrl(GET, CATALOG_ENDPOINT, "?owner=default");
        var promiseOwnedCatalog = CallUrl(GET, CATALOG_ENDPOINT, "?owner=" + ethAddres);
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
        promiseDefaultCatalog.Reject("Unable to get default catalog");

        promiseOwnedCatalog.Then(catalogJson =>
        {
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(catalogJson);

            amountOfCatalogReceived++;
            if (amountOfCatalogReceived >= 2)
                fullCatalogPromise.Resolve(true);
        });

        promiseOwnedCatalog.Reject("Unable to get owned catalog");

        return fullCatalogPromise;
    }

    public Promise<bool> GetAssets(List<string> assetsIds)
    {
        List<string> assetsToAsk = new List<string>();
        foreach (string assetsId in assetsIds)
        {
            if (!AssetCatalogBridge.i.sceneObjectCatalog.ContainsKey(assetsId))
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

        var promise =  CallUrl(GET, ASSETS_ENDPOINT , query);

        promise.Then(apiResult =>
        {
            var assets = apiResponseResolver.GetArrayFromCall<SceneObject>(apiResult);
            if (assets != null)
            {
                AssetCatalogBridge.i.AddScenesObjectToSceneCatalog(assets);
                fullCatalogPromise.Resolve(true);
            }
            else
            {
                fullCatalogPromise.Reject(API_KO_RESPONSE_ERROR);
            }
        });

        return fullCatalogPromise;
    }

    public Promise<List<ProjectData>> GetAllProjectsData()
    {
        Promise<List<ProjectData>> fullCatalogPromise = new Promise< List<ProjectData>>();

        var promise =  CallUrl(GET, GET_PROJECTS_ENDPOINT);

        promise.Then(result =>
        {
            List<ProjectData> allManifest = apiResponseResolver.GetArrayFromCall<ProjectData>(result).ToList();
            fullCatalogPromise.Resolve(allManifest);
        });
        promise.Catch(error =>
        {
            fullCatalogPromise.Reject(error);
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
}