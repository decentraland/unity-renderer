using System;
using System.Collections.Generic;
using DCL;
using DCL.Interface;
using UnityEngine;

internal class UnpublishHandler : IDisposable
{
    private event Action<PublishSceneResultPayload> OnUnpublishResult;
    private List<IUnpublishRequester> requesters = new List<IUnpublishRequester>();

    public UnpublishHandler() { DataStore.i.builderInWorld.unpublishSceneResult.OnChange += UnpublishSceneResultOnOnChange; }

    public void Dispose()
    {
        DataStore.i.builderInWorld.unpublishSceneResult.OnChange -= UnpublishSceneResultOnOnChange;

        int count = requesters.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (requesters[i] == null)
                continue;

            RemoveRequester(requesters[i]);
        }
    }

    public void AddListener(IUnpublishListener listener) { OnUnpublishResult += listener.OnUnpublishResult; }

    public void RemoveListener(IUnpublishListener listener) { OnUnpublishResult -= listener.OnUnpublishResult; }

    public void AddRequester(IUnpublishRequester requester)
    {
        if (requesters.Contains(requester))
            return;

        requester.OnRequestUnpublish += OnUnpublishRequested;
        requesters.Add(requester);
    }

    public void RemoveRequester(IUnpublishRequester requester)
    {
        requester.OnRequestUnpublish -= OnUnpublishRequested;
        requesters.Remove(requester);
    }

    private void OnUnpublishRequested(Vector2Int coordinates) { WebInterface.UnpublishScene(coordinates); }

    private void UnpublishSceneResultOnOnChange(PublishSceneResultPayload current, PublishSceneResultPayload previous) { OnUnpublishResult?.Invoke(current); }
}