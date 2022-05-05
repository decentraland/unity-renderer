using System;
using DCL.Helpers;
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
    private Vector3 position;

    public SpawnPointIndicator(SpawnPointIndicatorMonoBehaviour spawnPointIndicatorBehaviour)
    {
        this.spawnPointIndicatorBehaviour = spawnPointIndicatorBehaviour;
        CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
    }

    void IDisposable.Dispose()
    {
        CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
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
        this.position = position;
        spawnPointIndicatorBehaviour.SetPosition(PositionUtils.WorldToUnityPosition(position));
    }

    void ISpawnPointIndicator.SetSize(in Vector3 size)
    {
        spawnPointIndicatorBehaviour.SetSize(size);
    }

    void ISpawnPointIndicator.SetRotation(in Quaternion? rotation)
    {
        spawnPointIndicatorBehaviour.SetRotation(rotation);
    }

    void OnWorldReposition(Vector3 current, Vector3 previous)
    {
        spawnPointIndicatorBehaviour.SetPosition(PositionUtils.WorldToUnityPosition(position));
    }
}