using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;

public static class DeployedScenesFetcher
{
    public static Promise<DeployedScene[]> FetchScenes(ICatalyst catalyst, string[] parcels)
    {
        Promise<DeployedScene[]> promise = new Promise<DeployedScene[]>();
        catalyst.GetDeployedScenes(parcels)
                .Then(result =>
                {
                    promise.Resolve(result.Select(deployment => new DeployedScene(deployment, catalyst.contentUrl)).ToArray());
                })
                .Catch(err => promise.Reject(err));
        return promise;
    }

    public static Promise<LandWithAccess[]> FetchLandsFromOwner(ICatalyst catalyst, ITheGraph theGraph, string ethAddress, string tld)
    {
        Promise<LandWithAccess[]> resultPromise = new Promise<LandWithAccess[]>();

        List<Land> lands = new List<Land>();

        Promise<string[]> getOwnedParcelsPromise = new Promise<string[]>();
        Promise<DeployedScene[]> getDeployedScenesPromise = new Promise<DeployedScene[]>();

        theGraph.QueryLands(tld, ethAddress, TheGraphCache.UseCache)
                .Then(landsReceived =>
                {
                    lands = landsReceived;

                    List<string> parcels = new List<string>();
                    for (int i = 0; i < landsReceived.Count; i++)
                    {
                        if (landsReceived[i].parcels == null)
                            continue;
                        
                        parcels.AddRange(landsReceived[i].parcels.Select(parcel => $"{parcel.x},{parcel.y}"));
                    }
                    getOwnedParcelsPromise.Resolve(parcels.ToArray());
                })
                .Catch(err => getOwnedParcelsPromise.Reject(err));

        getOwnedParcelsPromise.Then(parcels =>
                              {
                                  if (parcels.Length > 0)
                                  {
                                      FetchScenes(catalyst, parcels)
                                          .Then(scenes => getDeployedScenesPromise.Resolve(scenes))
                                          .Catch(err => getDeployedScenesPromise.Reject(err));
                                  }
                                  else
                                  {
                                      getDeployedScenesPromise.Resolve(new DeployedScene[]{});
                                  }
                              })
                              .Catch(err => getDeployedScenesPromise.Reject(err));

        getDeployedScenesPromise.Then(scenes =>
                                {
                                    resultPromise.Resolve(GetLands(lands, scenes));
                                })
                                .Catch(err => resultPromise.Reject(err));

        return resultPromise;
    }

    private static LandWithAccess[] GetLands(List<Land> lands, DeployedScene[] scenes)
    {
        LandWithAccess[] result = new LandWithAccess[lands.Count];

        for (int i = 0; i < lands.Count; i++)
        {
            result[i] = ProcessLand(lands[i], scenes);
        }

        return result;
    }

    private static LandWithAccess ProcessLand(Land land, DeployedScene[] scenes)
    {
        List<DeployedScene> scenesInLand = new List<DeployedScene>();

        LandWithAccess result = new LandWithAccess(land);
        for (int i = 0; i < result.parcels.Length; i++)
        {
            DeployedScene sceneInParcel = scenes.FirstOrDefault(scene => scene.parcels.Contains(result.parcels[i]) && !scenesInLand.Contains(scene));
            if (sceneInParcel != null)
            {
                sceneInParcel.sceneLand = result;
                scenesInLand.Add(sceneInParcel);
            }
        }
        result.scenes = scenesInLand;

        return result;
    }
}

public class LandWithAccess
{
    public string id => rawData.id;
    public LandType type => rawData.type;
    public LandRole role => rawData.role;
    public int size => rawData.size;
    public string name => rawData.name;
    public string owner => rawData.owner;

    public List<DeployedScene> scenes;
    public Vector2Int[] parcels;
    public Vector2Int @base;
    public Land rawData;

    public LandWithAccess(Land land)
    {
        rawData = land;
        parcels = land.parcels.Select(parcel => new Vector2Int(parcel.x, parcel.y)).ToArray();
        @base = land.type == LandType.PARCEL ? new Vector2Int(land.x, land.y) : parcels[0];
    }
}

public class DeployedScene
{
    public enum Source { BUILDER, BUILDER_IN_WORLD, SDK }

    public string title => metadata.display.title;
    public string description => metadata.display.description;
    public string author => metadata.contact.name;
    public string navmapThumbnail => thumbnail;
    public Vector2Int @base => baseCoord;
    public Vector2Int[] parcels => parcelsCoord;
    public string id => entityId;
    public Source source => deploymentSource;
    public LandWithAccess land => sceneLand;
    public string[] requiredPermissions => metadata.requiredPermissions;
    public string contentRating => metadata.policy?.contentRating;
    public bool voiceEnabled => metadata.policy?.voiceEnabled ?? false;
    public string[] bannedUsers => metadata.policy?.blacklist;
    public string projectId => metadata.source?.projectId;

    private CatalystSceneEntityMetadata metadata;
    private Source deploymentSource;
    private Vector2Int baseCoord;
    private Vector2Int[] parcelsCoord;
    private string thumbnail;
    private string entityId;

    internal LandWithAccess sceneLand;

    public DeployedScene() { }

    public DeployedScene(CatalystSceneEntityPayload pointerData, string contentUrl)
    {
        const string builderInWorldStateJson = "scene-state-definition.json";
        const string builderSourceName = "builder";

        metadata = pointerData.metadata;
        entityId = pointerData.id;

        deploymentSource = Source.SDK;

        if (pointerData.content != null && pointerData.content.Any(content => content.file == builderInWorldStateJson))
        {
            deploymentSource = Source.BUILDER_IN_WORLD;
        }
        else if (metadata.source != null && metadata.source.origin == builderSourceName)
        {
            deploymentSource = Source.BUILDER;
        }

        baseCoord = StringToVector2Int(metadata.scene.@base);
        parcelsCoord = metadata.scene.parcels.Select(StringToVector2Int).ToArray();
        thumbnail = GetNavmapThumbnailUrl(pointerData, contentUrl);
    }

    static Vector2Int StringToVector2Int(string coords)
    {
        string[] coordSplit = coords.Split(',');
        if (coordSplit.Length == 2 && int.TryParse(coordSplit[0], out int x) && int.TryParse(coordSplit[1], out int y))
        {
            return new Vector2Int(x, y);
        }
        return Vector2Int.zero;
    }

    static string GetNavmapThumbnailUrl(CatalystSceneEntityPayload pointerData, string contentUrl)
    {
        const string contentDownloadUrlFormat = "{0}/contents/{1}";
        const string builderUrlFormat = "https://builder-api.decentraland.org/v1/projects/{0}/media/preview.png";

        string thumbnail = pointerData.metadata.display.navmapThumbnail;

        bool isThumbnailPresent = !string.IsNullOrEmpty(thumbnail);
        bool isThumbnailFileDeployed = isThumbnailPresent && !thumbnail.StartsWith("http");

        if (isThumbnailPresent && !isThumbnailFileDeployed)
        {
            return thumbnail;
        }

        if (isThumbnailFileDeployed && pointerData.content != null)
        {
            string thumbnailHash = pointerData.content.FirstOrDefault(content => content.file == thumbnail)?.hash;
            if (!string.IsNullOrEmpty(thumbnailHash))
            {
                return string.Format(contentDownloadUrlFormat, contentUrl, thumbnailHash);
            }
        }

        if (pointerData.metadata.source != null && !string.IsNullOrEmpty(pointerData.metadata.source.projectId))
        {
            return string.Format(builderUrlFormat, pointerData.metadata.source.projectId);
        }

        return thumbnail;
    }
}