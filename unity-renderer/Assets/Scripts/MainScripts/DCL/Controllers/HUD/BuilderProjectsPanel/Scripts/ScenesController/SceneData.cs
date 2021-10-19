using System;
using DCL.Builder;
using UnityEngine;

internal interface IPlaceData
{
    Vector2Int coords { get; }
    Vector2Int size { get; }
    string id { get; }
    string name { get; }
    string thumbnailUrl { get; }
    bool isOwner { get; }
    bool isOperator { get; }
    bool isContributor { get; }
    bool isDeployed { get; }
    int entitiesCount { get; }
    string authorThumbnail { get; }
    string authorName { get; }
    string[] requiredPermissions { get; }
    bool isMatureContent { get; }
    bool allowVoiceChat { get; }
    string description { get; }
    string[] contributors { get; }
    string[] admins { get;  }
    string[] bannedUsers { get;  }
    bool isEditable { get; }
    Vector2Int[] parcels { get; }
    string projectId { get; }
    bool isEmpty { get; }
    Place.Source source { get; }
}

[Serializable]
internal class PlaceData : IPlaceData
{
    private const string CONTENT_MATURE_SYMBOL = "M";
    private const string CONTENT_ADULTS_ONLY_SYMBOL = "AO";

    public Vector2Int coords;
    public Vector2Int size;
    public string id;
    public string name;
    public string thumbnailUrl;
    public bool isOwner;
    public bool isOperator;
    public bool isContributor;
    public bool isDeployed;
    public int entitiesCount;
    public string authorThumbnail;
    public string authorName;
    public string[] requiredPermissions;
    public bool isMatureContent;
    public bool allowVoiceChat;
    public string description;
    public string[] contributors;
    public string[] admins;
    public string[] bannedUsers;
    public bool isEditable;
    public Vector2Int[] parcels;
    public string projectId;
    public bool isEmpty;
    public Place.Source source;

    Vector2Int IPlaceData.coords => coords;
    Vector2Int IPlaceData.size => size;
    string IPlaceData.id => id;
    string IPlaceData.name => name;
    string IPlaceData.thumbnailUrl => thumbnailUrl;
    bool IPlaceData.isOwner => isOwner;
    bool IPlaceData.isOperator => isOperator;
    bool IPlaceData.isContributor => isContributor;
    bool IPlaceData.isDeployed => isDeployed;
    int IPlaceData.entitiesCount => entitiesCount;
    string IPlaceData.authorThumbnail => authorThumbnail;
    string IPlaceData.authorName => authorName;
    string[] IPlaceData.requiredPermissions => requiredPermissions;
    bool IPlaceData.isMatureContent => isMatureContent;
    bool IPlaceData.allowVoiceChat => allowVoiceChat;
    string IPlaceData.description => description;
    string[] IPlaceData.contributors => contributors;
    string[] IPlaceData.admins => admins;
    string[] IPlaceData.bannedUsers => bannedUsers;
    bool IPlaceData.isEditable => isEditable;
    Vector2Int[] IPlaceData.parcels => parcels;
    string IPlaceData.projectId => projectId;
    bool IPlaceData.isEmpty => isEmpty;
    Place.Source IPlaceData.source => source;

    public PlaceData() { }

    public PlaceData(Place place)
    {
        coords = place.@base;
        id = place.id;
        name = place.title;
        description = place.description;
        thumbnailUrl = place.navmapThumbnail;
        isOwner = place?.land?.role == LandRole.OWNER;
        isOperator = !isOwner;
        isContributor = false;
        isDeployed = true;
        authorName = place.author;
        requiredPermissions = place.requiredPermissions;
        bannedUsers = place.bannedUsers;
        isEditable = place.source != Place.Source.SDK;
        parcels = place.parcels;
        projectId = place.projectId;
        isEmpty = place.isEmpty;
        source = place.source;

        isMatureContent = false;
        if (!string.IsNullOrEmpty(place.contentRating))
        {
            isMatureContent = place.contentRating.Equals(CONTENT_MATURE_SYMBOL, StringComparison.OrdinalIgnoreCase)
                              || place.contentRating.Equals(CONTENT_ADULTS_ONLY_SYMBOL, StringComparison.OrdinalIgnoreCase);
        }

        if (place.parcels.Length < 2)
        {
            size = new Vector2Int(1, 1);
        }
        else
        {
            int minX = place.parcels[0].x;
            int maxX = place.parcels[0].x;
            int minY = place.parcels[0].y;
            int maxY = place.parcels[0].y;

            for (int i = 1; i < place.parcels.Length; i++)
            {
                minX = Mathf.Min(minX, place.parcels[i].x);
                minY = Mathf.Min(minY, place.parcels[i].y);
                maxX = Mathf.Max(maxX, place.parcels[i].x);
                maxY = Mathf.Max(maxY, place.parcels[i].y);
            }

            size = new Vector2Int(Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY));
        }
    }
}