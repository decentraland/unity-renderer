using System;
using System.Linq;
using DCL.Configuration;
using UnityEngine;

namespace DCL.Builder
{
    public class Scene
    {
        public enum Source { BUILDER, SDK }

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
        public string projectId => metadata.source?.projectId;
        public bool isEmpty => metadata.source?.isEmpty ?? false;
        public long deployTimestamp => timestamp;

        private CatalystSceneEntityMetadata metadata;
        internal Source deploymentSource;
        private Vector2Int baseCoord;
        internal Vector2Int[] parcelsCoord;
        private string thumbnail;
        private string entityId;

        internal LandWithAccess sceneLand;
        internal long timestamp;
        internal CatalystEntityContent[] contents;

        public Scene() { }

        public Scene(CatalystSceneEntityPayload pointerData, string contentUrl)
        {

            const string builderSourceName = "builder";

            metadata = pointerData.metadata;
            entityId = pointerData.id;

            deploymentSource = Source.SDK;

            if (metadata.source != null && metadata.source.origin == builderSourceName)
            {
                deploymentSource = Source.BUILDER;
            }

            if (pointerData.content != null)
                contents = pointerData.content;

            baseCoord = StringToVector2Int(metadata.scene.@base);
            parcelsCoord = metadata.scene.parcels.Select(StringToVector2Int).ToArray();
            thumbnail = GetNavmapThumbnailUrl(pointerData, contentUrl);

            timestamp = pointerData.timestamp;
        }

        public bool HasContent(string name)
        {
            foreach (CatalystEntityContent content in contents)
            {
                if (content.file == name)
                    return true;
            }
            return false;
        }

        public bool TryGetHashForContent(string name, out string hash)
        {
            hash = null;
            foreach (CatalystEntityContent content in contents)
            {
                if (content.file == name)
                {
                    hash = content.hash;
                    return true;
                }
            }
            return false;
        }

        public void SetScene(LandWithAccess land) { sceneLand = land; }

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
}