using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Controllers;
using DCLPlugins.DebugPlugins.Commons;

public class DebugShapesBoundingBoxDisplayer : IPlugin
{
    private readonly IBaseDictionary<int, bool> isBoundingBoxEnabledForScene;
    private readonly IWorldState worldState;
    private readonly ISceneController sceneController;
    internal readonly Dictionary<int, WatchSceneHandler> scenesWatcher = new Dictionary<int, WatchSceneHandler>();
    internal readonly List<int> pendingSceneNumbers = new List<int>();
    private readonly IUpdateEventHandler updateEventHandler;

    public DebugShapesBoundingBoxDisplayer() : this(
        DataStore.i.debugConfig.showSceneBoundingBoxes,
        Environment.i.world.state,
        Environment.i.world.sceneController,
        Environment.i.platform.updateEventHandler) { }

    internal DebugShapesBoundingBoxDisplayer(
        IBaseDictionary<int, bool> isBoundingBoxEnabledVariable,
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

    private void KillWatchScene(int sceneNumber)
    {
        if (!scenesWatcher.TryGetValue(sceneNumber, out WatchSceneHandler watchHandler))
            return;

        watchHandler?.Dispose();
        scenesWatcher.Remove(sceneNumber);
    }

    private void WatchScene(int sceneNumber)
    {
        // NOTE: in case scene is not loaded yet, we add it to the "pending" list
        if (!worldState.TryGetScene(sceneNumber, out IParcelScene scene))
        {
            if (!pendingSceneNumbers.Contains(sceneNumber))
                pendingSceneNumbers.Add(sceneNumber);

            return;
        }

        WatchScene(scene);
    }

    private void WatchScene(IParcelScene scene)
    {
        if (scenesWatcher.TryGetValue(scene.sceneData.sceneNumber, out WatchSceneHandler watchHandler))
            watchHandler?.Dispose();

        scenesWatcher[scene.sceneData.sceneNumber] = new WatchSceneHandler(scene, new SceneEntitiesTracker(updateEventHandler));
    }

    private void IsBoundingBoxEnabledVariableOnOnRemoved(int sceneNumber, bool enabled)
    {
        KillWatchScene(sceneNumber);
    }

    private void IsBoundingBoxEnabledVariableOnOnAdded(int sceneNumber, bool enabled)
    {
        if (enabled)
            WatchScene(sceneNumber);
        else
            KillWatchScene(sceneNumber);
    }

    private void SceneControllerOnOnNewSceneAdded(IParcelScene scene)
    {
        if (pendingSceneNumbers.Remove(scene.sceneData.sceneNumber))
            WatchScene(scene);
    }
}