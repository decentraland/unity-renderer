using System.Linq;
using UnityEngine;

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