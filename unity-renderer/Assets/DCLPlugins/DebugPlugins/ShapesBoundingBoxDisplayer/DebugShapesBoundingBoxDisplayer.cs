using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Controllers;
using DCLPlugins.DebugPlugins.Commons;

public class DebugShapesBoundingBoxDisplayer : IPlugin
{
    private readonly IBaseDictionary<string, bool> isBoundingBoxEnabledForScene;
    private readonly IWorldState worldState;
    private readonly ISceneController sceneController;
    internal readonly Dictionary<string, WatchSceneHandler> scenesWatcher = new Dictionary<string, WatchSceneHandler>();
    internal readonly List<string> pendingScenesId = new List<string>();
    private readonly IUpdateEventHandler updateEventHandler;

    public DebugShapesBoundingBoxDisplayer() : this(
        DataStore.i.debugConfig.showSceneBoundingBoxes,
        Environment.i.world.state,
        Environment.i.world.sceneController,
        Environment.i.platform.updateEventHandler) { }

    internal DebugShapesBoundingBoxDisplayer(
        IBaseDictionary<string, bool> isBoundingBoxEnabledVariable,
        IWorldState state,
        ISceneController sceneController,
        IUpdateEventHandler updateEventHandler)
    {
        this.isBoundingBoxEnabledForScene = isBoundingBoxEnabledVariable;
        this.worldState = state;
        this.sceneController = sceneController;
        this.updateEventHandler = updateEventHandler;

        // NOTE: we search for scenes that might be added to the variable previous to this class instantiation
        using (var iterator = isBoundingBoxEnabledVariable.Get().GetEnumerator())
        {
            while (iterator.MoveNext() && iterator.Current.Value)
            {
                IsBoundingBoxEnabledVariableOnOnAdded(iterator.Current.Key, iterator.Current.Value);
            }
        }

        isBoundingBoxEnabledVariable.OnAdded += IsBoundingBoxEnabledVariableOnOnAdded;
        isBoundingBoxEnabledVariable.OnRemoved += IsBoundingBoxEnabledVariableOnOnRemoved;
        sceneController.OnNewSceneAdded += SceneControllerOnOnNewSceneAdded;
    }

    public void Dispose()
    {
        isBoundingBoxEnabledForScene.OnAdded -= IsBoundingBoxEnabledVariableOnOnAdded;
        isBoundingBoxEnabledForScene.OnRemoved -= IsBoundingBoxEnabledVariableOnOnRemoved;
        sceneController.OnNewSceneAdded -= SceneControllerOnOnNewSceneAdded;

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
        if (!worldState.TryGetScene(sceneId, out IParcelScene scene))
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

        scenesWatcher[scene.sceneData.id] = new WatchSceneHandler(scene, new SceneEntitiesTracker(updateEventHandler));
    }

    private void IsBoundingBoxEnabledVariableOnOnRemoved(string sceneId, bool enabled)
    {
        KillWatchScene(sceneId);
    }

    private void IsBoundingBoxEnabledVariableOnOnAdded(string sceneId, bool enabled)
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