using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Controllers;
using DCLPlugins.DebugPlugins.Commons;

public class DebugShapesCollidersDisplayer : IPlugin
{
    private readonly IBaseDictionary<string, bool> isCollidersDisplayEnabledForScene;
    private readonly WorldRuntimeContext worldRuntime;
    internal readonly Dictionary<string, WatchSceneHandler> scenesWatcher = new Dictionary<string, WatchSceneHandler>();
    internal readonly List<string> pendingScenesId = new List<string>();

    public DebugShapesCollidersDisplayer() : this(
        DataStore.i.debugConfig.showSceneColliders,
        Environment.i.world) { }

    internal DebugShapesCollidersDisplayer(
        IBaseDictionary<string, bool> isCollidersDisplayEnabledVariable,
        WorldRuntimeContext worldRuntime)
    {
        this.isCollidersDisplayEnabledForScene = isCollidersDisplayEnabledVariable;
        this.worldRuntime = worldRuntime;

        // NOTE: we search for scenes that might be added to the variable previous to this class instantiation
        using (var iterator = isCollidersDisplayEnabledVariable.Get().GetEnumerator())
        {
            while (iterator.MoveNext() && iterator.Current.Value)
            {
                IsCollidersDisplayEnabledVariableOnOnAdded(iterator.Current.Key, iterator.Current.Value);
            }
        }

        isCollidersDisplayEnabledVariable.OnAdded += IsCollidersDisplayEnabledVariableOnOnAdded;
        isCollidersDisplayEnabledVariable.OnRemoved += IsBoundingBoxEnabledVariableOnOnRemoved;
        worldRuntime.sceneController.OnNewSceneAdded += SceneControllerOnOnNewSceneAdded;
    }
    public void Dispose()
    {
        isCollidersDisplayEnabledForScene.OnAdded -= IsCollidersDisplayEnabledVariableOnOnAdded;
        isCollidersDisplayEnabledForScene.OnRemoved -= IsBoundingBoxEnabledVariableOnOnRemoved;
        worldRuntime.sceneController.OnNewSceneAdded -= SceneControllerOnOnNewSceneAdded;

        var scenesId = scenesWatcher.Keys.ToArray();
        for (int i = 0; i < scenesId.Length; i++)
        {
            KillWatchScene(scenesId[i]);
        }
    }

    private void KillWatchScene(string sceneId)
    {
        if (!scenesWatcher.TryGetValue(sceneId, out WatchSceneHandler watchHandler))
            return;

        watchHandler?.Dispose();
        scenesWatcher.Remove(sceneId);
    }

    private void WatchScene(string sceneId)
    {
        // NOTE: in case scene is not loaded yet, we add it to the "pending" list
        if (!worldRuntime.state.loadedScenes.TryGetValue(sceneId, out IParcelScene scene))
        {
            if (!pendingScenesId.Contains(sceneId))
            {
                pendingScenesId.Add(sceneId);
            }
            return;
        }

        WatchScene(scene);
    }

    private void WatchScene(IParcelScene scene)
    {
        if (scenesWatcher.TryGetValue(scene.sceneData.id, out WatchSceneHandler watchHandler))
        {
            watchHandler?.Dispose();
        }
        scenesWatcher[scene.sceneData.id] = new WatchSceneHandler(scene, new SceneEntitiesTracker());
    }

    private void IsBoundingBoxEnabledVariableOnOnRemoved(string sceneId, bool enabled)
    {
        KillWatchScene(sceneId);
    }

    private void IsCollidersDisplayEnabledVariableOnOnAdded(string sceneId, bool enabled)
    {
        if (enabled)
        {
            WatchScene(sceneId);
        }
        else
        {
            KillWatchScene(sceneId);
        }
    }

    private void SceneControllerOnOnNewSceneAdded(IParcelScene scene)
    {
        if (pendingScenesId.Remove(scene.sceneData.id))
        {
            WatchScene(scene);
        }
    }
}