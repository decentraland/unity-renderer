using DCL;
using Variables.SpawnPoints;

public class SpawnPointsDisplayerPlugin : IPlugin
{
    private readonly BaseDictionary<string, SceneSpawnPointsData> spawnPointsVariable;
    private ISpawnPointsDataHandler spawnPointsDataHandler;

    public SpawnPointsDisplayerPlugin() : this(
        DataStore.i.debugConfig.showSceneSpawnPoints,
        new SpawnPointsDataHandler(new SpawnPointIndicatorInstantiator())) { }

    internal SpawnPointsDisplayerPlugin(
        in BaseDictionary<string, SceneSpawnPointsData> spawnPointsVariable,
        in ISpawnPointsDataHandler spawnPointsDataHandler)
    {
        this.spawnPointsVariable = spawnPointsVariable;
        this.spawnPointsDataHandler = spawnPointsDataHandler;

        spawnPointsVariable.OnAdded += OnSpawnPointAdded;
        spawnPointsVariable.OnRemoved += OnSpawnPointRemoved;
    }

    public void Dispose()
    {
        spawnPointsVariable.OnAdded -= OnSpawnPointAdded;
        spawnPointsVariable.OnRemoved -= OnSpawnPointRemoved;

        spawnPointsDataHandler?.Dispose();
        spawnPointsDataHandler = null;
    }

    private void OnSpawnPointAdded(string sceneId, SceneSpawnPointsData spawnPointsInfo)
    {
        spawnPointsDataHandler.RemoveSpawnPoints(sceneId);
        if (spawnPointsInfo.enabled.HasValue && spawnPointsInfo.enabled.Value)
        {
            spawnPointsDataHandler.CreateSpawnPoints(sceneId, spawnPointsInfo.spawnPoints);
        }
    }

    private void OnSpawnPointRemoved(string sceneId, SceneSpawnPointsData spawnPointsInfo)
    {
        spawnPointsDataHandler.RemoveSpawnPoints(sceneId);
    }
}