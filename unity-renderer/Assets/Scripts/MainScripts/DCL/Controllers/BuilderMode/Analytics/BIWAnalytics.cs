using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

/// <summary>
/// This class include all the analytics that we are tracking for builder-in-world, if you want to add a new one
/// Please do it inside this class and call the SendEvent method to include all the default info
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
    public static void PlayerUnpublishScene(string type, Vector2 coords)
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
    /// <param name="coords">Coords of the land that we are editing</param>
    /// <param name="ownership">It is owner or operator</param>
    /// <param name="source">It comes from the shortcut or from BuilderPanel</param>
    public static void StartEditor(Vector2 coords,  string ownership, string source)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Ownership", ownership);
        events.Add("Coords", coords);
        events.Add("Source", source);
        SendEvent("StartEditor", events);
    }

    public static void EnterEditor(Vector2 coords,  string ownership, float loadingTime)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Ownership", ownership);
        events.Add("Coords", coords);
        events.Add("Loading time", loadingTime);
        SendEvent("EnterEditor", events);
    }

    public static void ExitEditor(Vector2 coords,  string ownership, float timeInvestedInTheEditor)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Time in the editor", timeInvestedInTheEditor);
        SendEvent("ExitEditor", events);
    }

    public static void StartScenePublish(Vector2 coords, string sceneSize,  string ownership, SceneMetricsModel sceneLimits)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Ownership", ownership);
        events.Add("Coords", coords);
        events.Add("Scene size", sceneSize);
        events.Add("Scene Limits", ConvertSceneMetricsModelToDictionary(sceneLimits));
        SendEvent("StartPublishOfTheScene", events);
    }

    public static void EndScenePublish(Vector2 coords, string sceneSize, string ownership , SceneMetricsModel sceneLimits, string successOrError, float publicationTime)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Ownership", ownership);
        events.Add("Coords", coords);
        events.Add("Scene size", sceneSize);
        events.Add("Success", successOrError);
        events.Add("Publication Time", publicationTime);
        events.Add("Scene Limits", ConvertSceneMetricsModelToDictionary(sceneLimits));
        SendEvent("EndScenePublish", events);
    }

    public static void SceneLimitsOverPassed(string sceneSize, SceneMetricsModel sceneLimits)
    {
        Dictionary<object, object> events = new Dictionary<object, object>();
        events.Add("Scene size", sceneSize);
        events.Add("Scene Limits", ConvertSceneMetricsModelToDictionary(sceneLimits));
        SendEvent("SceneLimitsOverPassed", events);
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

    private static void SendEvent(string eventName, Dictionary<object, object> events) { }
}