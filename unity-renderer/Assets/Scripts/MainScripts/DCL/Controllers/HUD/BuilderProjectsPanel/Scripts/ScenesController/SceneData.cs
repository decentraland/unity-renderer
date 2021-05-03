using System;
using UnityEngine;

internal interface ISceneData
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
}

[Serializable]
internal class SceneData : ISceneData
{
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

    Vector2Int ISceneData.coords => coords;
    Vector2Int ISceneData.size => size;
    string ISceneData.id => id;
    string ISceneData.name => name;
    string ISceneData.thumbnailUrl => thumbnailUrl;
    bool ISceneData.isOwner => isOwner;
    bool ISceneData.isOperator => isOperator;
    bool ISceneData.isContributor => isContributor;
    bool ISceneData.isDeployed => isDeployed;
    int ISceneData.entitiesCount => entitiesCount;
    string ISceneData.authorThumbnail => authorThumbnail;
    string ISceneData.authorName => authorName;
    string[] ISceneData.requiredPermissions => requiredPermissions;
    bool ISceneData.isMatureContent => isMatureContent;
    bool ISceneData.allowVoiceChat => allowVoiceChat;
    string ISceneData.description => description;
    string[] ISceneData.contributors => contributors;
    string[] ISceneData.admins => admins;
    string[] ISceneData.bannedUsers => bannedUsers;
    bool ISceneData.isEditable => isEditable;
    Vector2Int[] ISceneData.parcels => parcels;
    string ISceneData.projectId => projectId;

    public SceneData() { }

    public SceneData(DeployedScene deployedScene)
    {
        coords = deployedScene.@base;
        id = deployedScene.id;
        name = deployedScene.title;
        description = deployedScene.description;
        thumbnailUrl = deployedScene.navmapThumbnail;
        isOwner = deployedScene?.land.role == LandRole.OWNER;
        isOperator = !isOwner;
        isContributor = false;
        isDeployed = true;
        authorName = deployedScene.author;
        requiredPermissions = deployedScene.requiredPermissions;
        bannedUsers = deployedScene.bannedUsers;
        isEditable = deployedScene.source != DeployedScene.Source.SDK;
        parcels = deployedScene.parcels;
        projectId = deployedScene.projectId;

        isMatureContent = false;
        if (!string.IsNullOrEmpty(deployedScene.contentRating))
        {
            isMatureContent = deployedScene.contentRating.Equals("M", StringComparison.OrdinalIgnoreCase)
                              || deployedScene.contentRating.Equals("AO", StringComparison.OrdinalIgnoreCase);
        }

        if (deployedScene.parcels.Length < 2)
        {
            size = new Vector2Int(1, 1);
        }
        else
        {
            int minX = deployedScene.parcels[0].x;
            int maxX = deployedScene.parcels[0].x;
            int minY = deployedScene.parcels[0].y;
            int maxY = deployedScene.parcels[0].y;
            
            for (int i = 1; i < deployedScene.parcels.Length; i++)
            {
                minX = Mathf.Min(minX, deployedScene.parcels[i].x);
                minY = Mathf.Min(minY, deployedScene.parcels[i].y);
                maxX = Mathf.Max(maxX, deployedScene.parcels[i].x);
                maxY = Mathf.Max(maxY, deployedScene.parcels[i].y);
            }
            
            size = new Vector2Int(Mathf.Abs(maxX-minX), Mathf.Abs(maxY-minY));
        }
    }
}