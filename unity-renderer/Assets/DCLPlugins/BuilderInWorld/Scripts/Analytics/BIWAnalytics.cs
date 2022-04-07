using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// This class include all the analytics that we are tracking for builder-in-world, if you want to add a new one
/// Please do it inside this class and call the SendEditorEvent method to include all the default info
/// </summary>
public static class BIWAnalytics
{
    #region BuilderPanel

    public static void PlayerOpenPanel(int landsOwned, int landsOperator)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("Lands Owned", landsOwned.ToString());
        events.Add("Lands Operator", landsOperator.ToString());
        SendEvent("player_open_panel", events);
    }

    public static void PlayerClosesPanel(int landsOwned, int landsOperator)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("Lands Owned", landsOwned.ToString());
        events.Add("Lands Operator", landsOperator.ToString());
        SendEvent("player_closes_panel", events);
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
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("source", source);
        events.Add("mode", mode);
        events.Add("coords", coords.ToString());
        events.Add("ownership", ownership);
        SendEvent("player_jump_or_edit", events);
    }

    /// <summary>
    /// When the user unpublished a scene
    /// </summary>
    /// <param name="type">What type of scene is unpublished: Builder, Builder-in-world, SDK</param>
    /// <param name="coords">Coords of the land where the scene is deployed </param>
    public static void PlayerUnpublishScene(string type, Vector2Int coords)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("type", type);
        events.Add("coords", coords.ToString());
        SendEvent("player_unpublish_scene", events);
    }

    /// <summary>
    /// When the user has created a new project
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="size"></param>
    public static void CreatedNewProject(string name, string description, Vector2Int size)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("name", name);
        events.Add("description", description);
        events.Add("size", size.ToString());
        SendEvent("created_new_project", events);
    }

    /// <summary>
    /// When a project is deleted
    /// </summary>
    /// <param name="id"></param>
    /// <param name="size"></param>
    public static void ProjectDeleted(string id, Vector2Int size)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("id", id);
        events.Add("size", size.ToString());
        SendEvent("deleted_project", events);
    }
    
    /// <summary>
    /// When a project has been duplicated
    /// </summary>
    /// <param name="id"></param>
    /// <param name="size"></param>
    public static void ProjectDuplicated(string id, Vector2Int size)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("id", id);
        events.Add("size", size.ToString());
        SendEvent("duplicate_project", events);
    }

    #endregion

    #region BuilderEditor

    /// <summary>
    /// Everytime we start the flow of the editor
    /// </summary>
    /// <param name="source">It comes from the shortcut or from BuilderPanel</param>
    public static void StartEditorFlow(string source)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("source", source);
        SendEditorEvent("start_editor_flow", events);
    }

    public static void EnterEditor(float loadingTime)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("loading_time", loadingTime.ToString());
        SendEditorEvent("enter_editor", events);
    }

    public static void ExitEditor(float timeInvestedInTheEditor)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("time_in_the_editor", timeInvestedInTheEditor.ToString());
        SendEditorEvent("exit_editor", events);
    }

    public static void StartScenePublish(SceneMetricsModel sceneLimits)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("scene_limits", JsonConvert.SerializeObject(ConvertSceneMetricsModelToDictionary(sceneLimits)));
        SendEditorEvent("start_publish_of_the_scene", events);
    }

    public static void EndScenePublish(SceneMetricsModel sceneLimits, string successOrError, float publicationTime)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("success", successOrError);
        events.Add("publication_time", publicationTime.ToString());
        events.Add("scene_limits", JsonConvert.SerializeObject(ConvertSceneMetricsModelToDictionary(sceneLimits)));
        SendEditorEvent("end_scene_publish", events);
    }

    public static void SceneLimitsExceeded(SceneMetricsModel sceneUsage, SceneMetricsModel sceneLimits)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("limits_passed", GetLimitsPassedArray(sceneUsage, sceneLimits));
        events.Add("scene_limits", JsonConvert.SerializeObject(ConvertSceneMetricsModelToDictionary(sceneLimits)));
        events.Add("current_usage", JsonConvert.SerializeObject(ConvertSceneMetricsModelToDictionary(sceneUsage)));
        SendEditorEvent("scene_limits_exceeded", events);
    }

    /// <summary>
    /// When a new item is placed from the catalog
    /// </summary>
    /// <param name="catalogItem">The item that has been added</param>
    /// <param name="source">It has been added from Categories, Asset pack, Favorites or Quick Access</param>
    public static void NewObjectPlaced(CatalogItem catalogItem, string source)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("name", catalogItem.name);
        events.Add("assetPack", catalogItem.assetPackName);
        events.Add("category", catalogItem.category);
        events.Add("category_name", catalogItem.categoryName);
        events.Add("source", source);
        events.Add("type", catalogItem.itemType.ToString());
        SendEditorEvent("new_object_placed", events);
    }

    /// <summary>
    /// This will send all the items placed in a period of time in a single message
    /// </summary>
    /// <param name="catalogItem">The item that has been added</param>
    /// <param name="source">It has been added from Categories, Asset pack, Favorites or Quick Access</param>
    public static void NewObjectPlacedChunk(List<KeyValuePair<CatalogItem, string>> itemsToSendAnalytics)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        List<string> items = new List<string>();
        foreach (var catalogItem in itemsToSendAnalytics)
        {
            if (events.ContainsKey(catalogItem.Key.name))
                continue;

            Dictionary<string, string> item = new Dictionary<string, string>();
            int amountOfItems = 0;
            foreach (var itemsToCompare in itemsToSendAnalytics)
            {
                if (catalogItem.Key == itemsToCompare.Key)
                    amountOfItems++;
            }

            item.Add("name", catalogItem.Key.name);
            item.Add("amount", amountOfItems.ToString());
            item.Add("assetPack", catalogItem.Key.assetPackName);
            item.Add("category", catalogItem.Key.category);
            item.Add("category_name", catalogItem.Key.categoryName);
            item.Add("source", catalogItem.Value);
            item.Add("type", catalogItem.Key.ToString());

            items.Add( JsonConvert.SerializeObject(item));
        }

        events.Add("items", JsonConvert.SerializeObject(items));
        SendEditorEvent("new_object_placed", events);
    }

    public static void QuickAccessAssigned(CatalogItem catalogItem, string source)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("name", catalogItem.name);
        events.Add("assetPack", catalogItem.assetPackName);
        events.Add("category", catalogItem.category);
        events.Add("category Name", catalogItem.categoryName);
        events.Add("source", source);
        events.Add("type", catalogItem.itemType.ToString());
        SendEditorEvent("quick_access_assigned", events);
    }

    public static void FavoriteAdded(CatalogItem catalogItem)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("name", catalogItem.name);
        events.Add("assetPack", catalogItem.assetPackName);
        events.Add("category", catalogItem.category);
        events.Add("category Name", catalogItem.categoryName);
        events.Add("type", catalogItem.itemType.ToString());
        SendEditorEvent("favorite_added", events);
    }

    public static void CatalogItemSearched(string searchQuery, int resultAmount)
    {
        Dictionary<string, string> events = new Dictionary<string, string>();
        events.Add("search_query", searchQuery);
        events.Add("result_amount", resultAmount.ToString());
        SendEditorEvent("catalog_item_searched", events);
    }

    private static Dictionary<string, string> ConvertSceneMetricsModelToDictionary(SceneMetricsModel sceneLimits)
    {
        Dictionary<string, string> sceneLimitsDictionary = new Dictionary<string, string>();
        sceneLimitsDictionary.Add("meshes", sceneLimits.meshes.ToString());
        sceneLimitsDictionary.Add("bodies", sceneLimits.bodies.ToString());
        sceneLimitsDictionary.Add("materials", sceneLimits.materials.ToString());
        sceneLimitsDictionary.Add("textures", sceneLimits.textures.ToString());
        sceneLimitsDictionary.Add("triangles", sceneLimits.triangles.ToString());
        sceneLimitsDictionary.Add("entities", sceneLimits.entities.ToString());
        sceneLimitsDictionary.Add("scene_height", sceneLimits.sceneHeight.ToString());

        return sceneLimitsDictionary;
    }

    public static string GetLimitsPassedArray(SceneMetricsModel sceneUsage, SceneMetricsModel sceneLimits)
    {
        string limitsPassed = "[";

        if (sceneUsage.bodies >= sceneLimits.bodies)
            limitsPassed += "bodies,";
        if (sceneUsage.entities >= sceneLimits.entities)
            limitsPassed += "entities,";
        if (sceneUsage.textures >= sceneLimits.textures)
            limitsPassed += "textures,";
        if (sceneUsage.triangles >= sceneLimits.triangles)
            limitsPassed += "triangles,";
        if (sceneUsage.materials >= sceneLimits.materials)
            limitsPassed += "materials,";
        if (sceneUsage.meshes >= sceneLimits.meshes)
            limitsPassed += "meshes,";

        limitsPassed = limitsPassed.Substring(0, limitsPassed.Length - 1);
        limitsPassed += "]";
        return limitsPassed;
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

    private static void SendEditorEvent(string eventName, Dictionary<string, string> events)
    {
        events.Add("ownership", ownership);
        events.Add("coords", coords.ToString());
        events.Add("scene_size", size.ToString());
        SendEvent(eventName, events);
    }

    private static void SendEvent(string eventName, Dictionary<string, string> events)
    {
        IAnalytics analytics = DCL.Environment.i.platform.serviceProviders.analytics;
        analytics.SendAnalytic(eventName, events);
    }
}