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
    private readonly Dictionary<int, ICRDTExecutor> crdtExecutors;
    private readonly ISceneController sceneController;
    private readonly IWorldState worldState;
    private readonly ECSComponentsManager componentsManager;
    private readonly CRDTServiceContext rpcCrdtServiceContext;

    private int cachedSceneNumber;
    private ICRDTExecutor cachedCrdtExecutor;

    public CrdtExecutorsManager(Dictionary<int, ICRDTExecutor> crdtExecutors,
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
        int sceneNumber = scene.sceneData.sceneNumber;
        if (crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
        {
            crdtExecutor.Dispose();
            crdtExecutors.Remove(sceneNumber);
        }
        
        if (cachedSceneNumber == sceneNumber)
        {
            cachedSceneNumber = -1;
        }
    }

    private ICRDTExecutor GetCachedExecutor(int sceneNumber)
    {
        if (cachedSceneNumber != sceneNumber)
        {
            cachedCrdtExecutor = null;
            cachedSceneNumber = -1;
            if (crdtExecutors.TryGetValue(sceneNumber, out cachedCrdtExecutor))
            {
                cachedSceneNumber = sceneNumber;
            }
            else if (worldState.TryGetScene(sceneNumber, out IParcelScene scene))
            {
                cachedSceneNumber = sceneNumber;
                if (scene.crdtExecutor == null)
                {
                    cachedCrdtExecutor = new CRDTExecutor(scene, componentsManager);
                    scene.crdtExecutor = cachedCrdtExecutor;
                    crdtExecutors[sceneNumber] = cachedCrdtExecutor;
                }
                else
                {
                    cachedCrdtExecutor = scene.crdtExecutor;
                }
            }
        }
        return cachedCrdtExecutor;
    }

    private void CrdtMessageReceived(int sceneNumber, CRDTMessage crdtMessage)
    {
        ICRDTExecutor executor = GetCachedExecutor(sceneNumber);

        if (executor != null)
        {
            executor.Execute(crdtMessage);
        }
        else
        {
            Debug.LogError($"CrdtExecutor not found for sceneNumber {sceneNumber}");
        }
    }
}