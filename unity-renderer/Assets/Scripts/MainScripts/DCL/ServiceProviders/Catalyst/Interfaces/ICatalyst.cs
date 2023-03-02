using System;
using Cysharp.Threading.Tasks;
using DCL.Helpers;

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
    /// url for lambdas
    /// </summary>
    public string lambdasUrl { get; }

    /// <summary>
    /// This will get the file from the specified hash
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    UniTask<string> GetContent(string hash);

    /// <summary>
    /// get scenes deployed in parcels
    /// </summary>
    /// <param name="parcels">parcels to get scenes from</param>
    /// <returns>promise of an array of scenes entities</returns>
    Promise<CatalystSceneEntityPayload[]> GetDeployedScenes(string[] parcels);

    /// <summary>
    /// get scenes deployed in parcels
    /// </summary>
    /// <param name="parcels">parcels to get scenes from</param>
    /// <param name="cacheMaxAgeSeconds">discard cache if it's older than cacheMaxAgeSeconds ago</param>
    /// <returns>promise of an array of scenes entities</returns>
    Promise<CatalystSceneEntityPayload[]> GetDeployedScenes(string[] parcels, float cacheMaxAgeSeconds);

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
