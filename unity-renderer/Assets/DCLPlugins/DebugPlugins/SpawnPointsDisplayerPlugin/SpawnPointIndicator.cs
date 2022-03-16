using System;
using UnityEngine;
using Object = UnityEngine.Object;

internal interface ISpawnPointIndicator : IDisposable
{
    void SetName(in string name);
    void SetPosition(in Vector3 position);
    void SetSize(in Vector3 size);
    void SetRotation(in Quaternion? rotation);
}

internal class SpawnPointIndicator : ISpawnPointIndicator
{
    private SpawnPointIndicatorMonoBehaviour spawnPointIndicatorBehaviour;

    public SpawnPointIndicator(SpawnPointIndicatorMonoBehaviour spawnPointIndicatorBehaviour)
    {
        this.spawnPointIndicatorBehaviour = spawnPointIndicatorBehaviour;
    }

    void IDisposable.Dispose()
    {
        if (!spawnPointIndicatorBehaviour.isDestroyed)
        {
            Object.Destroy(spawnPointIndicatorBehaviour.gameObject);
        }
    }

    void ISpawnPointIndicator.SetName(in string name)
    {
        spawnPointIndicatorBehaviour.SetName(name);
    }

    void ISpawnPointIndicator.SetPosition(in Vector3 position)
    {
        spawnPointIndicatorBehaviour.SetPosition(position);
    }

    void ISpawnPointIndicator.SetSize(in Vector3 size)
    {
        spawnPointIndicatorBehaviour.SetSize(size);
    }

    void ISpawnPointIndicator.SetRotation(in Quaternion? rotation)
    {
        spawnPointIndicatorBehaviour.SetRotation(rotation);
    }
}