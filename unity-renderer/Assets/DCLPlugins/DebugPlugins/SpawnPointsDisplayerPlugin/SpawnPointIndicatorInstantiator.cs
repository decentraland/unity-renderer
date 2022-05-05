using UnityEngine;

internal interface ISpawnPointIndicatorInstantiator
{
    ISpawnPointIndicator Instantiate();
}

internal class SpawnPointIndicatorInstantiator : ISpawnPointIndicatorInstantiator
{
    internal const string RESOURCE_PATH = "SpawnPointIndicator";

    private readonly SpawnPointIndicatorMonoBehaviour resource;

    public SpawnPointIndicatorInstantiator()
    {
        resource = Resources.Load<SpawnPointIndicatorMonoBehaviour>(RESOURCE_PATH);
    }

    ISpawnPointIndicator ISpawnPointIndicatorInstantiator.Instantiate()
    {
        SpawnPointIndicatorMonoBehaviour go = Object.Instantiate(resource);
        return new SpawnPointIndicator(go);
    }
}