using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Variables.SpawnPoints;

internal interface ISpawnPointsDataHandler : IDisposable
{
    void CreateSpawnPoints(in string sceneId, in SceneSpawnPoint[] spawnPoints);
    void RemoveSpawnPoints(in string sceneId);
}

internal class SpawnPointsDataHandler : ISpawnPointsDataHandler
{
    internal const string POINT_TYPE_NAME = "SpawnPoint";
    internal const string AREA_TYPE_NAME = "SpawnArea";
    internal const string DEFAULT_NAME = "Unnamed";
    internal const string NAME_DEFAULT_INDICATOR = "*";
    internal const string NAME_FORMAT = "{0}: {1}{2}";

    internal const float SIZE_MIN_XZ = 0.5f;
    internal const float SIZE_MIN_Y = 2.0f;

    internal Dictionary<string, List<ISpawnPointIndicator>> indicatorsBySceneId = new Dictionary<string, List<ISpawnPointIndicator>>();
    internal ISpawnPointIndicatorInstantiator instantiator;

    public SpawnPointsDataHandler(ISpawnPointIndicatorInstantiator instantiator)
    {
        this.instantiator = instantiator;
    }

    void ISpawnPointsDataHandler.CreateSpawnPoints(in string sceneId, in SceneSpawnPoint[] spawnPoints)
    {
        List<ISpawnPointIndicator> sceneSpawnPoints = new List<ISpawnPointIndicator>(spawnPoints.Length);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            var spawnPoint = spawnPoints[i];

            var name = GetName(spawnPoint);
            var size = GetSize(spawnPoint);
            var position = GetPosition(spawnPoint, size);
            var rotation = GetLookAtRotation(spawnPoint, position);

            var indicator = instantiator.Instantiate();
            indicator.SetName(name);
            indicator.SetPosition(position);
            indicator.SetSize(size);
            indicator.SetRotation(rotation);

            sceneSpawnPoints.Add(indicator);
        }
        indicatorsBySceneId.Add(sceneId, sceneSpawnPoints);
    }

    void ISpawnPointsDataHandler.RemoveSpawnPoints(in string sceneId)
    {
        if (DisposeIndicatorsOfSceneId(sceneId, indicatorsBySceneId))
        {
            indicatorsBySceneId.Remove(sceneId);
        }
    }

    void IDisposable.Dispose()
    {
        var sceneIds = indicatorsBySceneId.Keys.ToArray();
        foreach (var sceneId in sceneIds)
        {
            DisposeIndicatorsOfSceneId(sceneId, indicatorsBySceneId);
        }
        indicatorsBySceneId.Clear();
    }

    private static bool DisposeIndicatorsOfSceneId(in string sceneId, in IDictionary<string, List<ISpawnPointIndicator>> indicatorsBySceneId)
    {
        if (!indicatorsBySceneId.TryGetValue(sceneId, out List<ISpawnPointIndicator> indicators))
        {
            return false;
        }
        foreach (var indicator in indicators)
        {
            indicator.Dispose();
        }
        return true;
    }

    internal static string GetName(in SceneSpawnPoint spawnPoint)
    {
        string nameType = IsSpawnArea(spawnPoint) ? AREA_TYPE_NAME : POINT_TYPE_NAME;
        string name = string.IsNullOrEmpty(spawnPoint.name) ? DEFAULT_NAME : spawnPoint.name;
        string defaultIndicator = spawnPoint.@default.HasValue && spawnPoint.@default.Value ? NAME_DEFAULT_INDICATOR : string.Empty;
        return string.Format(NAME_FORMAT, nameType, name, defaultIndicator);
    }

    internal static Vector3 GetSize(in SceneSpawnPoint spawnPoint)
    {
        float x = spawnPoint.position.x.Length > 1 ? (spawnPoint.position.x[1] - spawnPoint.position.x[0]) : 0;
        float y = spawnPoint.position.y.Length > 1 ? (spawnPoint.position.y[1] - spawnPoint.position.y[0]) : 0;
        float z = spawnPoint.position.z.Length > 1 ? (spawnPoint.position.z[1] - spawnPoint.position.z[0]) : 0;

        if (x < SIZE_MIN_XZ)
            x = SIZE_MIN_XZ;

        if (z < SIZE_MIN_XZ)
            z = SIZE_MIN_XZ;

        if (y < SIZE_MIN_Y)
            y = SIZE_MIN_Y;

        return new Vector3(x, y, z);
    }

    internal static Vector3 GetPosition(in SceneSpawnPoint spawnPoint, in Vector3 size)
    {
        float x = spawnPoint.position.x[0] + size.x * 0.5f;
        float y = spawnPoint.position.y[0] + size.y * 0.5f;
        float z = spawnPoint.position.z[0] + size.z * 0.5f;

        return new Vector3(x, y, z);
    }

    internal static Quaternion? GetLookAtRotation(in SceneSpawnPoint spawnPoint, in Vector3 position)
    {
        if (spawnPoint.cameraTarget == null)
        {
            return null;
        }

        return Quaternion.LookRotation(spawnPoint.cameraTarget.Value - position);
    }

    private static bool IsSpawnArea(in SceneSpawnPoint spawnPoint)
    {
        return spawnPoint.position.x.Length > 1 || spawnPoint.position.y.Length > 1 || spawnPoint.position.y.Length > 1;
    }
}