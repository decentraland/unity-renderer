using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

/// <summary>
/// This class include all the analytics that we are tracking for builder-in-world, if you want to add a new one
/// Please do it inside this class and call the AddLandInfoSendEvent method to include all the default info
/// </summary>
public static class BIWAnalytics
{

    #region BuilderPanel

    public static void PlayerOpenPanel(int landsOwned, int landsOperator)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Lands Owned", landsOwned);
        events.Add("Lands Operator", landsOperator);
        SendEvent("PlayerOpenPanel", events);
    }

    public static void PlayerClosesPanel(int landsOwned, int landsOperator)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Lands Owned", landsOwned);
        events.Add("Lands Operator", landsOperator);
        SendEvent("PlayerClosesPanel", events);
    }

    /// <summary>
    /// When the user interact with the panel to go to a land or a scene
    /// </summary>
    /// <param name="source">From where it is comming: scenes, lands, projects</param>
    /// <param name="mode">The button he has been pressed: Editor, Jump in</param>
    /// <param name="coords">Coords of the land where the scene is deployed or land coords</param>
    /// <param name="ownership">It is owner or operator</param>
    public static void PlayerJumpOrEdit(string source, string mode, Vector2 coords, string ownership)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Source", source);
        events.Add("Mode", mode);
        events.Add("Coords", coords);
        events.Add("Ownership", ownership);
        SendEvent("PlayerJumpOrEdit", events);
    }

    /// <summary>
    /// When the user unpublished a scene
    /// </summary>
    /// <param name="type">What type of scene is unpublished: Builder, Builder-in-world, SDK</param>
    /// <param name="coords">Coords of the land where the scene is deployed </param>
    public static void PlayerUnpublishScene(string type, Vector2Int coords)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Type", type);
        events.Add("Coords", coords);
        SendEvent("PlayerUnpublishScene", events);
    }

    #endregion

    #region BuilderEditor

    /// <summary>
    /// Everytime we start the flow of the editor
    /// </summary>
    /// <param name="source">It comes from the shortcut or from BuilderPanel</param>
    public static void StartEditorFlow(string source)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Source", source);
        AddLandInfoSendEvent("StartEditorFlow", events);
    }

    public static void EnterEditor(float loadingTime)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Loading time", loadingTime);
        AddLandInfoSendEvent("EnterEditor", events);
    }

    public static void ExitEditor(float timeInvestedInTheEditor)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Time in the editor", timeInvestedInTheEditor);
        AddLandInfoSendEvent("ExitEditor", events);
    }

    public static void StartScenePublish(SceneMetricsModel sceneLimits)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Scene Limits", ConvertSceneMetricsModelToDictionary(sceneLimits));
        AddLandInfoSendEvent("StartPublishOfTheScene", events);
    }

    public static void EndScenePublish(SceneMetricsModel sceneLimits, string successOrError, float publicationTime)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Success", successOrError);
        events.Add("Publication Time", publicationTime);
        events.Add("Scene Limits", ConvertSceneMetricsModelToDictionary(sceneLimits));
        AddLandInfoSendEvent("EndScenePublish", events);
    }

    public static void SceneLimitsOverPassed(SceneMetricsModel sceneLimits)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Scene Limits", ConvertSceneMetricsModelToDictionary(sceneLimits));
        AddLandInfoSendEvent("SceneLimitsOverPassed", events);
    }

    /// <summary>
    /// When a new item is placed from the catalog
    /// </summary>
    /// <param name="catalogItem">The item that has been added</param>
    /// <param name="source">It has been added from Categories, Asset pack, Favorites or Quick Access</param>
    public static void NewObjectPlaced(CatalogItem catalogItem, string source)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Name", catalogItem.name);
        events.Add("AssetPack", catalogItem.assetPackName);
        events.Add("Category", catalogItem.category);
        events.Add("Category Name", catalogItem.categoryName);
        events.Add("Source", source);
        events.Add("Type", catalogItem.itemType.ToString());
        AddLandInfoSendEvent("NewObjectPlaced", events);
    }

    public static void QuickAccessAssigned(CatalogItem catalogItem, string source)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Name", catalogItem.name);
        events.Add("AssetPack", catalogItem.assetPackName);
        events.Add("Category", catalogItem.category);
        events.Add("Category Name", catalogItem.categoryName);
        events.Add("Source", source);
        events.Add("Type", catalogItem.itemType.ToString());
        AddLandInfoSendEvent("QuickAccessAssigned", events);
    }

    public static void FavoriteAdded(CatalogItem catalogItem)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Name", catalogItem.name);
        events.Add("AssetPack", catalogItem.assetPackName);
        events.Add("Category", catalogItem.category);
        events.Add("Category Name", catalogItem.categoryName);
        events.Add("Type", catalogItem.itemType.ToString());
        AddLandInfoSendEvent("FavoriteAdded", events);
    }

    public static void CatalogItemSearched(string searchQuery, int resultAmount)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Search Query", searchQuery);
        events.Add("Result Amount", resultAmount);
        AddLandInfoSendEvent("CatalogItemSearched", events);
    }

    private static Dictionary<object, object> ConvertSceneMetricsModelToDictionary(SceneMetricsModel sceneLimits)
    {
        Dictionary<object, object> sceneLimitsDictionary = new Dictionary<object, object>();
        sceneLimitsDictionary.Add("Meshes", sceneLimits.meshes);
        sceneLimitsDictionary.Add("Bodies", sceneLimits.bodies);
        sceneLimitsDictionary.Add("Materials", sceneLimits.materials);
        sceneLimitsDictionary.Add("Textures", sceneLimits.textures);
        sceneLimitsDictionary.Add("Triangles", sceneLimits.triangles);
        sceneLimitsDictionary.Add("Entities", sceneLimits.entities);
        sceneLimitsDictionary.Add("Scene Height", sceneLimits.sceneHeight);

        return sceneLimitsDictionary;
    }

    #endregion

    #region CommonInfo

    private static Vector2Int coords;
    private static string ownership;
    private static Vector2Int size;

    public static void AddSceneInfo(Vector2Int sceneCoords,  string sceneOwnership, Vector2Int sceneSize)
    {
        coords = sceneCoords;
        ownership = sceneOwnership;
        size = sceneSize;
    }

    #endregion

    private static void AddLandInfoSendEvent(string eventName, Dictionary<object, object> events)
    {
        events.Add("Ownership", ownership);
        events.Add("Coords", coords);
        events.Add("Scene size", size);
        SendEvent(eventName, events);
    }

    private static void SendEvent(string eventName, Dictionary<object, object> events)
    {
        //Analytics.i.SendAnalytic(eventName,events);
    }
}