using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using RPC.Context;
using UnityEngine;

public class CrdtExecutorsManager : IDisposable
{
    private readonly Dictionary<string, ICRDTExecutor> crdtExecutors;
    private readonly ISceneController sceneController;
    private readonly IWorldState worldState;
    private readonly ECSComponentsManager componentsManager;
    private readonly CRDTServiceContext rpcCrdtServiceContext;

    private string cachedSceneId;
    private ICRDTExecutor cachedCrdtExecutor;

    public CrdtExecutorsManager(Dictionary<string, ICRDTExecutor> crdtExecutors,
        ECSComponentsManager componentsManager, ISceneController sceneController, IWorldState worldState,
        CRDTServiceContext rpcCrdtServiceContext)
    {
        this.crdtExecutors = crdtExecutors;
        this.sceneController = sceneController;
        this.worldState = worldState;
        this.componentsManager = componentsManager;
        this.rpcCrdtServiceContext = rpcCrdtServiceContext;

        sceneController.OnSceneRemoved += OnSceneRemoved;
        rpcCrdtServiceContext.CrdtMessageReceived += CrdtMessageReceived;
    }

    public void Dispose()
    {
        sceneController.OnSceneRemoved -= OnSceneRemoved;
        rpcCrdtServiceContext.CrdtMessageReceived -= CrdtMessageReceived;

        foreach (ICRDTExecutor crdtExecutor in crdtExecutors.Values)
        {
            crdtExecutor.Dispose();
        }
        crdtExecutors.Clear();
    }

    private void OnSceneRemoved(IParcelScene scene)
    {
        string sceneId = scene.sceneData.id;
        if (crdtExecutors.TryGetValue(sceneId, out ICRDTExecutor crdtExecutor))
        {
            crdtExecutor.Dispose();
            crdtExecutors.Remove(sceneId);
        }

        if (cachedSceneId == sceneId)
        {
            cachedSceneId = null;
        }
    }

    private ICRDTExecutor GetCachedExecutor(string sceneId)
    {
        if (cachedSceneId != sceneId)
        {
            cachedCrdtExecutor = null;
            cachedSceneId = null;
            if (crdtExecutors.TryGetValue(sceneId, out cachedCrdtExecutor))
            {
                cachedSceneId = sceneId;
            }
            else if (worldState.TryGetScene(sceneId, out IParcelScene scene))
            {
                cachedSceneId = sceneId;
                if (scene.crdtExecutor == null)
                {
                    cachedCrdtExecutor = new CRDTExecutor(scene, componentsManager);
                    scene.crdtExecutor = cachedCrdtExecutor;
                    crdtExecutors[sceneId] = cachedCrdtExecutor;
                }
                else
                {
                    cachedCrdtExecutor = scene.crdtExecutor;
                }
            }
        }
        return cachedCrdtExecutor;
    }

    private void CrdtMessageReceived(string sceneId, CRDTMessage crdtMessage)
    {
        ICRDTExecutor executor = GetCachedExecutor(sceneId);

        if (executor != null)
        {
            executor.Execute(crdtMessage);
        }
        else
        {
            Debug.LogError($"CrdtExecutor not found for sceneId {sceneId}");
        }
    }
}