using System.Collections.Generic;
using UnityEngine;
using System;

internal class ExploreMiniMapDataController : IDisposable
{
    Dictionary<Vector2Int, PendingData> pendingSceneData = new Dictionary<Vector2Int, PendingData>();

    public ExploreMiniMapDataController()
    {
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated += OnSceneInfoUpdated;
    }

    public void Dispose()
    {
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnSceneInfoUpdated;
        pendingSceneData.Clear();
    }

    public void SetMinimapData(Vector2Int baseCoord, IMapDataView targetView, Action<IMapDataView> onResolvedCallback, Action<IMapDataView> onRejectedCallback)
    {
        if (targetView.HasMinimapSceneInfo())
        {
            onResolvedCallback?.Invoke(targetView);
            return;
        }

        targetView.SetBaseCoord(baseCoord);

        var info = MinimapMetadata.GetMetadata().GetSceneInfo(baseCoord.x, baseCoord.y);

        if (info != null)
        {
            targetView.SetMinimapSceneInfo(info);
            onResolvedCallback?.Invoke(targetView);
        }
        else
        {
            PendingData pending = null;
            if (!pendingSceneData.TryGetValue(baseCoord, out pending))
            {
                pending = new PendingData();
                pendingSceneData.Add(baseCoord, pending);
            }

            pending.AddPending(targetView, onResolvedCallback, onRejectedCallback);
        }
    }

    public void ClearPending()
    {
        using (var iterator = pendingSceneData.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.Clear();
            }
        }

        pendingSceneData.Clear();
    }

    void OnSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo info)
    {
        if (pendingSceneData.Count == 0)
        {
            return;
        }

        Vector2Int? keyToRemove = null;

        using (var iterator = pendingSceneData.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (info.parcels.Contains(iterator.Current.Key))
                {
                    iterator.Current.Value.Resolve(info);
                    keyToRemove = iterator.Current.Key;
                    break;
                }
            }
        }
        if (keyToRemove != null)
        {
            pendingSceneData.Remove(keyToRemove.Value);
        }
    }
}

class PendingData
{
    List<MapDataToResolve> infoToResolve = new List<MapDataToResolve>();

    public void AddPending(IMapDataView view, Action<IMapDataView> onResolveCallback, Action<IMapDataView> onRejectCallback)
    {
        infoToResolve.Add(new MapDataToResolve()
        {
            view = view,
            onResolveCallback = onResolveCallback,
            onRejectCallback = onRejectCallback
        });
    }

    public void Resolve(MinimapMetadata.MinimapSceneInfo info)
    {
        MapDataToResolve toResolve;
        for (int i = 0; i < infoToResolve.Count; i++)
        {
            toResolve = infoToResolve[i];
            toResolve.view.SetMinimapSceneInfo(info);
            toResolve.onResolveCallback?.Invoke(toResolve.view);
        }
        infoToResolve.Clear();
    }

    public void Clear()
    {
        MapDataToResolve toClear;
        for (int i = 0; i < infoToResolve.Count; i++)
        {
            toClear = infoToResolve[i];
            toClear.onRejectCallback?.Invoke(toClear.view);
        }
        infoToResolve.Clear();
    }
}

class MapDataToResolve
{
    public IMapDataView view;
    public Action<IMapDataView> onResolveCallback;
    public Action<IMapDataView> onRejectCallback;
}
