using DCL;
using DCL.Controllers;
using DCLPlugins.PreviewModePlugin.Commons;

public class ShapesBoundingBoxDisplayer : IPlugin
{
    private string targetSceneId;
    private IBaseVariable<bool> isDebugEnabled;
    private KernelConfig kernelConfig;
    private WorldRuntimeContext worldRuntime;
    private WatchSceneHandler watchSceneHandler;

    public ShapesBoundingBoxDisplayer() : this(DataStore.i.debugConfig.isDebugMode, KernelConfig.i, Environment.i.world) { }

    internal ShapesBoundingBoxDisplayer(IBaseVariable<bool> debugModeVariable, KernelConfig kernelConfig, WorldRuntimeContext worldRuntime)
    {
        isDebugEnabled = debugModeVariable;
        this.kernelConfig = kernelConfig;
        this.worldRuntime = worldRuntime;

        isDebugEnabled.OnChange += DebugModeVariableOnOnChange;
        kernelConfig.OnChange += KernelConfigOnOnChange;

        DebugModeVariableOnOnChange(isDebugEnabled.Get(), false);
        KernelConfigOnOnChange(kernelConfig.Get(), null);
    }

    public void Dispose()
    {
        isDebugEnabled.OnChange -= DebugModeVariableOnOnChange;
        kernelConfig.OnChange -= KernelConfigOnOnChange;
        worldRuntime.sceneController.OnNewSceneAdded -= SceneControllerOnOnNewSceneAdded;

        KillWatchScene();
    }

    private void KillWatchScene()
    {
        if (watchSceneHandler == null)
        {
            return;
        }

        watchSceneHandler.Dispose();
        watchSceneHandler = null;
    }

    private void WatchScene(string sceneId)
    {
        if (!worldRuntime.state.TryGetScene(sceneId, out IParcelScene scene))
            return;

        WatchScene(scene);
    }

    private void WatchScene(IParcelScene scene)
    {
        if (!isDebugEnabled.Get())
        {
            return;
        }

        watchSceneHandler?.Dispose();
        watchSceneHandler = new WatchSceneHandler(scene, new SceneEntitiesTracker());
    }

    private void DebugModeVariableOnOnChange(bool current, bool previous)
    {
        if (current)
        {
            worldRuntime.sceneController.OnNewSceneAdded += SceneControllerOnOnNewSceneAdded;
            if (!string.IsNullOrEmpty(targetSceneId))
            {
                WatchScene(targetSceneId);
            }
        }
        else
        {
            worldRuntime.sceneController.OnNewSceneAdded -= SceneControllerOnOnNewSceneAdded;
            KillWatchScene();
        }
    }

    private void KernelConfigOnOnChange(KernelConfigModel current, KernelConfigModel previous)
    {
        if (current?.debugConfig != null && current.debugConfig.shapeBoundingBoxDisplaySceneId == targetSceneId)
        {
            return;
        }

        targetSceneId = current.debugConfig.shapeBoundingBoxDisplaySceneId;

        if (!string.IsNullOrEmpty(targetSceneId))
        {
            WatchScene(targetSceneId);
            return;
        }

        KillWatchScene();
    }

    private void SceneControllerOnOnNewSceneAdded(IParcelScene scene)
    {
        if (scene.sceneData.id == targetSceneId)
        {
            WatchScene(scene);
        }
    }
}